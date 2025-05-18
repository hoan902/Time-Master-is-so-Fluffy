using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ObjectMace : MonoBehaviour
{
    [SerializeField] private float m_timeWaitStart = 2f;
    [SerializeField] private float m_timeMove = 2f;
    [SerializeField] private float m_timeWaitEnd = 2f;
    [SerializeField] private int m_healthReduce = 20;
    [SerializeField] private string m_triggerKey = "";
    [SerializeField] private Ease m_ease;
    [HideInInspector]
    [SerializeField] private Transform m_body;
    [HideInInspector]
    [SerializeField] private SpriteRenderer m_line;
    [HideInInspector]
    [SerializeField] private LayerMask m_layer;
    [HideInInspector]
    [SerializeField] private AudioClip m_audioOpen;//object-door-switch
    [HideInInspector]
    [SerializeField] private AudioClip m_audioActive;//object-mace
    [SerializeField] private bool m_alwaysShake = false;

    private Vector2 m_startPos;
    private Vector2 m_endPos;
    private Tweener m_tween;
    private bool m_shake = false;
    private GameObject m_sound;
    private float m_range;
    private bool m_stop;
    private Tween m_hShakeTween;
    private bool m_activeByTrigger;

    public bool AlwaysShake{get => m_alwaysShake;}

    void Start()
    {
        m_range = GetComponent<BoxCollider2D>().size.x;
        m_startPos = m_body.position;
        BoxCollider2D collider = m_body.GetComponent<BoxCollider2D>();
        RaycastHit2D ray = Physics2D.Raycast(collider.bounds.center, -m_body.up, Mathf.Infinity, m_layer);
        if (ray.collider != null)
            m_endPos = ray.point - (Vector2)(collider.size.y / 2 * (-m_body.up));
        m_stop = true;

        m_activeByTrigger = m_triggerKey != "";
        if(m_activeByTrigger)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            GameController.triggerEvent += OnTrigger;
        }
    }
    private void OnDestroy() 
    {
        GameController.triggerEvent -= OnTrigger;
    }
#if UNITY_EDITOR
    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Alpha0) && m_activeByTrigger)
        {
            if(!m_stop)
                return;
            m_shake = true;
            m_stop = false;
            Move();
        }
               
    }
#endif

    void OnTrigger(string key, bool state, GameObject triggerSource)
    {
        if(key != m_triggerKey || state == false)
            return;
        if(!m_stop)
            return;
        m_shake = true;
        m_stop = false;
        Move();
    }

    IEnumerator WaitStart()
    {
        m_stop = false;
        if(m_alwaysShake)
        {
            m_hShakeTween?.Kill();
            m_hShakeTween = transform.DOShakePosition(0.1f, new Vector3(0.15f, 0, 0)).SetLoops(-1);
        }   
        yield return new WaitForSeconds(m_timeWaitStart);
        m_hShakeTween?.Kill();
        Move();
        if (m_sound == null)
            m_sound = SoundManager.PlaySound3D(m_audioOpen, m_range, true, transform.position);
    }

    void Move()
    {
        if (m_tween == null)
        {
            m_tween = m_body.DOMove(m_endPos, m_timeMove).SetEase(m_ease).SetAutoKill(false).OnComplete(() =>
            {
                if (m_shake || m_alwaysShake)
                    GameController.ShakeCamera();
                StartCoroutine(WaitEnd());
                Destroy(m_sound);
                GameObject go = SoundManager.PlaySound3D(m_audioActive, m_range, false, transform.position);
                go.transform.position = m_body.position;
            }).OnUpdate(() =>
            {
                m_line.size = new Vector2(m_line.size.x, Vector2.Distance(m_body.position, m_startPos) + 2);
            });
            m_tween.OnRewind(() =>
            {
                if (m_shake && !m_activeByTrigger)
                    StartCoroutine(WaitStart());
                else
                    m_stop = true;
                Destroy(m_sound);
            });
        }
        else
            m_tween.PlayForward();
    }
    void MoveByTrigger()
    {
        m_tween = m_body.DOMove(m_endPos, m_timeMove).SetEase(m_ease).OnComplete(() =>
        {
            if (m_shake || m_alwaysShake)
                GameController.ShakeCamera();
            StartCoroutine(WaitEnd());
            Destroy(m_sound);
            GameObject go = SoundManager.PlaySound3D(m_audioActive, m_range, false, transform.position);
            go.transform.position = m_body.position;
        }).OnUpdate(() =>
        {
            m_line.size = new Vector2(m_line.size.x, Vector2.Distance(m_body.position, m_startPos) + 2);
        });
    }

    IEnumerator WaitEnd()
    {
        if (m_timeWaitEnd < 0)
            yield break;
        yield return new WaitForSeconds(m_timeWaitEnd);
        if (m_tween != null)
        {
            if (m_sound == null)
                m_sound = SoundManager.PlaySound3D(m_audioOpen, m_range, true, transform.position);
            m_tween.PlayBackwards();
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == GameTag.PLAYER)
        {
            m_shake = true;
            if (m_stop)
                StartCoroutine(WaitStart());
        }
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.tag == GameTag.PLAYER)
            m_shake = false;
    }

    public int GetDamage()
    {
        return m_healthReduce;
    }
}
