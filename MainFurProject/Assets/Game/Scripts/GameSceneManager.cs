using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField] private AssetReference m_home;
    [SerializeField] private AssetReference m_game;
    [SerializeField] private AudioListener m_mainAudio;
    [SerializeField] private Camera m_uiCamera;

    private GameObject m_currentScene;
    private UniversalAdditionalCameraData m_cameraData;
    
    void Awake()
    {
#if UNITY_EDITOR
        if (!MainModel.configLoaded)
        {
            SceneManager.LoadScene("Loading", LoadSceneMode.Single);
            return;
        }
#endif

        m_cameraData = m_uiCamera.GetUniversalAdditionalCameraData();

        MainController.openSceneEvent += OnOpenScene;
    }

    void Start()
    {
        MainController.OpenScene(SceneType.Home);
    }

    void OnDestroy()
    {
        MainController.openSceneEvent -= OnOpenScene;
    }

    void OnOpenScene(SceneType sceneType)
    {
        StopAllCoroutines();
        StartCoroutine(IOpenScene(sceneType));
    }

    IEnumerator IOpenScene(SceneType sceneType)
    {
        m_mainAudio.enabled = sceneType == SceneType.Home;
        SoundManager.StopMusic();
        if(m_currentScene != null)
            Destroy(m_currentScene);
        m_currentScene = null;
        yield return new WaitForEndOfFrame();
        AssetReference go = null;
        switch (sceneType)
        {
            case SceneType.Home:
                go = m_home;
                StartCoroutine(IWaitHome(sceneType));
                break;
            case SceneType.Game:
                go = m_game;
                StartCoroutine(IWaitGame(sceneType));
                break;
        }
        if (go == null)
            yield break;
        MainModel.currentSceneType = sceneType;
        AsyncOperationHandle<GameObject> scene = go.InstantiateAsync(transform, false);
        yield return scene;
        m_currentScene = scene.Result;
    }
    
    IEnumerator IWaitHome(SceneType sceneType)
    {
        while(m_currentScene == null)
        {
            yield return null;
        }
        m_uiCamera.clearFlags = CameraClearFlags.SolidColor;
        yield return new WaitForEndOfFrame();
        MainController.DoSceneTrasition(true, () =>
        {            
            MainController.SceneReady(sceneType);
        });
    }

    IEnumerator IWaitGame(SceneType sceneType)
    {
        while(Camera.main == null)
        {
            yield return null;
        }
        m_uiCamera.clearFlags = CameraClearFlags.Nothing;
        yield return new WaitForEndOfFrame();
        MainController.DoSceneTrasition(true, () =>
        {            
            MainController.SceneReady(sceneType);
            /*if(MainModel.gameInfo != null && MainModel.gameInfo.playMode == PlayMode.Normal)
            {
                TrackingManager.IngameAction(ConfigLoader.instance.GetFakeLevelIndex(MainModel.gameInfo.level, MainModel.gameInfo.world) + "_0");
            }*/
        });
    }
}
