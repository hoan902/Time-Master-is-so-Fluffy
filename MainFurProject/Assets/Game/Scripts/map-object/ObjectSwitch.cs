
using System;
using System.Linq;
using UnityEngine;

public class ObjectSwitch : MonoBehaviour
{
    private static Action<Vector3, Vector3, string, bool> s_syncTransformEvent;
    private static Action<Transform> s_masterEvent;

    [SerializeField] private string[] m_keys;
    [SerializeField] private string m_syncKey = "SYNC-KEY-1";
    [SerializeField] private bool m_invert = false;
    [SerializeField] private Transform m_switchJoin;
    [SerializeField] private AudioClip m_audio;//object-switch

    private bool m_on;
    private bool m_master;

    void Start()
    {
        m_on = false;
        s_syncTransformEvent += OnSyncTransform;
        s_masterEvent += OnMaster;
        //
        //InitDirection();
    }

    void OnDestroy()
    {
        s_syncTransformEvent -= OnSyncTransform;
        s_masterEvent -= OnMaster;
    }

    private void OnMaster(Transform other)
    {
        m_master = other == transform;
    }

    private void OnSyncTransform(Vector3 position, Vector3 rotation, string key, bool state)
    {
        if (m_switchJoin == null || m_master || m_syncKey != key)
            return;
        m_switchJoin.localPosition = position;
        m_switchJoin.localEulerAngles = rotation;
        m_on = state;
    }

    void Update()
    { 
        float angle = m_switchJoin.localEulerAngles.z;
        bool active = m_invert ? m_on : !m_on;
        if (active)
        {
            if (angle <= 45)
            {               
                m_on = !m_invert;
                if(m_master)
                {
                    foreach (var s in m_keys)
                    {
                        SoundManager.PlaySound3D(m_audio, 5, false, transform.position);
                        GameController.DoTrigger(s, m_on, gameObject);
                    }
                }
            }
        }
        else
        {
            if (angle >= 315)
            {                
                m_on = m_invert;
                if(m_master)
                {
                    foreach (var s in m_keys)
                    {
                        SoundManager.PlaySound3D(m_audio, 5, false, transform.position);
                        GameController.DoTrigger(s, m_on, gameObject);
                    }
                }
            }
        }
        if (m_switchJoin == null || !m_master)
            return;
        s_syncTransformEvent?.Invoke(m_switchJoin.localPosition, m_switchJoin.localEulerAngles, m_syncKey, m_on);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == null || m_master)
            return;
        if (collision.tag == GameTag.PLAYER)
           s_masterEvent?.Invoke(transform);
    }

    public void InitDirection()
    {
        HingeJoint2D joint = m_switchJoin.GetComponent<HingeJoint2D>();
        JointAngleLimits2D limits = joint.limits;
        if (m_invert)
        {
            m_switchJoin.localEulerAngles = new Vector3(0, 0, 42);
            limits.min = 0;
            limits.max = 80;
        }
        else
        {
            m_switchJoin.localEulerAngles = new Vector3(0, 0, 320);
            limits.min = 0;
            limits.max = -80;
        }
        joint.limits = limits;
    }

    public void ResetState()
    {
        m_on = false;
        InitDirection();
    }
}
