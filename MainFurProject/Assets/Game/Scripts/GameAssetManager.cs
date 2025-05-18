using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;



public class GameAssetManager : MonoBehaviour
{
    public static GameAssetManager api;
    public static Action<float> updateEvent;
    public static Action completeEvent;

    [SerializeField] private AssetReference[] m_preloadAssets;

    private bool m_activeScene;

    void Awake()
    {
        if (api == null)
            api = this;
        DontDestroyOnLoad(gameObject);
        Addressables.InitializeAsync().Completed += OnInitializeComplete;
    }

    private void OnInitializeComplete(AsyncOperationHandle<IResourceLocator> obj)
    {
        SystemVariable.assetReady = true;
        StartCoroutine(ILoad());
    }

    public void ActiveScene()
    {
        m_activeScene = true;
    }

    IEnumerator ILoad()
    {
        yield return new WaitForEndOfFrame();
        foreach (AssetReference asset in m_preloadAssets)
        {
            AsyncOperationHandle<GameObject> async = asset.LoadAssetAsync<GameObject>();
            yield return async;
        }
        //load config;
        AsyncOperationHandle<GameConfigObject> asyncConfig = ConfigLoader.instance.configAsset.LoadAssetAsync<GameConfigObject>();
        yield return asyncConfig;
        ConfigLoader.instance.config = asyncConfig.Result;
        //load buttons style
        AsyncOperationHandle<CutsceneButtonStyleConfig> asyncButtons = ConfigLoader.instance.buttonStyleConfigAsset.LoadAssetAsync<CutsceneButtonStyleConfig>();
        yield return asyncButtons;
        ConfigLoader.instance.buttonStyleConfig = asyncButtons.Result;
        //load level
        string levelName = ConfigLoader.instance.GetCurrentLevel();
        AsyncOperationHandle<GameObject> asyncLevel = Addressables.LoadAssetAsync<GameObject>($"level-{levelName}");
        yield return asyncLevel;
        Addressables.Release(asyncLevel);
        //load scene
        AsyncOperationHandle<SceneInstance> asyncScene = Addressables.LoadSceneAsync("Assets/Game/Scenes/Game.unity", LoadSceneMode.Single, false);
        yield return asyncScene;
        completeEvent?.Invoke();
        while (!m_activeScene)
        {
            yield return null;
        }
        asyncScene.Result.ActivateAsync();
    }
}
