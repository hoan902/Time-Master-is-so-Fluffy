using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class ObjectCanonBallRotate : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation m_spine;
    [SerializeField] private Transform m_target;

    private Bone m_aimBone;
    private Bone m_rootBone;
    private Vector3 m_direction;

    private void Awake()
    {
        m_aimBone = m_spine.Skeleton.FindBone("aim");
        m_rootBone = m_spine.Skeleton.FindBone("ech_than");

        Vector3 localPos = m_spine.transform.InverseTransformPoint(m_target.position);
        localPos.x *= m_spine.Skeleton.ScaleX;
        localPos.y *= m_spine.Skeleton.ScaleY;
        m_aimBone.SetLocalPosition(localPos);
    }

    private void Update() 
    {
        Vector3 localPos = m_spine.transform.InverseTransformPoint(m_target.position);
        localPos.x *= m_spine.Skeleton.ScaleX;
        localPos.y *= m_spine.Skeleton.ScaleY;
        m_aimBone.SetLocalPosition(localPos);
    }
}
