
using Spine;
using Spine.Unity;
using UnityEngine;

public class EffectJump : MonoBehaviour
{
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
        Destroy(gameObject);
    }
}
