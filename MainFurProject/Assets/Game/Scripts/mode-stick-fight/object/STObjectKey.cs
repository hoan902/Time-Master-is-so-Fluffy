using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine;
using Spine.Unity;

public class STObjectKey : ObjectFollowable
{
    [SerializeField] private string m_key = "";
    [SerializeField] private GameObject m_effect;
    [SerializeField] private AudioClip m_audio;//collect-item
    [SerializeField] float m_smoothTime = 3f;
    [SerializeField] GameObject m_lightEff;

    private bool m_stop;
    private bool m_active;
    private Vector3 m_baseScale;
    private STPlayerController m_target;
    private Vector3 velocity;

    void Awake()
    {
        m_baseScale = transform.localScale;

        followableObjectCollectEvent += OnFollowableObjectCollected;
    }

    void Start()
    {
        m_stop = false;
        m_active = false;
        GameController.keyActiveEvent += OnActive;
        //
        transform.DOScale(new Vector3(m_baseScale.x + 0.15f, m_baseScale.y + 0.15f, 1), 0.2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
    }

    void OnDestroy()
    {
        transform.DOKill();
        GameController.keyActiveEvent -= OnActive;
        followableObjectCollectEvent -= OnFollowableObjectCollected;
    }

    private void Update() 
    {
        if(!m_target)
            return;
        Vector3 target = m_target.transform.position;
        target += new Vector3(-1 * index, 2f, 0);
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, m_smoothTime * index);
    }

    private void OnActive(string key, Vector2 position)
    {
        if(key != m_key || m_active)
            return;
        m_target = null;
        m_active = true;
        GameController.RemoveKey(m_key);
        transform.DOMove(position, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            GameController.DoTrigger(m_key, true);

            Destroy(gameObject);
        });
    }
    void OnFollowableObjectCollected(GameObject collectedObject)
    {
        followableObjectsCollectedCount++;
        if(collectedObject == this.gameObject && !collected)
        {
            index = followableObjectsCollectedCount;
            collected = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == null || m_stop)
            return;
        if (collision.tag == GameTag.PLAYER)
        {
            SoundManager.PlaySound3D(m_audio, 5, false, transform.position);
            transform.DOKill();
            transform.localScale = m_baseScale;
            m_stop = true;
            m_effect.transform.SetParent(transform.parent, false);
            m_effect.transform.position = transform.position;
            m_effect.SetActive(true);
            m_effect.GetComponent<SkeletonAnimation>().AnimationState.Complete += (entry) =>
            {
                GameController.PickupKey(m_key);
                Destroy(m_effect);
            };
            followableObjectCollectEvent?.Invoke(this.gameObject);
            if(!m_target)
            {
                STPlayerController player = collision.gameObject.GetComponent<STPlayerController>();
                m_target = player;
                transform.DOScale(Vector3.one, 0.5f);
                m_lightEff.SetActive(true);
            }
        }
    }
}
