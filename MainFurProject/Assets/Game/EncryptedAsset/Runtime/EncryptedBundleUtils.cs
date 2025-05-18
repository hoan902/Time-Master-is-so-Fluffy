using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace EncryptedAsset.Runtime
{
    internal static class EncryptedBundleUtils
    {
        internal static byte[] DecryptXOR(byte[] data)
        {
            string key = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(Application.identifier))).Replace("-", "");
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            int keyLength = keyBytes.Length;

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ keyBytes[i % keyLength]);
            }
            return data;
        }
    } 
    
    public class UnityWebRequestResult
    {
        /// <summary>
        /// Creates a new instance of <see cref="UnityWebRequestResult"/>.
        /// </summary>
        /// <param name="request">The unity web request.</param>
        public UnityWebRequestResult(UnityWebRequest request)
        {
            string error = request.error;
#if UNITY_2020_1_OR_NEWER
            if (request.result == UnityWebRequest.Result.DataProcessingError && request.downloadHandler != null)
            {
                // https://docs.unity3d.com/ScriptReference/Networking.DownloadHandler-error.html
                // When a UnityWebRequest ends with the result, UnityWebRequest.Result.DataProcessingError, the message describing the error is in the download handler
                error = $"{error} : {request.downloadHandler.error}";
            }

            Result = request.result;
#endif
            Error = error;
            ResponseCode = request.responseCode;
            Method = request.method;
            Url = request.url;
        }

        /// <summary>Provides a new string object describing the result.</summary>
        /// <returns>A newly allocated managed string.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

#if UNITY_2020_1_OR_NEWER
            sb.AppendLine($"{Result} : {Error}");
#else
            if (!string.IsNullOrEmpty(Error))
                sb.AppendLine(Error);
#endif
            if (ResponseCode > 0)
                sb.AppendLine($"ResponseCode : {ResponseCode}, Method : {Method}");
            sb.AppendLine($"url : {Url}");

            return sb.ToString();
        }

        /// <summary>
        /// A string explaining the error that occured.
        /// </summary>
        public string Error { get; internal set; }

        /// <summary>
        /// The numeric HTTP response code returned by the server, if any.
        /// See <a href="https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest-responseCode.html">documentation</a> for more details.
        /// </summary>
        public long ResponseCode { get; }

#if UNITY_2020_1_OR_NEWER
        /// <summary>
        /// The outcome of the request.
        /// </summary>
        public UnityWebRequest.Result Result { get; }
