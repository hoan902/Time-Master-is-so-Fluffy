using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteShapeCatcher : MonoBehaviour
{
    [SerializeField] private Transform m_rootTransform;
    [SerializeField] private LayerMask m_layerMask;

    private Dictionary<Rigidbody2D, CatchObject> m_objects = new Dictionary<Rigidbody2D, CatchObject>();

    private Collider2D m_collider;
    private PolygonCollider2D m_poly;

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

    int TempName(Vector3 C)
    {
        float shortestArea = 500f;
        int currentPoint = -1;
        m_poly = m_collider as PolygonCollider2D;
        var curPathDetails = m_poly.points.ToList();

        for (int i = 0; i < curPathDetails.Count; i++)
        {
 
            float area;
 
            Vector2 A = new Vector2(curPathDetails[i].x, curPathDetails[i].y);
            Vector2 B = new Vector2(curPathDetails[(i + 1) % curPathDetails.Count].x, curPathDetails[(i + 1) % curPathDetails.Count].y);
 
            float vertDist = Vector2.Distance(A, B);
 
            Vector2 P = new Vector2(C.x, C.y);
            area = Mathf.Abs((A.x * (B.y - P.y) + B.x * (P.y - A.y) + P.x * (A.y - B.y)) / 2);
 
         
            if (area < shortestArea)
            {
         
                if (Vector2.Distance(A, P) < vertDist && Vector2.Distance(B, P) < vertDist)
                {
                    shortestArea = area;
                    currentPoint = i;
                }
            }
        }

        return currentPoint;
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
