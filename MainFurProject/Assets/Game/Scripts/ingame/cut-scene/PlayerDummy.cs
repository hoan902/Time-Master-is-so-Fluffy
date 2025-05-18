using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDummy : MonoBehaviour
{
    [SerializeField] private bool m_syncSkin = false;
    [HideInInspector]
    [SerializeField] private SkeletonAnimation m_spine;
   
    void Awake()
    {
        GameController.changeSkinEvent += OnChangeSkin;
    }

    IEnumerator Start()
    {
        yield return null;
        OnChangeSkin(MainModel.currentSkin);
    }

    private void OnDestroy()
    {
        GameController.changeSkinEvent -= OnChangeSkin;
    }

    private void OnChangeSkin(string newSkin)
    {
        if (!m_syncSkin)
            return;
        m_spine.SetSkin(newSkin);
    }
}
