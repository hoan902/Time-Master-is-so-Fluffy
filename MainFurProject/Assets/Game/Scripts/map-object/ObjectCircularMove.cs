using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCircularMove : MonoBehaviour
{
    [SerializeField] private float m_speed;
    [SerializeField] private bool m_clockwise;
    [SerializeField] private float m_centerOffset = 3f;

    private bool m_actived = false;
    private Vector3 m_endPosition;
    private float m_slerpTime = 0;

    public Vector3 startPosition;
    public Vector3 localStartPos = Vector3.one;

    private void Awake() 
    {
        startPosition = transform.TransformPoint(localStartPos);
        m_endPosition = transform.position;
        transform.position = startPosition;

        gameObject.SetActive(false);
    }
    private void Update() 
    {
        if(!m_actived)
            return;
        m_slerpTime += Time.deltaTime;

        Vector3 centerPivot = (startPosition + m_endPosition) * 0.5f;
        if(m_clockwise)
            centerPivot += new Vector3(-m_centerOffset, -m_centerOffset);
        else
            centerPivot -= new Vector3(-m_centerOffset, -m_centerOffset);

        Vector3 startRelativeToCenter = startPosition - centerPivot;
        Vector3 endRelativeToCenter = m_endPosition - centerPivot;

        transform.position = Vector3.Slerp(startRelativeToCenter, endRelativeToCenter, m_slerpTime * m_speed) + centerPivot;
        if(Vector3.Distance(transform.position, m_endPosition) < 0.05f)
            m_actived = false;
    }

    public void StartMove()
    {
        m_actived = true;
    }

    public void UpdateParams(Vector3 start, float speed, float offset, bool clockwise)
    {
        startPosition = start;
        m_speed = speed;
        m_centerOffset = offset;
        m_clockwise = clockwise;

        transform.position = startPosition;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1f);    
    }
}
