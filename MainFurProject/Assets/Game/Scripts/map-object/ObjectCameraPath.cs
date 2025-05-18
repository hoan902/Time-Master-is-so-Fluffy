using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCameraPath : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera m_virtualCamera;
    private float m_shakeTimer;
    private float m_shakeTimerTotal;
    private float m_startingIntensity;

    void Start()
    {
        GameObject target = GameObject.FindGameObjectWithTag("camera-follow");
        if (target != null)
            m_virtualCamera.Follow = target.transform;        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != GameTag.PLAYER)
            return;
        m_virtualCamera.gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag != GameTag.PLAYER)
            return;
        m_virtualCamera.gameObject.SetActive(false);
    }
}
