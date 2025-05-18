using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutSceneUI : MonoBehaviour
{
    [SerializeField] private GameObject m_board;
    [SerializeField] private TextMeshProUGUI m_textMessage;
    [SerializeField] private TextMeshProUGUI m_textOk;
    [SerializeField] private TextMeshProUGUI m_textCancel;
    //[SerializeField] private TextMeshProUGUI m_textTime;
    [SerializeField] private GameObject m_iconAds;
    [SerializeField] private SlicedFilledImage m_timeProgress;
    [SerializeField] private GameObject m_buttonCancel;
    [SerializeField] private GameObject m_buttonOk;
    [SerializeField] private GameObject m_messageBox;

    private bool m_showAds;
    private bool m_cutscenePaused;

    void Awake()
    {
        CutSceneController.boardEvent += OnBoard;
    }

    void OnDestroy()
    {
        CutSceneController.boardEvent -= OnBoard;
    }

    private void OnBoard(string message, string textOk, string textCancel, bool showAds, int timeWait, bool cutscenePaused)
    {
        StopAllCoroutines();
        m_showAds = showAds;
        m_cutscenePaused = cutscenePaused;
        m_board.SetActive(true);
        m_timeProgress.transform.parent.gameObject.SetActive(timeWait > 0);
        m_iconAds.gameObject.SetActive(showAds);
        m_messageBox.SetActive(message != "");
        m_buttonCancel.SetActive(textCancel != "");
        //
        m_textMessage.text = message;
        m_textOk.text = textOk;
        m_textCancel.text = textCancel;
        //
        int buttonCutsceneStyle = 1;
        ButtonStyle style = ConfigLoader.instance.buttonStyleConfig.styles[buttonCutsceneStyle];
        m_buttonOk.transform.Find("bg").GetComponent<Image>().sprite = style.buttonOkSprite;
        m_timeProgress.sprite = style.processSprite;
        if(buttonCutsceneStyle > 0)
        {
            m_timeProgress.color = style.color;
        }
            
        
        Animation anim = m_buttonOk.GetComponentInChildren<Animation>();
        if (showAds)
            anim.Play();
        else
        {
            anim.Stop();
            anim.transform.localScale = Vector3.one;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_buttonOk.transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_buttonCancel.transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_buttonOk.transform.parent as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_messageBox.transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_board.transform as RectTransform);
        if (timeWait > 0)
            StartCoroutine(IWait(timeWait));
        //else
            //m_textTime.text = "";
    }

    IEnumerator IWait(int time)
    {
        //m_textTime.text = "(" + time + "s)";
        m_timeProgress.fillAmount = 1;
        float startTime = Time.time;
        float del = 0;
        while (time > del)
        {            
            yield return null;
            //time--;
            //m_textTime.text = "(" + time + "s)";
            del = Time.time - startTime;
            m_timeProgress.fillAmount = (time - del) / time;
        }
        m_board.SetActive(false);
        yield return new WaitForSeconds(1f);
        OnBoardCancel();
    }

    public void OnBoardOk()
    {
        StopAllCoroutines();
        m_board.SetActive(false);
        CutSceneController.DoBoardResult(true);
    }

    public void OnBoardCancel()
    {
        m_board.SetActive(false);
        CutSceneController.DoBoardResult(false);
    }
}
