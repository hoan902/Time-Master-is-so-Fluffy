using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        MainController.readySceneEvent += OnSceneReady;
    }

    private void OnSceneReady(SceneType sceneType)
    {
        if (sceneType == SceneType.Home)
        {
            MainController.readySceneEvent -= OnSceneReady;
            StartCoroutine(ILoad());
        }
    }
    

    IEnumerator ILoad()
    {
        yield return new WaitForSeconds(1f);
        SystemController.InitInappUpdate();
        yield return new WaitForSeconds(1f);
        IAPManager.api.Init();
    }
    
}