#endif
        /// <summary>
        /// The HTTP verb used by this UnityWebRequest, such as GET or POST.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// The target url of the request.
        /// </summary>
        public string Url { get; }

        internal bool ShouldRetryDownloadError()
        {
            if (string.IsNullOrEmpty(Error))
                return true;

            if (Error == "Request aborted" ||
                Error == "Unable to write data" ||
                Error == "Malformed URL" ||
                Error == "Out of memory" ||
                Error == "Encountered invalid redirect (missing Location header?)" ||
                Error == "Cannot modify request at this time" ||
                Error == "Unsupported Protocol" ||
                Error == "Destination host has an erroneous SSL certificate" ||
                Error == "Unable to load SSL Cipher for verification" ||
                Error == "SSL CA certificate error" ||
                Error == "Unrecognized content-encoding" ||
                Error == "Request already transmitted" ||
                Error == "Invalid HTTP Method" ||
                Error == "Header name contains invalid characters" ||
                Error == "Header value contains invalid characters" ||
                Error == "Cannot override system-specified headers"
               )
                return false;

            /* Errors that can be retried:
                "Unknown Error":
                "No Internet Connection"
                "Backend Initialization Error":
                "Cannot resolve proxy":
                "Cannot resolve destination host":
                "Cannot connect to destination host":
                "Access denied":
                "Generic/unknown HTTP error":
                "Unable to read data":
                "Request timeout":
                "Error during HTTP POST transmission":
                "Unable to complete SSL connection":
                "Redirect limit exceeded":
                "Received no data in response":
                "Destination host does not support SSL":
                "Failed to transmit data":
                "Failed to receive data":
                "Login failed":
                "SSL shutdown failed":
                "Redirect limit is invalid":
                "Not implemented":
                "Data Processing Error, see Download Handler error":
             */
            return true;
        }
    }
    
    internal class WebRequestQueueOperation
    {
        private bool m_Completed = false;
        public UnityWebRequestAsyncOperation Result;
        public Action<UnityWebRequestAsyncOperation> OnComplete;

        public bool IsDone
        {
            get { return m_Completed || Result != null; }
        }

        internal UnityWebRequest m_WebRequest;

        public WebRequestQueueOperation(UnityWebRequest request)
        {
            m_WebRequest = request;
        }

        internal void Complete(UnityWebRequestAsyncOperation asyncOp)
        {
            m_Completed = true;
            Result = asyncOp;
            OnComplete?.Invoke(Result);
        }
    }


    internal static class WebRequestQueue
    {
        internal static int s_MaxRequest = 500;
        internal static Queue<WebRequestQueueOperation> s_QueuedOperations = new Queue<WebRequestQueueOperation>();
        internal static List<UnityWebRequestAsyncOperation> s_ActiveRequests = new List<UnityWebRequestAsyncOperation>();
        public static void SetMaxConcurrentRequests(int maxRequests)
        {
            if (maxRequests < 1)
                throw new ArgumentException("MaxRequests must be 1 or greater.", "maxRequests");
            s_MaxRequest = maxRequests;
        }

        public static WebRequestQueueOperation QueueRequest(UnityWebRequest request)
        {
            WebRequestQueueOperation queueOperation = new WebRequestQueueOperation(request);
            if (s_ActiveRequests.Count < s_MaxRequest)
            {
                UnityWebRequestAsyncOperation webRequestAsyncOp = null;
                try
                {
                    webRequestAsyncOp = request.SendWebRequest();
                    s_ActiveRequests.Add(webRequestAsyncOp);

                    if (webRequestAsyncOp.isDone)
                        OnWebAsyncOpComplete(webRequestAsyncOp);
                    else
                        webRequestAsyncOp.completed += OnWebAsyncOpComplete;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }

                queueOperation.Complete(webRequestAsyncOp);
            }
            else
                s_QueuedOperations.Enqueue(queueOperation);

            return queueOperation;
        }

        internal static void WaitForRequestToBeActive(WebRequestQueueOperation request, int millisecondsTimeout)
        {
            var completedRequests = new List<UnityWebRequestAsyncOperation>();
            while (s_QueuedOperations.Contains(request))
            {
                completedRequests.Clear();
                foreach (UnityWebRequestAsyncOperation webRequestAsyncOp in s_ActiveRequests)
                {
                    if (UnityWebRequestUtilities.IsAssetBundleDownloaded(webRequestAsyncOp))
                        completedRequests.Add(webRequestAsyncOp);
                }
                foreach (UnityWebRequestAsyncOperation webRequestAsyncOp in completedRequests)
                {
                    bool requestIsActive = s_QueuedOperations.Peek() == request;
                    webRequestAsyncOp.completed -= OnWebAsyncOpComplete;
                    OnWebAsyncOpComplete(webRequestAsyncOp);
                    if (requestIsActive)
                        return;
                }
                System.Threading.Thread.Sleep(millisecondsTimeout);
            }
        }

        private static void OnWebAsyncOpComplete(AsyncOperation operation)
        {
            s_ActiveRequests.Remove((operation as UnityWebRequestAsyncOperation));

            if (s_QueuedOperations.Count > 0)
            {
                var nextQueuedOperation = s_QueuedOperations.Dequeue();
                var webRequestAsyncOp = nextQueuedOperation.m_WebRequest.SendWebRequest();
                webRequestAsyncOp.completed += OnWebAsyncOpComplete;
                s_ActiveRequests.Add(webRequestAsyncOp);
                nextQueuedOperation.Complete(webRequestAsyncOp);
            }
        }
    }
    
    internal class UnityWebRequestUtilities
    {
        public static bool RequestHasErrors(UnityWebRequest webReq, out UnityWebRequestResult result)
        {
            result = null;
            if (webReq == null || !webReq.isDone)
                return false;

#if UNITY_2020_1_OR_NEWER
            switch (webReq.result)
            {
                case UnityWebRequest.Result.InProgress:
                case UnityWebRequest.Result.Success:
                    return false;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    result = new UnityWebRequestResult(webReq);
                    return true;
                default:
                    throw new NotImplementedException($"Cannot determine whether UnityWebRequest succeeded or not from result : {webReq.result}");
            }
#else
            var isError = webReq.isHttpError || webReq.isNetworkError;
            if (isError)
                result = new UnityWebRequestResult(webReq);

            return isError;
#endif
        }

        internal static bool IsAssetBundleDownloaded(UnityWebRequestAsyncOperation op)
        {
#if ENABLE_ASYNC_ASSETBUNDLE_UWR
            var handler = (DownloadHandlerAssetBundle)op.webRequest.downloadHandler;
            if (handler != null && handler.autoLoadAssetBundle)
                return handler.isDownloadComplete;
#endif
            return op.isDone;
        }
    }
    
    public class RemoteProviderException : ProviderException
    {
        /// <summary>
        /// Creates a new instance of <see cref="ProviderException"/>.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="location">The resource location that the operation was trying to provide.</param>
        /// <param name="uwrResult">The result of the unity web request, if any.</param>
        /// <param name="innerException">The exception that caused the error, if any.</param>
        public RemoteProviderException(string message, IResourceLocation location = null, UnityWebRequestResult uwrResult = null, Exception innerException = null)
            : base(message, location, innerException)
        {
            WebRequestResult = uwrResult;
        }

        ///<inheritdoc/>
        public override string Message => this.ToString();

        /// <summary>
        /// The result of the unity web request, if any.
        /// </summary>
        public UnityWebRequestResult WebRequestResult { get; }

        /// <summary>Provides a new string object describing the exception.</summary>
        /// <returns>A newly allocated managed string.</returns>
        public override string ToString()
        {
            if (WebRequestResult != null)
                return $"{GetType().Name} : {base.Message}\nUnityWebRequest result : {WebRequestResult}\n{InnerException}";
            else
                return base.ToString();
        }
    }
}
