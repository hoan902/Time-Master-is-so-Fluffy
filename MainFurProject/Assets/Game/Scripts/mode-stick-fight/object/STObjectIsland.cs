using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectIsland : MonoBehaviour
{
    [SerializeField] private GameObject m_point;
    [SerializeField] private GameObject m_line;

    void Start()
    {
        UpdateLine();
    }

    void UpdateLine()
    {
        STMovingPlatform movingPlatform = GetComponent<STMovingPlatform>();
        GameObject line = Instantiate(m_line);
        m_line.transform.SetParent(transform.parent, false);
        m_line.SetActive(true);
        LineRenderer lineRenderer = m_line.GetComponent<LineRenderer>();
        lineRenderer.positionCount = movingPlatform.worldNodes.Length;
        lineRenderer.SetPositions(movingPlatform.worldNodes.ToVector3Array());
        foreach(Vector3 p in movingPlatform.worldNodes)
        {
            GameObject node = Instantiate(m_point);
            node.transform.SetParent(transform.parent, false);
            node.SetActive(true);
            node.transform.position = p;
        }
    }
}
