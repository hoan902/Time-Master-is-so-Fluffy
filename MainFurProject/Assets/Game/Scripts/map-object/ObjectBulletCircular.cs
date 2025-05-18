using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectBulletCircular : MonoBehaviour
{
    private float m_speed;
    private bool m_clockwise;
    private float m_centerOffset = 3f;
    private Transform m_target;
    private float m_slerpTime = 0;
    private Vector3 m_startPos;
    private bool m_actived = false;

    public static Action cicularActivedEvent;

    public void Init(float speed, bool clockwise, float centerOffset, Transform target)
    {
        m_speed = speed;
        m_clockwise = clockwise;
        m_centerOffset = centerOffset;
        m_startPos = transform.position;
        m_target = target;
    }
    public void StartMove()
    {
        m_actived = true;
    }

    private void Update()
    {
        if(!m_target || !m_actived)
            return;    
        
        m_slerpTime += Time.deltaTime;

        Vector3 centerPivot = (m_startPos + m_target.position) * 0.5f;
        if(m_clockwise)
            centerPivot += new Vector3(-m_centerOffset, -m_centerOffset);
        else
            centerPivot -= new Vector3(-m_centerOffset, -m_centerOffset);

        Vector3 startRelativeToCenter = m_startPos - centerPivot;
        Vector3 endRelativeToCenter = m_target.position - centerPivot;

        transform.position = Vector3.Slerp(startRelativeToCenter, endRelativeToCenter, m_slerpTime * m_speed) + centerPivot;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.tag == GameTag.PLAYER)
        {
            if(!m_actived)
            {
                m_actived = true;
                cicularActivedEvent?.Invoke();
                return;
            }
        }
        
        else if(other.transform == m_target)
        {
            m_actived = false;
            other.SendMessage("OnHit", SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}
