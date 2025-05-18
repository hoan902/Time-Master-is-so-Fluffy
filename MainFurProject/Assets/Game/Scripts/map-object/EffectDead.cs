
using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class EffectDead : MonoBehaviour
{
    [HideInInspector]
    public Action onComplete;
    [SerializeField] private SkeletonAnimation m_spine;

    void Awake()
    {
        m_spine.AnimationState.Complete += OnComplete;
    }

    void OnDestroy()
    {
        m_spine.AnimationState.Complete -= OnComplete;
    }

    private void OnComplete(TrackEntry trackentry)
    {
        onComplete?.Invoke();
        Destroy(gameObject);
    }
}
