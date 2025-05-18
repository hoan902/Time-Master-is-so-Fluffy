using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;

public class ObjectSpringsEffect : MonoBehaviour
{
    [SerializeField] private float m_force = 4500;
    [SerializeField] private SkeletonAnimation m_animator;
    [SerializeField] private AudioClip m_audio;//object-loxo
    [SerializeField] private BoxCollider2D m_boxCollider;
    [SerializeField] private float m_size = 3f;

    private static Action<bool> m_triggerEvent;
    private bool m_collid;
    private Dictionary<Rigidbody2D, float> m_timer = new Dictionary<Rigidbody2D, float>();

    void Awake()
    {
        m_triggerEvent += OnTrigger;
    }
    private void Start()
    {
        m_animator.AnimationState.Complete += OnAnimComplete;
    }

    void OnDestroy()
    {
        m_triggerEvent -= OnTrigger;

        m_animator.AnimationState.Complete += OnAnimComplete;
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        switch(trackEntry.Animation.Name)
        {
            case "activated":
                m_animator.AnimationState.SetAnimation(0, "idle", true);
                break;
        }
    }
    private void OnTrigger(bool status)
    {
        m_collid = status;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_collid)
            return;
        if (collision.collider.offset.y > 0.5f)
            return;
        if (collision.GetContact(0).point.y < (m_boxCollider.bounds.center.y + m_boxCollider.bounds.extents.y))
            return;
        Rigidbody2D body = collision.gameObject.GetComponent<Rigidbody2D>();
        if (body == null || body.bodyType == RigidbodyType2D.Static || (m_timer.ContainsKey(body) && (Time.time - m_timer[body]) < 0.5f))
            return;
        m_collid = true;
        m_triggerEvent?.Invoke(m_collid);
        m_timer[body] = Time.time;
        SoundManager.PlaySound3D(m_audio, 5, false, transform.position);
        Vector2 force = transform.up * m_force;
        body.SendMessage("OnBounce", force, SendMessageOptions.DontRequireReceiver);

        //m_effect.Play();
        m_animator.AnimationState.SetAnimation(0, "activated", false);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.offset.y > 0.5f)
            return;
        Rigidbody2D body = collision.gameObject.GetComponent<Rigidbody2D>();
        if (body == null || body.bodyType == RigidbodyType2D.Static || !m_timer.ContainsKey(body))
            return;
        m_collid = false;
        m_triggerEvent?.Invoke(m_collid);
    }

    public void UpdateSize()
    {
        m_boxCollider.size = new Vector2(m_size, m_boxCollider.size.y);
    }
}
