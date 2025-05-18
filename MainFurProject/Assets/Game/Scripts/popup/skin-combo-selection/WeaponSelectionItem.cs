using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelectionItem : MonoBehaviour
{
    [SerializeField] private Image m_weaponAvatar;
    [SerializeField] private GameObject m_tick;
    [SerializeField] private GameObject m_outline;

    private string m_weaponSkinName;
    private WeaponConfig m_weaponConfig;

    private void OnDestroy()
    {
        PopupSkinComboSelection.weaponOnClickEvent -= OnWeaponClicked;
    }

    public void Init(string skin)
    {
        m_weaponSkinName = skin;
        m_weaponConfig = ConfigLoader.instance.config.GetWeapon(m_weaponSkinName);
        m_weaponAvatar.sprite = m_weaponConfig.avatar;
        m_weaponAvatar.SetNativeSize();

        PopupSkinComboSelection.weaponOnClickEvent += OnWeaponClicked;
    }

    void OnWeaponClicked(string weaponSkinName)
    {
        m_tick.SetActive(m_weaponSkinName == MainModel.currentWeapon);
        m_outline.SetActive(m_weaponSkinName == weaponSkinName);
    }
}
