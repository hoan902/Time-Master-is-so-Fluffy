using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStickDummy : MonoBehaviour
{
    [SerializeField] private bool m_syncSkin = false;
    //[HideInInspector]
    [SerializeField] private SkeletonAnimation m_spine;

    void Awake()
    {
        GameController.changeSkinEvent += OnChangeSkin;
    }
    private void OnDestroy()
    {
        GameController.changeSkinEvent -= OnChangeSkin;
    }
    void OnEnable()
    {
        if (!m_syncSkin)
            return;
        OnChangeSkin(MainModel.CurrentSkin);
    }

    private void OnChangeSkin(string skin)
    {
        if (!m_syncSkin)
            return;
        m_spine.SetSkin(skin);
    }
}
