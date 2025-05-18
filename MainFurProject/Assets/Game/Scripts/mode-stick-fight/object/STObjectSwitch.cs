using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class STObjectSwitch : STObjectInteractive
{
    private static Action<Vector3, Vector3, string, bool> s_syncTransformEvent;
    private static Action<Transform> s_masterEvent;

    [SerializeField] private string[] m_keys;
    [SerializeField] private string m_syncKey = "SYNC-KEY-1";
    [SerializeField] private bool m_alwaysTrue = false;
    [SerializeField] private bool m_invert = false;
    [SerializeField] private Transform m_switch;
    [SerializeField] private float m_switchTime = 0.3f;

    private bool m_on;
    private bool m_master;
    private Tweener m_tweener;
    private float m_startAngle;

    public override void Awake()
    {
        base.Awake();
        
        m_on = false;
        m_startAngle = m_switch.transform.localEulerAngles.z;
        if(m_startAngle > 180)
            m_startAngle = m_startAngle - 360;

        s_syncTransformEvent += OnSyncTransform;
        s_masterEvent += OnMaster;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();

        s_syncTransformEvent -= OnSyncTransform;
        s_masterEvent -= OnMaster;
    }
    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        base.OnHit(attackerInfor);
        m_on = !m_on;
        foreach (var s in m_keys)
        {
            GameController.DoTrigger(s, m_alwaysTrue ? true : m_on, gameObject);
        }
        s_masterEvent?.Invoke(transform);
        UpdateState();
    }

    private void OnMaster(Transform other)
    {
        m_master = other == transform;
    }

    private void OnSyncTransform(Vector3 position, Vector3 rotation, string key, bool state)
    {
        if (m_switch == null || m_master || m_syncKey != key)
            return;
        m_switch.localPosition = position;
        m_switch.localEulerAngles = rotation;
        m_on = state;
    }

    void UpdateState()
    {
        m_tweener?.Kill();
        float targetAngle = m_on ? -45 : 45;
        float currentAngle = m_switch.transform.localEulerAngles.z;
        if(currentAngle > 180)
            currentAngle = currentAngle - 360;
        m_tweener = DOTween.To(() => currentAngle, x => currentAngle = x, targetAngle, m_switchTime).OnUpdate(() => {
            m_switch.localEulerAngles = new Vector3(0, 0, currentAngle);
            s_syncTransformEvent?.Invoke(m_switch.localPosition, m_switch.localEulerAngles, m_syncKey, m_on);
        });
    }

    public void ResetState()
    {
        m_on = false;
        m_switch.localEulerAngles = new Vector3(0, 0, m_startAngle);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (string text in m_keys)
        {
            stringBuilder.Append(text);
            stringBuilder.Append("\n");
        }

        Handles.Label(transform.position + Vector3.up, stringBuilder.ToString());
    }
#endif
}
