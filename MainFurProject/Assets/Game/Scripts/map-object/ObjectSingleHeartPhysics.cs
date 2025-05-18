using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSingleHeartPhysics : MonoBehaviour
{
    [SerializeField] private AudioClip m_audioCollect;

    private bool m_stop;

    private void Start() 
    {
        m_stop = false;    
    }

    public void OnTrigger(Collider2D other)
    {
        if(other.gameObject.tag != GameTag.PLAYER || m_stop)
            return;
        m_stop = true;
        SoundManager.PlaySound(m_audioCollect, false);
        GameController.UpdateHeart(1, transform.position);
        Destroy(gameObject);
    }
}
