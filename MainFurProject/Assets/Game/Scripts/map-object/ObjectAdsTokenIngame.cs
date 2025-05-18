using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAdsTokenIngame : MonoBehaviour
{
    private bool m_collected;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.tag != GameTag.PLAYER || m_collected)
            return;
        m_collected = true;
        // GameController.UpdateAdsToken(1, transform.position);
        Destroy(gameObject);
    }
}
