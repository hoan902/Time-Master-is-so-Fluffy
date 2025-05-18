using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;

namespace EncryptedAsset.Runtime
{
    public class EncryptedBundleResource : IAssetBundleResource, IUpdateReceiver
    {
        private static Dictionary<string, AssetBundle> m_bundlesCache = new Dictionary<string, AssetBundle>();//<size, bundle>

        public static void ClearLevelBundle(string levelName)
        {
            List<string> keys = new List<string>();
            foreach (string url in m_bundlesCache.Keys)
            {
                string bundleName = Path.GetFileNameWithoutExtension(url);
                if(bundleName.Length < levelName.Length)
                    continue;
                bundleName = bundleName.Substring(bundleName.Length - levelName.Length, levelName.Length);
                if (bundleName.Equals(levelName))
                {
                    keys.Add(url);
                    break;
                }
            }
            foreach (string url in keys)
            {
                m_bundlesCache[url].Unload(true);
                m_bundlesCache.Remove(url);
            }
        }

        internal enum LoadType
        {
            None,
            Local,
            Web
        }

        private string m_bundleUrl = "";
        AssetBundle m_AssetBundle;
        DownloadHandler m_downloadHandler;
        AsyncOperation m_RequestOperation;
        WebRequestQueueOperation m_WebRequestQueueOperation;
        internal ProvideHandle m_ProvideHandle;
        internal AssetBundleRequestOptions m_Options;
        [NonSerialized] bool m_WebRequestCompletedCallbackCalled = false;
        int m_Retries;
        long m_BytesToDownload;
        long m_DownloadedBytes;
        bool m_Completed = false;
#if UNLOAD_BUNDLE_ASYNC
        AssetBundleUnloadOperation m_UnloadOperation;
#endif
        const int k_WaitForWebRequestMainThreadSleep = 1;
        string m_TransformedInternalId;
        AssetBundleRequest m_PreloadRequest;
        bool m_PreloadCompleted = false;
        ulong m_LastDownloadedByteCount = 0;
        float m_TimeoutTimer = 0;
        int m_TimeoutOverFrames = 0;

        private bool HasTimedOut => m_TimeoutTimer >= m_Options.Timeout && m_TimeoutOverFrames > 5;

        internal long BytesToDownload
        {
            get
            {
                if (m_BytesToDownload == -1)
                {
                    if (m_Options != null)
                        m_BytesToDownload =
                            m_Options.ComputeSize(m_ProvideHandle.Location, m_ProvideHandle.ResourceManager);
                    else
                        m_BytesToDownload = 0;
                }

                return m_BytesToDownload;
            }
        }

        internal UnityWebRequest CreateWebRequest(IResourceLocation loc)
        {
            var url = m_ProvideHandle.ResourceManager.TransformInternalId(loc);
            return CreateWebRequest(url);
        }

        internal UnityWebRequest CreateWebRequest(string url)
        {
            return UnityWebRequest.Get(url);
        }

        float PercentComplete()
        {
            return m_RequestOperation != null ? m_RequestOperation.progress : 0.0f;
        }

        DownloadStatus GetDownloadStatus()
        {
            if (m_Options == null)
                return default;
            var status = new DownloadStatus() { TotalBytes = BytesToDownload, IsDone = PercentComplete() >= 1f };
            if (BytesToDownload > 0)
            {
                if (m_WebRequestQueueOperation != null &&
                    string.IsNullOrEmpty(m_WebRequestQueueOperation.m_WebRequest.error))
                    m_DownloadedBytes = (long)(m_WebRequestQueueOperation.m_WebRequest.downloadedBytes);
                else if (m_RequestOperation != null && m_RequestOperation is UnityWebRequestAsyncOperation operation &&
                         string.IsNullOrEmpty(operation.webRequest.error))
                    m_DownloadedBytes = (long)operation.webRequest.downloadedBytes;
            }

            status.DownloadedBytes = m_DownloadedBytes;
            return status;
        }

