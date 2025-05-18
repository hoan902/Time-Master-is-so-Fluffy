using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CutsceneFastForward : MonoBehaviour
{
    [SerializeField] private float m_condition = 10f;
    [SerializeField] private Image m_process;
    [SerializeField] private Image m_arrow;

    private bool m_skiping = false;
    private bool m_skiped = false;
    private Tweener m_tweener;

    private void Awake() 
    {
        m_process.fillAmount = 0;
        m_skiping = false;
        m_skiped = false;

        m_arrow.gameObject.SetActive(false);
        m_process.gameObject.SetActive(false);
    }
    private void OnEnable() 
    {
        m_process.fillAmount = 0;
        m_skiping = false;
        m_skiped = false;
        m_arrow.gameObject.SetActive(false);
        m_process.gameObject.SetActive(false);
    }
    private void OnDisable() 
    {
        m_tweener?.Kill();
        m_skiping = false;
        m_process.fillAmount = 0;
        m_arrow.gameObject.SetActive(false);
        m_process.gameObject.SetActive(false);
    }

    private void OnMouseDown() 
    {
        if(m_skiping || m_skiped)
            return;
        m_arrow.gameObject.SetActive(true);
        m_process.gameObject.SetActive(true);
        m_tweener?.Kill();
        m_process.fillAmount = 0;
        m_skiping = true;
        m_tweener = m_process.DOFillAmount(1f, m_condition).SetEase(Ease.Linear).OnComplete(() => {
            GameController.SkipCutscene();
            m_skiped = true;
            m_skiping = false;
            m_process.fillAmount = 0;
            m_arrow.gameObject.SetActive(false);
            m_process.gameObject.SetActive(false);
        });
    }
    private void OnMouseUp()
    {
        m_arrow.gameObject.SetActive(false);
        m_process.gameObject.SetActive(false);
        m_tweener?.Kill();
        m_skiping = false;
        m_process.fillAmount = 0;
    }
}
