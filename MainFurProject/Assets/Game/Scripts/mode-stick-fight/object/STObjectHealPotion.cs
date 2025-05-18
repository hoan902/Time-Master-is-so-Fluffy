using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectHealPotion : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int m_healthPoints = 20;
    
    [Header("Reference")]
    [SerializeField] private AudioClip m_audioCollect;

    private bool m_stop = true;
    private Transform m_target;

    private void Start() 
    {
        m_stop = false;    
    }

    void OnTrigger(Collider2D other)
    {
        if(other.tag != GameTag.PLAYER || m_stop)
            return;
        m_stop = true;
        SoundManager.PlaySound(m_audioCollect, false);
        STGameController.UpdatePlayerHp(m_healthPoints);
        Destroy(gameObject);
    }
}
