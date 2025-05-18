using Spine.Unity.Playables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class LibraryBinder : MonoBehaviour
{
    private SpineAnimationStateTrack m_spineAnimTrack;
    private SpineSkeletonFlipTrack m_spineFlipTrack;
    //
    private SpriteShape m_spriteShape;
    private SpriteShapeController m_spriteShapeController;

    void Start()
    {
        m_spineAnimTrack = null;
        m_spineFlipTrack = null;
        //
        m_spriteShape = null;
        m_spriteShapeController = null;
    }
}
