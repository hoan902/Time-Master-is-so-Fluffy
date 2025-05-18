using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGear : MonoBehaviour
{
    [SerializeField] private float m_time;
    [SerializeField] private bool m_clockwise;
    private Rigidbody2D m_rigid;
    private float m_angle;
    
    void Awake()
    {
        m_rigid = GetComponent<Rigidbody2D>();
        m_rigid.bodyType = RigidbodyType2D.Kinematic;
        m_angle = m_rigid.rotation;
    }

    void FixedUpdate()
    {
        m_angle += (m_clockwise ? -1 : 1) * 360 * Time.deltaTime / m_time;
        m_rigid.MoveRotation(m_angle);
    }
}
