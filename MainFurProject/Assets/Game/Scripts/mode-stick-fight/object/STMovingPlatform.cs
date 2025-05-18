using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class STMovingPlatform : MonoBehaviour
{
    private const float M_MOVE_RATIO = 0.1f;//use this value to control speed to match with old moving platform speed
    
    public bool autoPlay = false;
    [Min(0.01f)]
    public float speed = 1.0f;
    public MovingPlatformType platformType;

    [HideInInspector]
    public Vector2[] localNodes = new Vector2[1];

    public float[] waitTimes = new float[1];

    private Vector2[] m_worldNode;
    private Sequence m_sequence;

    public Vector2[] worldNodes {get => m_worldNode;}
    public bool Started
    {
        get => m_sequence != null && m_sequence.IsPlaying();
    }

    private void Reset()
    {
        //we always have at least a node which is the local position
        localNodes[0] = Vector3.zero;
        waitTimes[0] = 0;
    }

    void Awake()
    {
        m_worldNode = new Vector2[localNodes.Length];
        for (int i = 0; i < m_worldNode.Length; ++i)
            m_worldNode[i] = transform.TransformPoint(localNodes[i]);
    }

    void Start()
    {
        if(m_worldNode.Length < 2)
            return;
        Init();
    }

    private void OnDestroy()
    {
        m_sequence.Kill();
    }

    void Init(bool addLoopNode = true)
    {
        m_sequence = DOTween.Sequence();
        Vector2 start = m_worldNode[0];
        if (addLoopNode)
        {
            if (platformType == MovingPlatformType.Loop)
            {
                List<Vector2> temp = m_worldNode.ToList();
                temp.Add(m_worldNode[0]);
                m_worldNode = temp.ToArray();
            }
        }
        for (int i = 1; i < m_worldNode.Length; i++)
        {
            float delay = waitTimes[i-1];
            float time = (m_worldNode[i] - start).magnitude / (speed * M_MOVE_RATIO);
            m_sequence.Append(transform.DOMove(m_worldNode[i], time).SetEase(Ease.Linear).SetDelay(delay));
            start = m_worldNode[i];
        }

        if (platformType != MovingPlatformType.Loop)
        {
            float delay = waitTimes[^1] / 2f;
            m_sequence.AppendInterval(delay);
        }
        
        m_sequence.SetAutoKill(false);
        switch (platformType)
        {
            case MovingPlatformType.Elevator:
                m_sequence.Pause();
                break;
            case MovingPlatformType.Loop:
                m_sequence.SetLoops(-1, LoopType.Restart);
                m_sequence.Goto(m_sequence.Duration(false)*1000001);
                break;
            case MovingPlatformType.PingPong:
                m_sequence.SetLoops(-1, LoopType.Yoyo);
                m_sequence.Goto(m_sequence.Duration(false)*1000001);
                break;
        }
        if(autoPlay)
            m_sequence.Play();
    }

    public void MoveForward()
    {
        m_sequence.PlayForward();
    }

    public void MoveBackward()
    {
        m_sequence.PlayBackwards();
    }

    public void StopMove()
    {
        m_sequence.Pause();
    }
    
    public void ResumeMove()
    {
        m_sequence.Play();
    }

    public void RestartMove()
    {
        m_sequence?.Kill();
        Init(false);
    }

    public void UpdateVelocity(float speedRatio)
    {
        m_sequence.timeScale = speedRatio;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if(m_sequence.isBackwards)
                MoveForward();
            else
                MoveBackward();
        }
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 1; i < localNodes.Length; ++i)
        {
            Vector2 node = transform.TransformPoint(localNodes[i]);
            Gizmos.DrawWireSphere(node, 0.5f);
        }
    }
#endif
}
