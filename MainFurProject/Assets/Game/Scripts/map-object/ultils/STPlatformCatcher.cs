using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class STPlatformCatcher : MonoBehaviour
{
    [SerializeField] private ContactFilter2D m_contactFilter;
    
    private Dictionary<int, CatchObject> m_objects = new Dictionary<int, CatchObject>();

    private Rigidbody2D m_rigidbody;

    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (m_rigidbody == null)
            return;
        Catch(m_rigidbody);
    }

    void Catch(Rigidbody2D rb)
    {
        //clear state
        foreach (CatchObject c in m_objects.Values.ToList())
        {
            if (c.rigidbody == null)
                m_objects.Remove(c.id);
            else
                c.marked = false;
        }
        
        //catch objects
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();
        int contactCount = rb.GetContacts(m_contactFilter, contacts);
        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = contacts[i];
            Rigidbody2D contactBody = contact.rigidbody == rb ? contact.otherRigidbody : contact.rigidbody;
            Collider2D contactCollider = contact.rigidbody == rb ? contact.otherCollider : contact.collider;
            int id = contactBody.GetInstanceID();
            if (contactBody != null)
            {
                if (contactBody.bodyType != RigidbodyType2D.Static && contactBody != m_rigidbody)
                {
                    float dot = Vector2.Dot(contact.normal, Vector2.down);
                    bool isTouch = m_rigidbody.IsTouching(contactCollider);
                    if (dot > 0.9f && isTouch)
                    {
                        if (m_objects.ContainsKey(id))
                            m_objects[id].marked = true;
                        else
                        {
                            if(contactBody.GetComponentInParent<STPlatformCatcher>() != null)
                                continue;
                            m_objects.Add(id, new CatchObject()
                            {
                                id = id,
                                rigidbody = contactBody,
                                parent = contactBody.transform.parent,
                                marked = true
                            });
                            contactBody.transform.SetParent(transform, true);
                        }
                    }
                }
            }
        }
        
        //clear object
        foreach (CatchObject c in m_objects.Values.ToList().Where(c => !c.marked))
        {
            if (c.rigidbody == null)
                m_objects.Remove(c.id);
            else
            {
                c.rigidbody.transform.SetParent(c.parent, true);
                m_objects.Remove(c.id);
            }
        }
    }

    public void RemovePlayer()
    {
        foreach (CatchObject c in m_objects.Values.ToList())
        {
            if (c.rigidbody.tag == GameTag.PLAYER)
            {
                c.rigidbody.transform.SetParent(c.parent, true);
                m_objects.Remove(c.id);
            }
        }
    }

    private class CatchObject
    {
        public int id;
        public Rigidbody2D rigidbody;
        public Transform parent;
        public bool marked;
    }
}
