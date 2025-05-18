using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPressGroup : MonoBehaviour
{
    public static Action updateEvt;
    private static int m_turn = 0;
    private static int m_total = 0;

    [SerializeField] private string m_key = "";
    [SerializeField] private int m_priority = 0;
    [SerializeField] private bool m_delayExcute = false;
    [HideInInspector]
    [SerializeField] private SkeletonAnimation m_button;
    [HideInInspector]
    [SerializeField] private AudioClip m_audio;//object-switch-press

    private bool m_on;

    private void Awake()
    {
        if (m_priority == 0)
        {
            m_turn = 0;
            m_total = FindObjectsOfType<ObjectPressGroup>().Length;
        }
        updateEvt += OnUpdate;        
    }

    void Start()
    {
        gameObject.SetActive(m_turn == m_priority);
    }

    void OnEnable()
    {
        m_on = true;
        StartCoroutine(IDelayStart());
    }

    IEnumerator IDelayStart()
    {
        // m_on = false;
        yield return null;
        TrackEntry entry =   m_button.AnimationState.SetAnimation(0, "up", false);
        entry.Complete += (entry) => {m_on = false;};
       
    }

    private void OnDestroy()
    {
        updateEvt -= OnUpdate;
    }

    private void OnUpdate()
    {
        gameObject.SetActive(m_priority == m_turn);
        if(m_priority == m_turn)
            StartCoroutine(IDelayStart());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_priority != m_turn || collision.gameObject == null || collision.tag != GameTag.PLAYER)
            return;
        if (m_on)
            return;
        SoundManager.PlaySound3D(m_audio, 5, false, transform.position);
        m_on = true;
        TrackEntry entry = m_button.AnimationState.SetAnimation(0, "down", false);
        entry.Complete += (entry) =>
        {
            m_turn++;
            if (m_turn >= m_total)
                m_turn = 0;
            if(!m_delayExcute)
                updateEvt?.Invoke();
            //StartCoroutine(IDelaySwitch());
        };
        GameController.DoTrigger(m_key, true);
    }

    IEnumerator IDelaySwitch()
    {
        yield return new WaitForSeconds(2f);
        updateEvt?.Invoke();
        GameController.DoTrigger(m_key, false);
    }

    public void ResetState()
    {
        
    }
}
