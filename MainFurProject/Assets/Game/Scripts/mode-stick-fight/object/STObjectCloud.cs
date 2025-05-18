using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Spine;
using Spine.Unity;

public class STObjectCloud : MonoBehaviour
{
    [SerializeField] private ContactFilter2D m_contactFilter;
    [SerializeField] private float m_timeShow = 5;
    [SerializeField] private float m_timeHide = 2;
    [SerializeField] private AudioClip m_audioIn;//object-cloud-in
    [SerializeField] private AudioClip m_audioOut;//object-cloud-out
    [SerializeField] private bool m_invisible = false;

    private Dictionary<int, CatchObject> m_objects = new Dictionary<int, CatchObject>();
    private Rigidbody2D m_rigidbody;
    private Collider2D m_collider;
    private SkeletonAnimation m_animation;
    private State m_state;
    private bool m_loop;
    private bool m_triggerStay;

    //  private List<GameObject> m_objects = new List<GameObject>();

    private class CatchObject
    {
        public int id;
        public Rigidbody2D rigidbody;
        public Transform parent;
        public bool marked;
    }
    private enum State
    {
        Idle,
        Awake,
        Show,
        Hide
    }

    private void Awake() 
    {
        m_rigidbody = GetComponent<Rigidbody2D>(); 
    }
    private void Start() 
    {
        m_collider = GetComponent<BoxCollider2D>();
        m_animation = GetComponent<SkeletonAnimation>();
        m_state = State.Idle;
        m_loop = false;
        m_animation.AnimationState.Complete += OnAnimComplete;
        m_animation.AnimationState.SetAnimation(0, "idle", true);
        
        if(m_animation != null && m_invisible)
        {
            Color temp = Color.white;
            temp.a = 0;
            m_animation.skeleton.SetColor(temp);
        }
    }
    void OnDestroy()
    {
        if(m_animation != null)
            m_animation.AnimationState.Complete -= OnAnimComplete;
    }

    private void FixedUpdate() 
    {
        if(m_rigidbody == null)
            return;
        Catch(m_rigidbody);    
        int currentStayCount = 0;
        foreach(CatchObject c in m_objects.Values.ToList())
        {
            if(c.rigidbody == null)
                continue;
            currentStayCount++;
            TriggerStay(c.rigidbody.gameObject);
        }
        m_loop = currentStayCount > 0;
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        string animName = trackEntry.Animation.Name;
        switch (m_state)
        {
            case State.Show:
                if(animName != "on")
                    return;
                m_collider.enabled = true;
                if (m_loop)
                    StartCoroutine(Fade());
                else
                {
                    m_animation.AnimationState.SetAnimation(0, "idle", true);
                    m_state = State.Idle;
                }
                break;
            case State.Hide:
                if(animName != "off")
                    return;
                StartCoroutine(Show());
                break;
        }
    }

    void TriggerStay(GameObject go)
    {
        if(m_state != State.Idle)
            return;
        m_loop = true;
        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        m_animation.AnimationState.SetAnimation(0, "hit", true);
        m_state = State.Awake;
        yield return new WaitForSeconds(m_timeShow);
        SoundManager.PlaySound3D(m_audioOut, 5, false, transform.position);
        m_collider.enabled = false;
        m_state = State.Hide;
        m_animation.AnimationState.SetAnimation(0, "off", false);
        
    }

    IEnumerator Show()
    {
        yield return new WaitForSeconds(m_timeHide);
        SoundManager.PlaySound3D(m_audioIn, 5, false, transform.position);
        m_state = State.Show;        
        m_animation.AnimationState.SetAnimation(0, "on", false);
    }

    void Catch(Rigidbody2D rb)
    {
        // clear state
        m_triggerStay = false;
        foreach(CatchObject c in m_objects.Values.ToList())
        {
            if(c.rigidbody == null)
                m_objects.Remove(c.id);
            else
                c.marked = false;
        }

        // catch objects
        List<ContactPoint2D> contacs = new List<ContactPoint2D>();
        int contactCount = rb.GetContacts(m_contactFilter, contacs);
        for(int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = contacs[i];
            Rigidbody2D contactBody = contact.rigidbody == rb ? contact.otherRigidbody : contact.rigidbody;
            Collider2D contactCollider = contact.rigidbody == rb ? contact.otherCollider : contact.collider;
            int id = contactBody.GetInstanceID();
            if(contactBody != null)
            {
                if (contactBody.bodyType != RigidbodyType2D.Static && contactBody != m_rigidbody)
                {
                    float dot = Vector2.Dot(contact.normal, Vector2.down);
                    bool isTouch = m_rigidbody.IsTouching(contactCollider);
                    if (dot > 0.98f && isTouch)
                    {
                        if (m_objects.ContainsKey(id))
                            m_objects[id].marked = true;
                        else
                        {
                            m_objects.Add(id, new CatchObject()
                            {
                                id = id,
                                rigidbody = contactBody,
                                parent = contactBody.transform.parent,
                                marked = true
                            });
                            // contactBody.transform.SetParent(transform, true);
                            m_triggerStay = true;
                        }
                    }
                }
            }
        }
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
}
