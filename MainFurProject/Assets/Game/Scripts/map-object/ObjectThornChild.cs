using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class ObjectThornChild : STObjectInteractive
{
    [SerializeField] private Collider2D m_collider;

    public override void Dead()
    {
        base.Dead();
        m_collider.enabled = false;
    }
    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        spine.AnimationState.SetAnimation(0, "idle2", true);
    }

}
