using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class BodyBoundingBox : MonoBehaviour
{
    [SerializeField] private string m_boneFollow;
    
    public void SetupCollider()
    {       
        SkeletonRenderer renderer = GetComponentInParent<SkeletonRenderer>();
        Slot slot = renderer.Skeleton.FindSlot(m_boneFollow.ToString());
        Skin sk = renderer.Skeleton.Data.FindSkin("default");
        Attachment att = sk.GetAttachment(renderer.Skeleton.Data.FindSlot(m_boneFollow.ToString()).Index, m_boneFollow.ToString());     
        SkeletonUtility.AddBoundingBoxAsComponent(att as BoundingBoxAttachment, slot , gameObject);

        BoneFollower boneFollower = GetComponent<BoneFollower>();
        boneFollower.boneName = m_boneFollow.ToString();
        boneFollower.followLocalScale = true;
    }
}
