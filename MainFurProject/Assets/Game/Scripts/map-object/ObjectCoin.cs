
using System;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class ObjectCoin : MonoBehaviour
{
    [SerializeField] private GameObject m_effect;
    [SerializeField] private GameObject m_text;
    [SerializeField] private AudioClip m_audioCollect;

    private bool m_stop = true;
    private Transform m_target;

    void Awake()
    {
        GameController.readyPlayEvent += OnReadyPlay;
        m_stop = true;
    }
    void OnDestroy()
    {
        GameController.readyPlayEvent -= OnReadyPlay;
    }

    private void OnReadyPlay()
    {
        m_stop = false;
    }
    private void OnEnable() 
    {
        m_stop = false;    
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == null || m_stop)
            return;
        if (collision.tag == GameTag.MAGNET && m_target == null)
        {
            m_target = collision.transform;
        }
        if (collision.tag == GameTag.PLAYER)
            DoTrigger();
    }

    void DoTrigger()
    {
        SoundManager.PlaySound(m_audioCollect, false);
        m_stop = true;
        m_effect.transform.parent.SetParent(transform.parent, false);
        m_effect.transform.parent.position = transform.position;
        m_effect.transform.parent.gameObject.SetActive(true);
        m_effect.GetComponent<SkeletonAnimation>().AnimationState.Complete += (entry) =>
        {
            Destroy(m_effect.transform.parent.gameObject);
        };
        //effect text
        TextMeshPro text = m_text.GetComponent<TextMeshPro>();
        text.text = "+" + MapConstant.COIN_RATIO;
        m_text.SetActive(true);
        m_text.transform.SetParent(transform.parent, true);
        m_text.transform.localEulerAngles = Vector3.zero;
        m_text.transform.DOMoveY(transform.position.y + 1.5f, 0.5f).OnComplete(()=>{
            text.DOFade(0, 0.5f).OnComplete(()=>{
                m_text.transform.DOKill();
                text.DOKill();
                Destroy(m_text);
            });
        });
        //
        GameController.UpdateCoin(1);
        GameController.ObjectDestroyed(this.gameObject);
        Destroy(gameObject);
    }

    void Update()
    {
        if(m_target == null || m_stop)
            return;
        transform.position = Vector3.Lerp(transform.position, m_target.position, 0.2f);
    }
}
