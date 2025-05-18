using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 1.0f;
    public MovingPlatformType platformType;

    [HideInInspector]
    public Vector3[] localNodes = new Vector3[1];

    public float[] waitTimes = new float[1];

    public Vector3[] worldNode {  get { return m_worldNode; } }

    private Vector3[] m_worldNode;

    private Rigidbody2D m_rigibody;

    private int m_current = 0;
    private int m_next = 0;
    private int m_dir = 1;
    private float m_waitTime = -1.0f;
    private Vector2 m_velocity;
    private bool m_started = false;
    private float m_slowSpeedRatio = 1f;

    public bool Started {get => m_started;}

    public Vector2 velocity
    {
        get { return m_velocity; }
    }

    private void Reset()
    {
        //we always have at least a node which is the local position
        localNodes[0] = Vector3.zero;
        waitTimes[0] = 0;
    }

    void Awake()
    {
        m_rigibody = GetComponent<Rigidbody2D>();

        m_worldNode = new Vector3[localNodes.Length];
        for (int i = 0; i < m_worldNode.Length; ++i)
            m_worldNode[i] = transform.TransformPoint(localNodes[i]);
    }

    void Start()
    {
        Init();
    }

    void Init()
    {
        m_current = 0;
        m_dir = 1;
        m_next = localNodes.Length > 1 ? 1 : 0;

        m_waitTime = waitTimes[0];
        m_started = platformType == MovingPlatformType.PingPong;
    }

    void FixedUpdate()
    {
        if (!m_started)
            return;

        //no need to update we have a single node in the path
        if (m_current == m_next)
            return;

        if(m_waitTime > 0)
        {
            m_waitTime -= Time.deltaTime;
            return;
        }

        float distanceToGo = speed * Time.deltaTime;

        while(distanceToGo > 0)
        {
            Vector2 direction = m_worldNode[m_next] - transform.position;

            float dist = distanceToGo;
            // if(direction.sqrMagnitude < dist * dist)
            if(Vector3.Distance(transform.position, m_worldNode[m_next]) < 0.1f)
            { 
                dist = direction.magnitude;

                m_current = m_next;

                m_waitTime = waitTimes[m_current];

                if (m_dir > 0)
                {
                    m_next += 1;
                    if (m_next >= m_worldNode.Length)
                    {
                        switch(platformType)
                        {
                            case MovingPlatformType.PingPong:
                                m_next = m_worldNode.Length - 2;
                                m_dir = -1;
                                break;
                            case MovingPlatformType.Elevator:
                                StopMoving();
                                break;
                            case MovingPlatformType.Loop:
                                m_next = 0;
                                m_dir = 1;
                                break;
                        }
                    }
                }
                else
                {
                    m_next -= 1;
                    if(m_next < 0)
                    { 
                        switch (platformType)
                        {
                            case MovingPlatformType.PingPong:
                                m_next = 1;
                                m_dir = 1;
                                break;
                            case MovingPlatformType.Elevator:
                                StopMoving();
                                break;
                            case MovingPlatformType.Loop:
                                m_next = m_worldNode.Length - 1;
                                m_dir = -1;
                                break;
                        }
                    }
                }
            }

            m_velocity = direction.normalized * dist * m_slowSpeedRatio;
            Vector3 position = Vector3.Lerp(m_rigibody.position, (Vector2)m_rigibody.position + m_velocity, 0.1f);
            m_rigibody.MovePosition(position);
            distanceToGo -= dist;
            if (m_waitTime > 0.001f || !m_started) 
                break;
        }
    }

    public void StartMoving()
    {
        m_started = true;
    }

    public void StopMoving()
    {
        m_started = false;
    }

    public void ResetPlatform()
    {
        transform.position = m_worldNode[0];
        Init();
    }

    public void MoveForward()
    {
        m_started = true;
        m_dir = 1;
        int temp = m_next;
        m_next = m_current;
        m_current = temp;
    }

    public void MoveBackward()
    {
        m_started = true;
        m_dir = -1;
        int temp = m_next;
        m_next = m_current;
        m_current = temp;
    }

    public void UpdateVelocity(float ratio)
    {
        m_slowSpeedRatio = ratio;
    }
}