using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class STObjectSaw : MonoBehaviour
{
    [SerializeField] private Vector3[] m_path;
    [SerializeField] private float m_moveTime = 4f;
    [SerializeField] private int m_playerDamage = 20;
    [SerializeField] private int m_monsterDamage = 10;
    [SerializeField] private Transform m_saw;
    [SerializeField] private float m_firstDelay = 0f;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(m_firstDelay);
        if(m_path.Length > 0)
        {
            m_saw.position = m_path[0];
            m_saw.DOPath(m_path, m_moveTime).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
        }        
    }
    void OnDestroy()
    {
        m_saw.DOKill();
    }

    public int GetDamage(bool toPlayer)
    {
        return toPlayer ? m_playerDamage : m_monsterDamage;
    }

    public Vector3[] GetPath()
    {
        return m_path;
    }

    public void UpdatePath(Vector3 value, int index)
    {
        m_path[index] = value;
    }
}
