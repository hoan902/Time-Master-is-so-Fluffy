using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectAutoDestroyBullet : MonoBehaviour
{
    [SerializeField] private Rigidbody2D m_parentRigidbody;

    private void Awake() 
    {
        if(!m_parentRigidbody)
            m_parentRigidbody = GetComponentInParent<Rigidbody2D>();
    }

    private void OnBecameInvisible() 
    {
        if(m_parentRigidbody.velocity.y < -0.1f && transform.parent.gameObject != null)
            Destroy(transform.parent.gameObject);    
    }
}
