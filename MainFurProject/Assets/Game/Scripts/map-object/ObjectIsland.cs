
using System;
using DG.Tweening;
using UnityEngine;

public class ObjectIsland : MonoBehaviour
{
    [SerializeField] private GameObject m_point;
    [SerializeField] private GameObject m_line;

    void Start()
    {
        UpdateLine();
    }

    void UpdateLine()
    {
        MovingPlatform movingPlatform = GetComponent<MovingPlatform>();
        GameObject line = Instantiate(m_line);
        m_line.transform.SetParent(transform.parent, false);
        m_line.SetActive(true);
        LineRenderer lineRenderer = m_line.GetComponent<LineRenderer>();
        lineRenderer.positionCount = movingPlatform.worldNode.Length;
        lineRenderer.SetPositions(movingPlatform.worldNode);
        foreach(Vector3 p in movingPlatform.worldNode)
        {
            GameObject node = Instantiate(m_point);
            node.transform.SetParent(transform.parent, false);
            node.SetActive(true);
            node.transform.position = p;
        }
    }    
}
