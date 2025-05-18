using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectArrow : MonoBehaviour
{
    [SerializeField] private float m_angle = 0f;
    [SerializeField] private Transform m_arrow;


    public void UpdateAngle()
    {
        m_arrow.eulerAngles = new Vector3(0, 0, m_angle);
    }
}
