using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelectionItem : MonoBehaviour
{
    [SerializeField] private Image m_skinAvatar;
    [SerializeField] private GameObject m_tick;
    [SerializeField] private GameObject m_outline;

    private string m_skin;

    private void OnDestroy()
    {
        PopupSkinComboSelection.skinOnClickEvent -= OnSkinClicked;
    }

    public void Init(string skin)
    {
        m_skin = skin;
        m_skinAvatar.sprite = ConfigLoader.instance.config.GetPlayerAvatar(m_skin);
        m_skinAvatar.SetNativeSize();

        PopupSkinComboSelection.skinOnClickEvent += OnSkinClicked;
    }

    void OnSkinClicked(string skin)
    {
        m_tick.SetActive(m_skin == MainModel.currentSkin);
        m_outline.SetActive(m_skin == skin);
    }
}
