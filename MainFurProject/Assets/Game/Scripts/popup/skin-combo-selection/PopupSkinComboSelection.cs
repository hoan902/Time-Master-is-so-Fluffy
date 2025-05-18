using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Spine.Unity;
using Spine;

public class PopupSkinComboSelection : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic m_character;
    [SerializeField] private PanelSkinSelection m_panelSkinSelection;
    [SerializeField] private PanelWeaponSelection m_panelWeaponSelection;

    public static Action<string> skinOnClickEvent;
    public static Action<string> weaponOnClickEvent;

    private string m_skin = MainModel.currentSkin;
    private string m_weapon = MainModel.currentWeapon;

    private void Start()
    {
        skinOnClickEvent += OnSkinClicked;
        weaponOnClickEvent += OnWeaponClicked;

        m_panelSkinSelection.Init();
        m_panelWeaponSelection.Init();
    }
    private void OnDestroy()
    {
        skinOnClickEvent -= OnSkinClicked;
        weaponOnClickEvent -= OnWeaponClicked;
    }

    void OnSkinClicked(string skin)
    {
        m_skin = skin;

        m_character.SetMixSkin(m_skin, m_weapon);
    }
    void OnWeaponClicked(string weapon)
    {
        m_weapon = weapon;

        m_character.SetMixSkin(m_skin, m_weapon);
    }

    public void SelectSkin()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.SelectSkin(m_skin);
        skinOnClickEvent?.Invoke(m_skin);
    }
    public void SelectWeapon()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.SelectWeapon(m_weapon);
        weaponOnClickEvent?.Invoke(m_weapon);
    }
    public void BackOnClick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.ClosePopup(PopupType.SkinSelection);
    }
}
