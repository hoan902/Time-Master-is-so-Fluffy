using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Spine;
using Spine.Unity;

public class STObjectSprings : MonoBehaviour
{
    private static Action<bool> m_triggerEvent;
    [SerializeField] private float m_force = 4500;
    [SerializeField] private SkeletonAnimation m_animator;
    [SerializeField] private AudioClip m_audio;//object-loxo

    private bool m_collid;
    private Dictionary<Rigidbody2D, float> m_timer = new Dictionary<Rigidbody2D, float>();

    void Awake()
    {
        m_triggerEvent += OnTrigger;
    }

    void OnDestroy()
    {
        m_triggerEvent -= OnTrigger;
    }

    private void OnTrigger(bool status)
    {
        m_collid = status;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_collid)
            return;
        Rigidbody2D body = collision.GetComponent<Rigidbody2D>();
        if (body == null || body.bodyType == RigidbodyType2D.Static || (m_timer.ContainsKey(body) && (Time.time - m_timer[body]) < 0.5f))
            return;
        m_collid = true;
        m_triggerEvent?.Invoke(m_collid);
        m_timer[body] = Time.time;
        SoundManager.PlaySound3D(m_audio, 5, false, transform.position);
        Vector2 force = transform.up * m_force;
        body.SendMessage("OnBounce", force, SendMessageOptions.DontRequireReceiver);
        m_animator.AnimationState.SetAnimation(0, "touch", false);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D body = collision.GetComponent<Rigidbody2D>();
        if (body == null || body.bodyType == RigidbodyType2D.Static || !m_timer.ContainsKey(body))
            return;
        m_collid = false;
        m_triggerEvent?.Invoke(m_collid);
    }
}
