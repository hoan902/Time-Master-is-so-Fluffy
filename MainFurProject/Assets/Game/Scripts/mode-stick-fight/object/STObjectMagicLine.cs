using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class STObjectMagicLine : MonoBehaviour
{
    [SerializeField] private float m_spreadOutTime = 1f;
    [SerializeField] private float m_spreadInTime = 0.5f;

    private Transform m_start;
    private Transform m_end;
    private float m_baseScaleY;
    private bool m_actived;
    private Tweener m_tween;
    private float m_targetScaleX;

    private void Awake() 
    {
        m_baseScaleY = transform.localScale.y;    
        transform.localScale = new Vector3(0, m_baseScaleY, 1);

        GameController.monsterDeadEvent += OnMonsterDead;
    }
    private void OnDestroy() 
    {
        GameController.monsterDeadEvent -= OnMonsterDead;
    }

    void OnMonsterDead(GameObject monster)
    {
        if(monster.transform != m_end)
            return; 
        SpreadIn();
    }

    private void Update() 
    {
        if(m_start)
        {
            transform.position = m_start.position;
        }    
    }

    public void Init(Transform start, Transform end)
    {
        m_start = start;
        m_end = end;
        transform.position = m_start.position;
        m_targetScaleX = Vector3.Distance(m_start.position, m_end.position) * 2 - 1;
        transform.localScale = new Vector3(0, m_baseScaleY, 1);

        SpreadOut();
    }

    void SpreadOut()
    {
        Vector3 dir = (m_end.position - m_start.position).normalized;
        float angle = Mathf.Atan2(dir.x, dir.y);
        Quaternion targetRotation = Quaternion.Euler(0, 0, -angle * Mathf.Rad2Deg + 90);
        transform.rotation = targetRotation;

        m_tween?.Kill();
        float currentScaleX = 0;
        m_tween = DOTween.To(() => currentScaleX, x => currentScaleX = x, m_targetScaleX, m_spreadOutTime).OnUpdate(() => {
            transform.localScale = new Vector3(currentScaleX, m_baseScaleY);
        });
    }
    void SpreadIn()
    {
        m_tween?.Kill();
        float currentScaleX = transform.localScale.x;
        m_tween = DOTween.To(() => currentScaleX, x => currentScaleX = x, 0, m_spreadOutTime).OnUpdate(() => {
            transform.localScale = new Vector3(currentScaleX, m_baseScaleY);
        }).OnComplete(() => {
            GameController.ObjectDestroyed(this.gameObject);
            Destroy(gameObject);
        });
    }
}
