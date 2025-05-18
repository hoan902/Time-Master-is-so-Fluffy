using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectHeal : MonoBehaviour
{   

    private int m_healValue;
    private Vector3 m_starPos;
    private Transform m_target;

    public void Init(int value, Vector3 startPos, Transform target)
    {
        m_healValue = value;
        m_starPos = startPos;

        transform.DOMoveY(transform.position.y + 2f, 2f).OnComplete(() => {
            GetComponent<BoxCollider2D>().enabled = true;
            m_target = target;
        });
    }

    private void Update() 
    {
        if(m_target != null)
        {
            transform.position = Vector3.Lerp(transform.position, m_target.position, 0.1f);
        }    
    }
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.tag != GameTag.PLAYER)
            return;
        GameController.UpdateHealth(m_healValue);
        Destroy(gameObject);
    }
}
