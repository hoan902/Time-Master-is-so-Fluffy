using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private GameObject m_blockInput;
    [SerializeField] private RectTransform m_top;
    [SerializeField] private RectTransform m_bot;
    [SerializeField] private RectTransform m_canvas;
    [SerializeField] private MaskVController m_maskV;
    [SerializeField] private GameObject m_fastForward;
    private bool m_isClose;//mask is close or open
    private Action m_action;

    // public static bool useMaskVTest = false;

    void Awake()
    {
        m_fastForward.SetActive(false);
        m_maskV.gameObject.SetActive(false);
        MainController.sceneTransitionEvent += OnTransition;
        CutSceneController.transitionEvent += OnCutSceneTransition;
        CutSceneController.finishEvent += OnCutSceneFinish;
        CutSceneController.activateFastForwardEvent += OnActivateFastForwardCutscene;
        GameController.teleportEvent += OnTeleport;
        GameController.skipCutsceneEvent += OnStartSkipCutscene;
        //
        m_isClose = true;
        
    }

    void Start()
    {
        MainController.ActiveLoading(true, 1);
    }

    void OnDestroy()
    {
        MainController.sceneTransitionEvent -= OnTransition;
        CutSceneController.transitionEvent -= OnCutSceneTransition;
        CutSceneController.finishEvent -= OnCutSceneFinish;
        CutSceneController.activateFastForwardEvent -= OnActivateFastForwardCutscene;
        GameController.teleportEvent -= OnTeleport;
        GameController.skipCutsceneEvent -= OnStartSkipCutscene;
    }

    public void OnActivateFastForwardCutscene(bool toActive)
    {
        m_fastForward.SetActive(toActive);
    }
    public void MaskVCloseComplete()
    {
        MainModel.readyToCheckInternet = true;
        m_action?.Invoke();
        m_blockInput.SetActive(false);
        if(!m_isClose)
            m_isClose = true;
    }
    public void MaskVOpenComplete()
    {
        MainModel.readyToCheckInternet = true;
        m_action?.Invoke();
        m_blockInput.SetActive(false);
        if(m_isClose)
            m_isClose = false;
    }

    private void OnTransition(bool isOpen, Action action)
    {
        m_action = action;
        m_blockInput.SetActive(true);
        float height = m_canvas.sizeDelta.y / 2;
        float time = 0.5f;
        MainModel.readyToCheckInternet = false;
        StopAllCoroutines();
        m_top.DOKill();
        m_bot.DOKill();
        
        if(m_maskV.gameObject.activeSelf)
        {
            m_top.sizeDelta = new Vector2(m_top.sizeDelta.x, 0);
            m_bot.sizeDelta = new Vector2(m_bot.sizeDelta.x, 0);
            if(isOpen)
            {
                MainController.ActiveLoading(false, 1);
                m_maskV.TriggerMask("out");
            }
            else
            {
                m_maskV.TriggerMask("in");
            }
            return;
        }
        
        if (isOpen)
        {    
            MainController.ActiveLoading(false, 1);
            if (m_isClose)
            {
                m_top.sizeDelta = new Vector2(m_top.sizeDelta.x, height);
                m_bot.sizeDelta = new Vector2(m_bot.sizeDelta.x, height);
                m_top.DOSizeDelta(new Vector2(m_top.sizeDelta.x, 0), time).SetEase(Ease.Linear);
                m_bot.DOSizeDelta(new Vector2(m_bot.sizeDelta.x, 0), time).SetEase(Ease.Linear).OnComplete(() =>
                {
                    MainModel.readyToCheckInternet = true;
                    action?.Invoke();
                    m_blockInput.SetActive(false);
                    m_isClose = false;                    
                });
            }
            else
            {
                MainModel.readyToCheckInternet = true;
                action?.Invoke();
                m_blockInput.SetActive(false);
            }
        }
        else
        {
            if (m_isClose)
            {
                MainModel.readyToCheckInternet = true;
                action?.Invoke();
                m_blockInput.SetActive(false);
                StartCoroutine(IShowLoading());
            }
            else
            {
                m_top.sizeDelta = new Vector2(m_top.sizeDelta.x, 0);
                m_bot.sizeDelta = new Vector2(m_bot.sizeDelta.x, 0);
                m_top.DOSizeDelta(new Vector2(m_top.sizeDelta.x, height), time).SetEase(Ease.Linear);
                m_bot.DOSizeDelta(new Vector2(m_bot.sizeDelta.x, height), time).SetEase(Ease.Linear).OnComplete(() =>
                {
                    MainModel.readyToCheckInternet = true;
                    action?.Invoke();
                    m_blockInput.SetActive(false);
                    m_isClose = true;
                    StartCoroutine(IShowLoading());
                });
            }
        }
    }

    void OnTeleport(Vector3 destination)
    {
        m_blockInput.SetActive(true);
        float height = m_canvas.sizeDelta.y / 2;
        m_top.DOKill();
        m_bot.DOKill();
        m_top.sizeDelta = new Vector2(m_top.sizeDelta.x, 0);
        m_bot.sizeDelta = new Vector2(m_bot.sizeDelta.x, 0);
        m_top.DOSizeDelta(new Vector2(m_top.sizeDelta.x, height), 0.3f).SetEase(Ease.Linear);
        m_bot.DOSizeDelta(new Vector2(m_bot.sizeDelta.x, height), 0.3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            GameController.MaskTeleClosed(destination);
            m_top.DOSizeDelta(new Vector2(m_top.sizeDelta.x, 0), 0.3f).SetEase(Ease.Linear).SetDelay(1f);
            m_bot.DOSizeDelta(new Vector2(m_bot.sizeDelta.x, 0), 0.3f).SetEase(Ease.Linear).SetDelay(1f).OnComplete(() => m_blockInput.SetActive(false));
        });
    }
    void OnStartSkipCutscene()
    {
        m_blockInput.SetActive(true);
        float height = m_canvas.sizeDelta.y / 2;
        m_top.DOKill();
        m_bot.DOKill();
        m_top.sizeDelta = new Vector2(m_top.sizeDelta.x, 0);
        m_bot.sizeDelta = new Vector2(m_bot.sizeDelta.x, 0);
        m_top.DOSizeDelta(new Vector2(m_top.sizeDelta.x, height), 1f).SetEase(Ease.Linear);
        m_bot.DOSizeDelta(new Vector2(m_bot.sizeDelta.x, height), 1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            GameController.MaskSkipCutsceneClosed();
            m_blockInput.SetActive(false);
        });
    }

    private void OnCutSceneTransition(bool isFull, float duration, bool finish)
    {
        RectTransform top = m_top;
        RectTransform bot = m_bot;
        float h = m_canvas.sizeDelta.y;
        Vector2 size = top.sizeDelta;
        float timeDelay = 0.1f;
        float time = duration / 4f;

        StopAllCoroutines();
        top.DOKill();
        bot.DOKill();
        m_blockInput.SetActive(false);
        MainController.ActiveLoading(false, 1);
        if (finish)
        {
            if (isFull)
            {
                if (m_isClose)
                {
                    StartCoroutine(IDelayCall(timeDelay, CutSceneController.NextCutSceneAction));
                    // GameController.ActivateUI(true);
                }
                else
                {
                    size.y = h / 2f;
                    top.DOSizeDelta(size, time).SetDelay(timeDelay).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        CutSceneController.NextCutSceneAction();
                        m_isClose = true;
                        // GameController.ActivateUI(true);
                    });
                    bot.DOSizeDelta(size, time).SetDelay(timeDelay);
                }
            }
            else
            {
                // GameController.ActivateUI(true);
                CutSceneController.NextCutSceneAction();
            }
                
        }
        else
        {
            if (isFull)
            {

                if (m_isClose)
                {
                    size.y = 120;
                    top.DOSizeDelta(size, time).SetDelay(timeDelay).SetEase(Ease.Linear);
                    bot.DOSizeDelta(size, time).SetDelay(timeDelay).SetEase(Ease.Linear);
                    CutSceneController.NextCutSceneAction();
                    m_isClose = false;
                }
                else
                {
                    size.y = h / 2f;
                    top.DOSizeDelta(size, time).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        size.y = 120;
                        top.DOSizeDelta(size, time).SetDelay(timeDelay).SetEase(Ease.Linear);
                        bot.DOSizeDelta(size, time).SetDelay(timeDelay).SetEase(Ease.Linear);
                        CutSceneController.NextCutSceneAction();
                    });
                    bot.DOSizeDelta(size, time);
                }
            }
            else
            {
                if (m_isClose)
                {
                    size.y = 120;
                    top.DOSizeDelta(size, time).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        CutSceneController.NextCutSceneAction();
                        m_isClose = false;
                    });
                    bot.DOSizeDelta(size, time);
                }
                else
                {
                    size.y = 120;
                    top.DOSizeDelta(size, time).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        CutSceneController.NextCutSceneAction();
                        m_isClose = false;
                    });
                    bot.DOSizeDelta(size, time);
                    // CutSceneController.NextCutSceneAction();
                }
            }
        }
    }

    private void OnCutSceneFinish()
    {
        float timeDelay = 0.1f;
        if (m_isClose)
        {
            RectTransform top = m_top;
            RectTransform bot = m_bot;
            float h = m_canvas.sizeDelta.y;
            Vector2 size = top.sizeDelta;
            size.y = 0;
            m_top.DOKill();
            m_bot.DOKill();
            top.DOSizeDelta(size, 0.5f).SetDelay(timeDelay).SetEase(Ease.Linear).OnComplete(() =>
            {
                CutSceneController.NextCutSceneAction();
                m_isClose = false;
                m_blockInput.SetActive(false);
            });
            bot.DOSizeDelta(size, 0.5f).SetEase(Ease.Linear).SetDelay(timeDelay);
        }
        else
        {
           RectTransform top = m_top;
            RectTransform bot = m_bot;
            float h = m_canvas.sizeDelta.y;
            Vector2 size = top.sizeDelta;
            size.y = 0;
            m_top.DOKill();
            m_bot.DOKill();
            top.DOSizeDelta(size, 0.5f).SetDelay(timeDelay).SetEase(Ease.Linear).OnComplete(() =>
            {
                CutSceneController.NextCutSceneAction();
                m_isClose = false;
                m_blockInput.SetActive(false);
            });
            bot.DOSizeDelta(size, 0.5f).SetEase(Ease.Linear).SetDelay(timeDelay);


            // StartCoroutine(IDelayCall(timeDelay, CutSceneController.NextCutSceneAction));
            // m_blockInput.SetActive(false);
        }
    }

    IEnumerator IDelayCall(float delayTime, Action action)
    {
        yield return new WaitForSeconds(delayTime);
        action?.Invoke();
    }

    IEnumerator IShowLoading()
    {
        yield return new WaitForSeconds(1f);
        MainController.ActiveLoading(true, 1);
    }
}
