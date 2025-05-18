using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTrackingIngame : MonoBehaviour
{
    [SerializeField] private string m_keyTracking;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(m_keyTracking == "")
        {
            Destroy(gameObject);
            return;
        }
        Destroy(gameObject);
    }
}
