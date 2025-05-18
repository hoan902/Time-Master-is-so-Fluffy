using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineBlendController : MonoBehaviour
{
    [Range(0, 10)]
    [SerializeField] private float m_blendTime = 1f;
    [SerializeField] private bool m_hasToBeDisable = false;
    
    private CinemachineVirtualCamera m_virtualCamera;
    private bool m_active;

    public float BlendTime {get => m_blendTime; set => m_blendTime = value;}

    private void Awake() 
    {
        m_virtualCamera = GetComponent<CinemachineVirtualCamera>();    
        m_active = false;

        CutSceneController.beginFinishEvent += OnCutsceneEnd;
    }
    private void OnDestroy() 
    {
        CutSceneController.beginFinishEvent -= OnCutsceneEnd;
    }

    void OnCutsceneEnd()
    {
        if(m_hasToBeDisable)
            gameObject.SetActive(false);
    }

    private void OnEnable() 
    {
        m_active = true;    
        GameController.UpdateBlendTime(m_blendTime);
    }

    // private void OnDisable() 
    // {
    //     if(m_active)
    //     {
    //         GameController.UpdateBlendTime(m_blendTime);
    //     }
    // }
}
