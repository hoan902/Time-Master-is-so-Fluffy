using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ObjectLaser : MonoBehaviour
{
    [SerializeField] private float m_timeRotate = 1f;
    [SerializeField] private bool m_useLimits = true;
    [SerializeField] private Vector2 m_minAngle;
    [SerializeField] private Vector2 m_maxAngle;
    [SerializeField] private bool m_clockwise;
    [SerializeField] private List<SpriteRenderer> m_lines;
    [SerializeField] private Transform m_startPoint;
    [SerializeField] private List<Transform> m_endPoints;
    [SerializeField] private LayerMask m_layerMask;
    [SerializeField] private ParticleSystem m_fx;
    [SerializeField] private bool m_lockRotate = false;

    private List<ParticleSystem> m_listEff;
    [SerializeField] private bool m_actived = true;

    void Start()
    {
        m_actived = true;
        m_listEff = new List<ParticleSystem>();
        for (int i = 0; i < m_lines.Count; i++)
        {
            Vector2 direction = m_endPoints[i].position - m_startPoint.position;
            m_lines[i].transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            m_lines[i].transform.position = m_endPoints[i].position;

            ParticleSystem eff = Instantiate(m_fx.gameObject, transform).GetComponent<ParticleSystem>();
            eff.gameObject.SetActive(true);
            m_listEff.Add(eff);
        }
        if(m_lockRotate)
            return;
        if (m_useLimits)
        {
            Vector2 minDirection = m_minAngle - (Vector2)m_startPoint.position;
            Vector2 maxDirection = m_maxAngle - (Vector2)m_startPoint.position;
            float minAngle = Mathf.Atan2(minDirection.y, minDirection.x) * Mathf.Rad2Deg;
            float maxAngle = Mathf.Atan2(maxDirection.y, maxDirection.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, minAngle);
            transform.DORotate(new Vector3(0, 0, maxAngle), m_timeRotate, RotateMode.Fast).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            transform.DORotate(new Vector3(0, 0, m_clockwise ? -360 : 360), m_timeRotate, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
        }
    }

    void OnDestroy()
    {
        transform.DOKill();
    }

    void Update()
    {
        if(!m_actived)
            return;
        for (int i = 0; i < m_lines.Count; i++)
        {
            Vector2 direction = m_endPoints[i].position - m_startPoint.position;
            RaycastHit2D hit = Physics2D.Raycast(m_startPoint.position, direction, Mathf.Infinity, m_layerMask);
            if (hit.collider == null)
            {
                Vector2 size = m_lines[i].size;
                size.x = 100;
                m_lines[i].size = size;
                if(m_listEff[i].isPlaying)
                    m_listEff[i].Stop();
            }
            else
            {
                float distance = (hit.point - (Vector2)m_endPoints[i].position).magnitude;
                Vector2 size = m_lines[i].size;
                size.x = distance;
                m_lines[i].size = size;
                if (hit.collider.tag == GameTag.PLAYER)
                {
                    GameController.UpdateHealth(-MainModel.gameInfo.health);
                }
                else
                {
                    MonsterWeakPoint monsterWeakPoint = hit.collider.GetComponent<MonsterWeakPoint>();
                    if(monsterWeakPoint != null)
                        hit.collider.GetComponent<MonsterWeakPoint>().OnHit();
                }
                if(m_listEff[i].isStopped)
                    m_listEff[i].Play();
                m_listEff[i].transform.position = hit.point;
            }
        }
    }

    public void Active(bool toActive)
    {
        m_actived = toActive;
        if(!m_actived)
        {
            for (int i = 0; i < m_lines.Count; i++)
            {
                Vector2 size = m_lines[i].size;
                size.x = 0;
                m_lines[i].size = size;
                if(m_listEff[i].isPlaying)
                    m_listEff[i].Stop();
            }
        }
        
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying || !m_useLimits)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(m_startPoint.position, m_minAngle);
        Gizmos.DrawLine(m_startPoint.position, m_maxAngle);
    }
}
