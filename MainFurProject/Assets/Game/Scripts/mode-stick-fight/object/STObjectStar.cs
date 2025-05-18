using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using DG.Tweening;

public class STObjectStar : MonoBehaviour
{
    [SerializeField] private GameObject m_effect;
    [SerializeField] private GameObject m_text;
    [SerializeField] private AudioClip m_audio;//collect-item

    private bool m_stop;

    void Start()
    {
        m_stop = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == null || m_stop || collision.tag != GameTag.PLAYER)
            return;
        STPlayerController player = collision.gameObject.GetComponent<STPlayerController>();
        if (player == null)
            return;
        SoundManager.PlaySound(m_audio, false);
        m_stop = true;
        DoTrigger();
        GameController.UpdatePoint(1, transform.position, player.transform.localScale.x > 0 ? 1 : -1);
    }

    private void DoTrigger()
    {
        TextMeshPro text = m_text.GetComponent<TextMeshPro>();
        text.text = "" + LeaderBoardConstant.DEFAULT_POINT_STAR;
        m_text.SetActive(true);
        Transform targetParent = transform.parent;
        if(!targetParent.GetComponent<SortingGroup>())
            targetParent = targetParent.parent;
        m_text.transform.SetParent(targetParent, true);
        m_text.transform.localEulerAngles = Vector3.zero;
        m_text.transform.DOMoveY(transform.position.y + 0.5f, 0.5f).OnComplete(() =>
        {
            text.DOFade(0, 0.5f).OnComplete(() =>
            {
                m_text.transform.DOKill();
                text.DOKill();
                Destroy(m_text);
            });
        });
        Destroy(gameObject);
    }
}
