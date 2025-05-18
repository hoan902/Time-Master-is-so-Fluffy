using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageUI : MonoBehaviour
{
    [SerializeField] private GameObject m_content;

    private Vector3 m_startPos;

    void Awake()
    {
        m_startPos = m_content.transform.localPosition;
        //
        MainController.noticeEvent += OnNotice;
    }

    private void OnDestroy()
    {
        MainController.noticeEvent -= OnNotice;
    }

    void OnNotice(string message)
    {
        m_content.transform.Find("text").GetComponent<TextMeshProUGUI>().text = message;
        m_content.transform.DOKill();
        m_content.transform.localPosition = m_startPos;
        m_content.SetActive(true);
        float y = m_content.transform.localPosition.y;
        float destinationY = y - 300 - ((MainModel.currentSceneType == SceneType.Game && MainModel.inCutscene) ? 200 : 0);
        m_content.transform.DOLocalMoveY(destinationY, 0.5f).OnComplete(() =>
        {
            m_content.transform.DOLocalMoveY(y, 0.5f).SetDelay(2f).OnComplete(() =>
            {
                m_content.SetActive(false);
            }).SetUpdate(true);
        }).SetUpdate(true);
    }
}
