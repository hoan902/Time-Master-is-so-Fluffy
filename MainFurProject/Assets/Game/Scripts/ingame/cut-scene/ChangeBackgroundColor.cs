using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ChangeBackgroundColor : MonoBehaviour
{
    [SerializeField] private Color m_targetColor = Color.white;
    [SerializeField] private float m_changeTime = 1f;
    [SerializeField] private List<SpriteRenderer> m_renderers;

    private void OnEnable() 
    {
        foreach(SpriteRenderer renderer in m_renderers)
        {
            renderer.DOColor(m_targetColor, m_changeTime);
        }
    }
}