        /// <summary>
        /// Get the asset bundle object managed by this resource.  This call may force the bundle to load if not already loaded.
        /// </summary>
        /// <returns>The asset bundle.</returns>
        public AssetBundle GetAssetBundle()
        {
            if (m_AssetBundle == null)
            {
                if (m_downloadHandler != null && m_downloadHandler.data != null)
                {
                     string key = m_bundleUrl;
                     if (m_bundlesCache.ContainsKey(key))
                         m_AssetBundle = m_bundlesCache[key];
                     if (m_AssetBundle == null)
                     {
                         m_bundlesCache.Remove(key);
                         byte[] bytes = EncryptedBundleUtils.DecryptXOR(m_downloadHandler.data);
                         m_AssetBundle = AssetBundle.LoadFromMemory(bytes, m_Options?.Crc ?? 0);
                         if(m_AssetBundle != null)
                             m_bundlesCache.Add(key, m_AssetBundle);
                     }
                     m_downloadHandler.Dispose();
                     m_downloadHandler = null;
                }
                else if (m_RequestOperation != null)
                {
                    m_AssetBundle = (m_RequestOperation as AssetBundleCreateRequest)?.assetBundle;
                }
            }

            return m_AssetBundle;
        }
#if UNLOAD_BUNDLE_ASYNC
        void OnUnloadOperationComplete(AsyncOperation op)
        {
            m_UnloadOperation = null;
            BeginOperation();
        }
#endif

#if UNLOAD_BUNDLE_ASYNC
        internal void Start(ProvideHandle provideHandle, AssetBundleUnloadOperation unloadOp)
#else
        internal void Start(ProvideHandle provideHandle)
#endif
        {
            m_Retries = 0;
            m_AssetBundle = null;
            m_downloadHandler = null;
            m_RequestOperation = null;
            m_WebRequestCompletedCallbackCalled = false;
            m_ProvideHandle = provideHandle;
            m_Options = m_ProvideHandle.Location.Data as AssetBundleRequestOptions;
            m_BytesToDownload = -1;
            m_ProvideHandle.SetProgressCallback(PercentComplete);
            m_ProvideHandle.SetDownloadProgressCallbacks(GetDownloadStatus);
            m_ProvideHandle.SetWaitForCompletionCallback(WaitForCompletionHandler);
#if UNLOAD_BUNDLE_ASYNC
            m_UnloadOperation = unloadOp;
            if (m_UnloadOperation != null && !m_UnloadOperation.isDone)
                m_UnloadOperation.completed += OnUnloadOperationComplete;
            else
#endif
            BeginOperation();
        }

        private bool WaitForCompletionHandler()
        {
#if UNLOAD_BUNDLE_ASYNC
            if (m_UnloadOperation != null && !m_UnloadOperation.isDone)
            {
                m_UnloadOperation.completed -= OnUnloadOperationComplete;
                m_UnloadOperation.WaitForCompletion();
                m_UnloadOperation = null;
                BeginOperation();
            }
#endif

            if (m_RequestOperation == null)
            {
                if (m_WebRequestQueueOperation == null)
                    return false;
                else
                    WebRequestQueue.WaitForRequestToBeActive(m_WebRequestQueueOperation,
                        k_WaitForWebRequestMainThreadSleep);
            }

            //We don't want to wait for request op to complete if it's a LoadFromFileAsync. Only UWR will complete in a tight loop like this.
            if (m_RequestOperation is UnityWebRequestAsyncOperation op)
            {
                while (!UnityWebRequestUtilities.IsAssetBundleDownloaded(op))
                    Thread.Sleep(k_WaitForWebRequestMainThreadSleep);
            }

            if (m_RequestOperation is UnityWebRequestAsyncOperation && !m_WebRequestCompletedCallbackCalled)
            {
                WebRequestOperationCompleted(m_RequestOperation);
                m_RequestOperation.completed -= WebRequestOperationCompleted;
            }

            var assetBundle = GetAssetBundle();
            if (!m_Completed && m_RequestOperation.isDone)
            {
                m_ProvideHandle.Complete(this, m_AssetBundle != null, null);
                m_Completed = true;
            }

            return m_Completed;
        }

        void AddCallbackInvokeIfDone(AsyncOperation operation, Action<AsyncOperation> callback)
        {
            if (operation.isDone)
                callback(operation);
            else
                operation.completed += callback;
        }

