using System;
using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;

public class STBossGiantGolemHand : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation m_spine;
    [SerializeField] private Transform m_hitBox;
    [SerializeField] private Collider2D m_collider;
    [SerializeField] private BodyBoundingBox m_boundingBox;
    
    private Tweener m_moveTween;
    
    private void Start()
    {
        m_boundingBox.SetupCollider();
        m_collider = m_hitBox.GetComponent<Collider2D>();
        m_collider.enabled = false;
        
        m_spine.AnimationState.Complete += OnAnimComplete;
        m_spine.AnimationState.Event += OnAnimEvent;
    }
    private void OnDestroy()
    {
        m_spine.AnimationState.Complete -= OnAnimComplete;
        m_spine.AnimationState.Event -= OnAnimEvent;
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        switch (trackEntry.Animation.Name)
        {
            
        }    
    }
    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        switch (trackEntry.Animation.Name)
        {
            
        }
    }

    public void SetAnimation(string animationName, bool loop)
    {
        m_spine.AnimationState.SetAnimation(0, animationName, loop);
    }
    public void SetColor(Color targetColor)
    {
        m_spine.skeleton.SetColor(targetColor);   
    }
    public void MoveTo(Vector3 destination, float duration, Action cb = null, Action updateCallback = null)
    {
        m_moveTween?.Kill();
        m_moveTween = transform.DOMove(destination, duration).OnUpdate(() =>
        {
            updateCallback?.Invoke();
        }).OnComplete(() =>
        {
            cb?.Invoke();
        });
    }
    public void Dead(string animation)
    {
        m_moveTween?.Kill();
        m_spine.AnimationState.SetAnimation(0, animation, false);
    }
    public void ActivateCollider(bool toActive)
    {
        m_collider.enabled = toActive;
    }
}
