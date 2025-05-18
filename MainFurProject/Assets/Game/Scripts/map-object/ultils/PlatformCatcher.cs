using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlatformCatcher : MonoBehaviour
{
    [SerializeField] private Transform m_rootTransform;
    [SerializeField] private LayerMask m_layerMask;

    private Dictionary<Rigidbody2D, CatchObject> m_objects = new Dictionary<Rigidbody2D, CatchObject>();

    private Collider2D m_collider;

    void Awake()
    {
        m_collider = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        if (m_collider == null)
            return;
        //clear state
        foreach(KeyValuePair<Rigidbody2D, CatchObject> c in m_objects)
        {
            c.Value.marked = false;
        }
        //catch object
        Vector2 start = m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, m_collider.bounds.extents.y + 0.02f);
        Vector2 end = m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, m_collider.bounds.extents.y + 0.02f);
        RaycastHit2D[] hits = Physics2D.LinecastAll(start, end, m_layerMask);
        for (int i = 0; i < hits.Length; i++)
        {
            Rigidbody2D rigid = hits[i].rigidbody;
            if (rigid == null)
                continue;
            float dot = Vector2.Dot(hits[i].normal, Vector2.down);
            if (dot > 0.9f)
            {
                if(rigid.transform.parent != transform && rigid.transform.parent.GetComponent<PlatformCatcher>() != null)
                    continue;
                if (m_objects.ContainsKey(rigid))
                {
                    m_objects[rigid].marked = true;
                    continue;
                }                
                CatchObject c = new CatchObject(){
                    rigidbody = rigid,
                    parent = rigid.transform.parent,
                    marked = true
                };
                m_objects.Add(rigid, c);
                if (rigid.tag == GameTag.PLAYER)
                    rigid.gameObject.SendMessage("SetParent", m_rootTransform);
                else
                    rigid.transform.SetParent(m_rootTransform, true);
            }
            else
            {
                if (!m_objects.ContainsKey(rigid))
                    continue;
                if (rigid.tag == GameTag.PLAYER)
                    rigid.gameObject.SendMessage("SetParent", m_objects[rigid].parent);
                else
                    rigid.transform.SetParent(m_objects[rigid].parent, true);
                m_objects.Remove(rigid);
            }
        }
        //clear object
        foreach(CatchObject c in m_objects.Values.ToList())
        {
            if(c.marked)
                continue;
            if (c.rigidbody.tag == GameTag.PLAYER)
                c.rigidbody.gameObject.SendMessage("SetParent", c.parent);
            else
                c.rigidbody.transform.SetParent(c.parent, true);
            m_objects.Remove(c.rigidbody);
        }
    }

    public void RemovePlayer()
    {
        foreach(CatchObject c in m_objects.Values.ToList())
        {
            if (c.rigidbody.tag == GameTag.PLAYER)
            {
                c.rigidbody.gameObject.SendMessage("SetParent", c.parent);
                m_objects.Remove(c.rigidbody);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (m_collider == null)
            m_collider = GetComponent<Collider2D>();
        Vector2 start = m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, m_collider.bounds.extents.y + 0.02f);
        Vector2 end = m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, m_collider.bounds.extents.y + 0.02f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, end);
    }

    private class CatchObject{
        public Rigidbody2D rigidbody;
        public Transform parent;
        public bool marked;
    }
}