        internal static void GetLoadInfo(ProvideHandle handle, out LoadType loadType, out string path)
        {
            GetLoadInfo(handle.Location, handle.ResourceManager, out loadType, out path);
        }

        internal static void GetLoadInfo(IResourceLocation location, ResourceManager resourceManager,
            out LoadType loadType, out string path)
        {
            var options = location?.Data as AssetBundleRequestOptions;
            if (options == null)
            {
                loadType = LoadType.None;
                path = null;
                return;
            }

            path = resourceManager.TransformInternalId(location);
            if (Application.platform == RuntimePlatform.Android && path.StartsWith("jar:"))
            {
                
            }
            else if (ResourceManagerConfig.ShouldPathUseWebRequest(path))
            {
                
            }
            else if (options.UseUnityWebRequestForLocalBundles)
                path = "file:///" + Path.GetFullPath(path);
            loadType = LoadType.Web;
        }

        private void BeginOperation()
        {
            m_DownloadedBytes = 0;
            GetLoadInfo(m_ProvideHandle, out LoadType loadType, out m_TransformedInternalId);
            if (loadType == LoadType.Web)
            {
                m_WebRequestCompletedCallbackCalled = false;
                var req = CreateWebRequest(m_TransformedInternalId);
#if ENABLE_ASYNC_ASSETBUNDLE_UWR
                ((DownloadHandlerAssetBundle)req.downloadHandler).autoLoadAssetBundle = !(m_ProvideHandle.Location is DownloadOnlyLocation);
#endif
                req.disposeDownloadHandlerOnDispose = false;

                m_WebRequestQueueOperation = WebRequestQueue.QueueRequest(req);
                if (m_WebRequestQueueOperation.IsDone)
                    BeginWebRequestOperation(m_WebRequestQueueOperation.Result);
                else
                    m_WebRequestQueueOperation.OnComplete += asyncOp => BeginWebRequestOperation(asyncOp);
            }
            else
            {
                m_RequestOperation = null;
                m_ProvideHandle.Complete<EncryptedBundleResource>(null, false,
                    new RemoteProviderException(
                        string.Format("Invalid path in AssetBundleProvider: '{0}'.", m_TransformedInternalId),
                        m_ProvideHandle.Location));
                m_Completed = true;
            }
        }

        private void BeginWebRequestOperation(AsyncOperation asyncOp)
        {
            m_TimeoutTimer = 0;
            m_TimeoutOverFrames = 0;
            m_LastDownloadedByteCount = 0;
            m_RequestOperation = asyncOp;
            if (m_RequestOperation == null || m_RequestOperation.isDone)
                WebRequestOperationCompleted(m_RequestOperation);
            else
            {
                if (m_Options.Timeout > 0)
                    m_ProvideHandle.ResourceManager.AddUpdateReceiver(this);
                m_RequestOperation.completed += WebRequestOperationCompleted;
            }
        }

        public void Update(float unscaledDeltaTime)
        {
            if (m_RequestOperation != null && m_RequestOperation is UnityWebRequestAsyncOperation operation &&
                !operation.isDone)
            {
                if (m_LastDownloadedByteCount != operation.webRequest.downloadedBytes)
                {
                    m_TimeoutTimer = 0;
                    m_TimeoutOverFrames = 0;
                    m_LastDownloadedByteCount = operation.webRequest.downloadedBytes;
                }
                else
                {
                    m_TimeoutTimer += unscaledDeltaTime;
                    if (HasTimedOut)
                        operation.webRequest.Abort();
                    m_TimeoutOverFrames++;
                }
            }
        }

        private void LocalRequestOperationCompleted(AsyncOperation op)
        {
            CompleteBundleLoad((op as AssetBundleCreateRequest).assetBundle);
        }

        private void CompleteBundleLoad(AssetBundle bundle)
        {
            m_AssetBundle = bundle;
            if (m_AssetBundle != null)
                m_ProvideHandle.Complete(this, true, null);
            else
                m_ProvideHandle.Complete<EncryptedBundleResource>(null, false,
                    new RemoteProviderException(
                        string.Format("Invalid path in AssetBundleProvider: '{0}'.", m_TransformedInternalId),
                        m_ProvideHandle.Location));
            m_Completed = true;
        }

