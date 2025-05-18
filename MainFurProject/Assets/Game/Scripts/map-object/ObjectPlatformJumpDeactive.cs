using Spine.Unity;
using Spine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPlatformJumpDeactive : MonoBehaviour
{
    [SerializeField] private ContactFilter2D m_contactFilter;
    [SerializeField] private float m_duration = 5f;
    [SerializeField] private AudioClip m_audioIn;//object-cloud-in
    [SerializeField] private AudioClip m_audioOut;//object-cloud-out
    [SerializeField] private bool m_invisible = false;
    [SerializeField] private SkeletonAnimation m_animation;

    [HideInInspector]
    [SerializeField] private string m_animIdle;
    [HideInInspector]
    [SerializeField] private string m_animCollision;
    [HideInInspector]
    [SerializeField] private string m_animBroken;
    [HideInInspector]
    [SerializeField] private string m_animDisappear;
    [HideInInspector]
    [SerializeField] private string m_animRevive;

    private Dictionary<int, CatchObject> m_objects = new Dictionary<int, CatchObject>();
    private Rigidbody2D m_rigidbody;
    private Collider2D m_collider;
    private bool m_hasPlayer;

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
        m_animation.AnimationState.SetAnimation(0, m_animIdle, true);

        if (m_animation != null && m_invisible)
        {
            Color temp = Color.white;
            temp.a = 0;
            m_animation.skeleton.SetColor(temp);
        }
    }
    private void OnDestroy()
    {

    }

    private void FixedUpdate()
    {
        if (m_rigidbody == null || !m_collider.enabled)
            return;
        m_hasPlayer = m_objects.Count > 0;
        Catch(m_rigidbody);
        if(m_objects.Count == 0 && m_hasPlayer)
        {
            TriggerExit();
        }
    }

    void TriggerExit()
    {
        Fade();
    }
    void Fade()
    {
        SoundManager.PlaySound3D(m_audioOut, 5, false, transform.position);
        m_collider.enabled = false;
        
        TrackEntry brokenTrack = m_animation.AnimationState.SetAnimation(0, m_animBroken, false);
        brokenTrack.Complete += (brokenTrack) =>
        {
            StartCoroutine(Show());
            m_animation.AnimationState.SetAnimation(0, m_animDisappear, true);
        };
    }

    IEnumerator Show()
    {
        yield return new WaitForSeconds(m_duration);
        SoundManager.PlaySound3D(m_audioIn, 5, false, transform.position);

        TrackEntry reviveTrack = m_animation.AnimationState.SetAnimation(0, m_animRevive, false);
        reviveTrack.Complete += (reviveTrack) =>
        {
            m_collider.enabled = true;
            m_animation.AnimationState.SetAnimation(0, m_animIdle, true);
        };
    }

    void Catch(Rigidbody2D rb)
    {
        // clear state
        foreach (CatchObject c in m_objects.Values.ToList())
        {
            if (c.rigidbody == null)
                m_objects.Remove(c.id);
            else
                c.marked = false;
        }

        // catch objects
        List<ContactPoint2D> contacs = new List<ContactPoint2D>();
        int contactCount = rb.GetContacts(m_contactFilter, contacs);
        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = contacs[i];
            Rigidbody2D contactBody = contact.rigidbody == rb ? contact.otherRigidbody : contact.rigidbody;
            Collider2D contactCollider = contact.rigidbody == rb ? contact.otherCollider : contact.collider;
            int id = contactBody.GetInstanceID();
            if (contactBody != null)
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
