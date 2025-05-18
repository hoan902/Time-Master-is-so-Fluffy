
using System;
using System.Collections;
using System.Collections.Generic;
using EncryptedAsset.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelManager : MonoBehaviour
{
    private string m_levelName;
    private GameObject m_level;
    private AsyncOperationHandle<GameObject> m_handle; 

    void Awake()
    {
        GameController.loadMapEvent += OnLoad;
        MainController.openSceneEvent += OnOpenScene;
    }

    void OnDestroy()
    {
        GameController.loadMapEvent -= OnLoad;
        MainController.openSceneEvent -= OnOpenScene;
    }

    void OnOpenScene(SceneType sceneType)
    {
        if (m_level != null)
        {
            Addressables.Release(m_handle);
            EncryptedBundleResource.ClearLevelBundle($"level-{m_levelName}");
        }
    }

    private void OnLoad()
    {
        StartCoroutine(ILoadLevel());
    }

    IEnumerator ILoadLevel()
    {
        yield return null;
        yield return null;
        MainModel.inCutscene = false;
        ConfigLoader loader = ConfigLoader.instance;
        GameInfo info = MainModel.gameInfo;
        int world = info.world;
        int level = info.level;
        switch (info.playMode)
        {
            case PlayMode.Normal:
                MainModel.levelResult.fakeLevelIndex = loader.GetFakeLevelIndex(level, world);
                break;
            case PlayMode.Boss:
                MainModel.levelResult.fakeLevelIndex = level + 1;
                break;
        }
        m_levelName = loader.GetLevel(world, level, info.playMode).levelPath;
        string baseLevel = m_levelName;
        //
        info.levelPath = m_levelName;
        AsyncOperationHandle<GameObject> operation = Addressables.InstantiateAsync($"level-{m_levelName}", transform, false);
        yield return operation;
        if (operation.Result == null)
        {
            m_levelName = baseLevel;
            info.levelPath = m_levelName;
            operation = Addressables.InstantiateAsync($"level-{baseLevel}", transform, false);
            yield return operation;
            if (operation.Result == null)
                MainController.OpenScene(SceneType.Home);
            else
            {
                m_level = operation.Result;
                //
                GameController.LevelReady();
            }
        }
        else
        {
            m_level = operation.Result;
            GameController.LevelReady();
        }

        m_handle = operation;
    }
}