        private void WebRequestOperationCompleted(AsyncOperation op)
        {
            if (m_WebRequestCompletedCallbackCalled)
                return;

            if (m_Options.Timeout > 0)
                m_ProvideHandle.ResourceManager.RemoveUpdateReciever(this);

            m_WebRequestCompletedCallbackCalled = true;
            UnityWebRequestAsyncOperation remoteReq = op as UnityWebRequestAsyncOperation;
            UnityWebRequest webReq = remoteReq?.webRequest;
            m_bundleUrl = webReq?.url;
            m_downloadHandler = webReq?.downloadHandler;
            UnityWebRequestResult uwrResult = null;
            if (webReq != null && !UnityWebRequestUtilities.RequestHasErrors(webReq, out uwrResult))
            {
                if (!m_Completed)
                {
                    m_ProvideHandle.Complete(this, true, null);
                    m_Completed = true;
                }
#if ENABLE_CACHING
                if (!string.IsNullOrEmpty(m_Options.Hash) && m_Options.ClearOtherCachedVersionsWhenLoaded)
                    Caching.ClearOtherCachedVersions(m_Options.BundleName, Hash128.Parse(m_Options.Hash));
#endif
            }
            else
            {
                if (HasTimedOut)
                    uwrResult.Error = "Request timeout";
                webReq = m_WebRequestQueueOperation.m_WebRequest;
                if (uwrResult == null)
                    uwrResult = new UnityWebRequestResult(m_WebRequestQueueOperation.m_WebRequest);

                m_downloadHandler = webReq.downloadHandler;
                m_downloadHandler.Dispose();
                m_downloadHandler = null;
                bool forcedRetry = false;
                string message = $"Web request failed, retrying ({m_Retries}/{m_Options.RetryCount})...\n{uwrResult}";
#if ENABLE_CACHING
                if (!string.IsNullOrEmpty(m_Options.Hash))
                {
                    CachedAssetBundle cab = new CachedAssetBundle(m_Options.BundleName, Hash128.Parse(m_Options.Hash));
                    if (Caching.IsVersionCached(cab))
                    {
                        message =
                            $"Web request failed to load from cache. The cached AssetBundle will be cleared from the cache and re-downloaded. Retrying...\n{uwrResult}";
                        Caching.ClearCachedVersion(cab.name, cab.hash);
                        if (m_Options.RetryCount == 0 && m_Retries == 0)
                        {
                            Debug.LogFormat(message);
                            BeginOperation();
                            m_Retries++; //Will prevent us from entering an infinite loop of retrying if retry count is 0
                            forcedRetry = true;
                        }
                    }
                }
#endif
                if (!forcedRetry)
                {
                    if (m_Retries < m_Options.RetryCount && uwrResult.ShouldRetryDownloadError())
                    {
                        m_Retries++;
                        Debug.LogFormat(message);
                        BeginOperation();
                    }
                    else
                    {
                        var exception = new RemoteProviderException($"Unable to load asset bundle from : {webReq.url}",
                            m_ProvideHandle.Location, uwrResult);
                        m_ProvideHandle.Complete<EncryptedBundleResource>(null, false, exception);
                        m_Completed = true;
                    }
                }
            }

            webReq.Dispose();
        }

        /// <summary>
        /// Unloads all resources associated with this asset bundle.
        /// </summary>
#if UNLOAD_BUNDLE_ASYNC
        public bool Unload(out AssetBundleUnloadOperation unloadOp)
#else
        public void Unload()
#endif
        {
#if UNLOAD_BUNDLE_ASYNC
            unloadOp = null;
            if (m_AssetBundle != null)
            {
                unloadOp = m_AssetBundle.UnloadAsync(true);
                m_AssetBundle = null;
            }
#else
            if (m_AssetBundle != null)
            {
                m_AssetBundle.Unload(true);
                m_AssetBundle = null;
            }
#endif
            if (m_downloadHandler != null)
            {
                m_downloadHandler.Dispose();
                m_downloadHandler = null;
            }

            m_RequestOperation = null;
#if UNLOAD_BUNDLE_ASYNC
            return unloadOp != null;
#endif
        }
    }
}