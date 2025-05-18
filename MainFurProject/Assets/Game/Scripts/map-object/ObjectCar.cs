using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCar : MonoBehaviour
{
    [SerializeField] private float m_speed = 10f;
    [SerializeField] private Vector2 m_startPoint;
    [SerializeField] private Vector2 m_endPoint;
    [SerializeField] private GameObject m_startPointObj;
    [SerializeField] private GameObject m_endPointObj;
    [SerializeField] private SpriteRenderer m_line;
    [SerializeField] private Rigidbody2D m_truckle;
    [SerializeField] private Transform m_wheelLeft;
    [SerializeField] private Transform m_wheelRight;

    private Vector3 m_startPos;
    private bool m_start;
    private float m_lastPosX;
    private int m_direction;
    private bool m_lockMove;

    void Start()
    {
        m_startPos = m_truckle.transform.localPosition;
        m_start = false;
    }

    void OnPlayerTriggerEnter(Collider2D collider)
    {
        if (collider.tag != GameTag.PLAYER)
            return;
        RegMove();
    }

    void OnPlayerTriggerExit(Collider2D collider)
    {
        if (collider.tag != GameTag.PLAYER)
            return;
        OnStopMove();
        UnRegMove();
    }

    void FixedUpdate()
    {
        if (!m_start)
            return;
        CheckTarget();
        if (m_lockMove)
            return;
        //m_lastPosX = m_truckle.transform.localPosition.x;
        Vector3 localPos = m_truckle.transform.localPosition + new Vector3(m_speed, 0) * Time.deltaTime * m_direction;
        m_truckle.MovePosition(m_truckle.transform.parent.TransformPoint(localPos));        
        //float delX = m_truckle.transform.localPosition.x - m_lastPosX;
       // m_wheelLeft.localEulerAngles = Vector3.Lerp(m_wheelLeft.localEulerAngles, new Vector3(0, 0, m_wheelLeft.localEulerAngles.z - delX * 2000f), 0.05f);
       // m_wheelRight.localEulerAngles = Vector3.Lerp(m_wheelRight.localEulerAngles, new Vector3(0, 0, m_wheelRight.localEulerAngles.z - delX * 2000f), 0.05f);
    }

    void RegMove()
    {
        UnRegMove();
        InputController.leftAction += OnMoveLeft;
        InputController.rightAction += OnMoveRight;
        InputController.releaseMove += OnStopMove;
    }

    void UnRegMove()
    {
        InputController.leftAction -= OnMoveLeft;
        InputController.rightAction -= OnMoveRight;
        InputController.releaseMove -= OnStopMove;
    }

    private void OnStopMove()
    {
        m_start = false;
        m_direction = 0;
    }

    void OnMoveRight()
    {
        m_start = true;
        m_direction = 1;
    }


    void OnMoveLeft()
    {
        m_start = true;
        m_direction = -1;
    }

    void CheckTarget()
    {
        m_lockMove = false;
        Vector3 target = m_truckle.transform.localPosition;
        if (m_direction < Mathf.Epsilon && target.x <= m_startPos.x)
        {
            m_truckle.transform.localPosition = m_startPos;
            m_lockMove = true;
        }
        if (m_direction > Mathf.Epsilon && target.x >= (m_startPos.x + m_line.size.x - 2.3f))
        {
            m_truckle.transform.localPosition = m_startPos + new Vector3(m_line.size.x - 2.3f, 0);
            m_lockMove = true;
        }
    }

    public void UpdateLine()
    {
        m_startPointObj.transform.localPosition = m_startPoint;
        m_endPointObj.transform.localPosition = m_endPoint;
        m_line.transform.localPosition = m_startPoint;  
        Vector2 direction = (m_endPoint - m_startPoint);
        Vector2 directNorm = direction.normalized;
        m_line.size = new Vector2(direction.magnitude, m_line.size.y);
        m_line.transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(directNorm.y, directNorm.x)*Mathf.Rad2Deg);    
    }

    public void UpdatePos(Vector2 startPos, Vector2 endPos)
    {
        m_startPoint = transform.InverseTransformPoint(startPos);
        m_endPoint = transform.InverseTransformPoint(endPos);
        UpdateLine();
    }

    public void InitPoint()
    {
        m_startPoint = m_startPointObj.transform.localPosition;
        m_endPoint = m_endPointObj.transform.localPosition;
    }

    public Vector2 GetStartPoint()
    {
        return m_startPointObj.transform.position;
    }

    public Vector2 GetEndPoint()
    {
        return m_endPointObj.transform.position;
    }
}
