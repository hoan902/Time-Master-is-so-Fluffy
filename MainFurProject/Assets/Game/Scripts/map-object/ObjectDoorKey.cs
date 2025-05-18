
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ObjectDoorKey : MonoBehaviour
{
    [SerializeField] private string m_key = "";
    [HideInInspector]
    [SerializeField] private Transform m_top;
    [HideInInspector]
    [SerializeField] private Transform m_bot;
    [HideInInspector]
    [SerializeField] private BoxCollider2D m_topCollider;
    [HideInInspector]
    [SerializeField] private BoxCollider2D m_botCollider;
    [HideInInspector]
    [SerializeField] private AudioClip m_audioOpen;

    private bool m_stop;
    private bool m_trigger;
    private BoxCollider2D m_collider;
    
    void Start()
    {
        m_stop = false;
        m_trigger = false;
        m_collider = GetComponent<BoxCollider2D>();
        GameController.triggerEvent += OnOpen;
    }

    void OnDestroy()
    {
        GameController.triggerEvent -= OnOpen;
        transform.DOKill();
    }

    private void OnOpen(string key, bool state, GameObject triggerSource)
    {
        if (key != m_key || m_stop)
            return;
        GameObject sound = SoundManager.PlaySound3D(m_audioOpen, 10, true, transform.position);
        m_stop = true;
        m_top.DOLocalMoveY(2.5f, 0.5f).OnComplete(() =>
        {
            m_topCollider.enabled = false;
            Destroy(sound);
        });
        m_bot.DOLocalMoveY(-2.5f, 0.5f).OnComplete(() =>
        {
            m_botCollider.enabled = false;
            m_collider.enabled = false;
        });
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == null || m_stop || m_trigger || !MainModel.gameInfo.HasKey(m_key))
            return;
        if (collision.tag != GameTag.PLAYER)
            return;
        m_trigger = true;
        GameController.ActiveKey(m_key, transform.position);
    }
}
