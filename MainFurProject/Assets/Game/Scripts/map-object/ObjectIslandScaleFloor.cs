using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectIslandScaleFloor : MonoBehaviour
{
    [SerializeField] private LayerMask m_layerMask;
    [SerializeField] private Collider2D m_collider;
    [SerializeField] private float m_offset = 0.1f;

    private PlatformCatcher m_platformCatcher;

    public int childCount;

    private void Awake() 
    {
        m_platformCatcher = GetComponent<PlatformCatcher>();    
    }

    private void FixedUpdate() 
    {
        Vector2 start = m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, m_collider.bounds.extents.y + m_offset);
        Vector2 end = m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, m_collider.bounds.extents.y + m_offset);
        RaycastHit2D[] hits = Physics2D.LinecastAll(start, end, m_layerMask);

        childCount = hits.Length;
    }

    void OnDrawGizmos()
    {
        if (m_collider == null)
            m_collider = GetComponent<Collider2D>();
        Vector2 start = m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, m_collider.bounds.extents.y + m_offset);
        Vector2 end = m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, m_collider.bounds.extents.y + m_offset);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, end);
    }
}
