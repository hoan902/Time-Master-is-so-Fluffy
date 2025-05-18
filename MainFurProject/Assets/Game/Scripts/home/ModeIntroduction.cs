using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using DG.Tweening;
using UnityEngine.UI;

public class ModeIntroduction : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float m_moveInTime = 2f;
    [SerializeField] private float m_moveOutTime = 2f;
    [SerializeField] private float m_idleTime = 2f;

    [Header("Reference")]
    [SerializeField] private Transform m_panel;
    [SerializeField] private RectTransform m_ramadan;
    [SerializeField] private RectTransform m_redBlue;
    [SerializeField] private RectTransform m_stickFight;
    [SerializeField] private Transform m_buttonModeSelection;
    [SerializeField] private Vector2 m_iconStartPos;

    private RectTransform m_objectToShow;
    private Image m_mask;
    private List<PlayMode> m_toShowModes;
    private bool m_showing;

    private void Awake() 
    {
        m_showing = false;
        m_toShowModes = new List<PlayMode>();
        m_mask = m_panel.GetComponent<Image>();    
    }
    private void Start() 
    {
        m_panel.gameObject.SetActive(false);
        DisableAllIcons(); 

        MainController.introduceNewModeEvent += OnActive;
    }
    private void OnDestroy() 
    {
        MainController.introduceNewModeEvent -= OnActive;
    }

    void OnActive(PlayMode mode, bool toShow)
    {
        if(toShow)
            m_toShowModes.Add(mode);
        else
            m_toShowModes.Remove(mode);
        
        bool show = toShow || (!toShow && m_toShowModes.Count > 0);

        m_panel.gameObject.SetActive(show);
        if(!show || m_showing)
            return;
        m_showing = true;
        PlayMode modeToShow = m_toShowModes[0];

        ConfigLoader.instance.SavePlayedModes(modeToShow, false);
        DisableAllIcons();

        Color black = Color.black;
        black.a = 0.6f;
        m_mask.color = black;

        switch(modeToShow)
        {

        }
        m_objectToShow.gameObject.SetActive(true);
        m_objectToShow.anchoredPosition = m_iconStartPos;
        m_objectToShow.localScale = Vector3.one;

        m_objectToShow.DOAnchorPos(Vector2.zero, m_moveInTime).SetEase(Ease.OutBack);
        m_objectToShow.transform.DOMove(m_buttonModeSelection.position, m_moveOutTime).SetDelay(m_idleTime);
        m_objectToShow.DOScale(Vector3.zero, m_moveOutTime).SetDelay(m_idleTime).OnComplete(() => {
            MainController.ActiveHandOnButton(PopupType.ModeSelection, true);
            m_showing = false;
            MainController.IntroduceNewMode(modeToShow, false);
        });
        m_mask.DOFade(0, 0.8f).SetDelay(m_idleTime);
    }

    void DisableAllIcons()
    {
        for(int i= 0; i < m_panel.childCount; i++)
        {
            m_panel.GetChild(i).gameObject.SetActive(false);
            m_panel.GetChild(i).GetComponent<RectTransform>().anchoredPosition = m_iconStartPos;
        }
    }
}
