using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class PopupCollectSkillEffect : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic m_spine;
    [SerializeField] private GameObject m_blockInput;
    [SerializeField] private AudioClip m_audio;

    private bool m_showing;

    private void Awake()
    {
        GameController.showCollectWeaponEffectEvent += OnShow;
    }
    private void OnDestroy()
    {
        GameController.showCollectWeaponEffectEvent -= OnShow;
    }

    void OnShow(string skin, string weapon)
    {
        if (m_showing)
            return;
        m_showing = true;
        m_blockInput.SetActive(true);
        m_spine.gameObject.SetActive(true);
        m_spine.AnimationState.SetEmptyAnimation(0, 0);
        m_spine.SetMixSkin(skin, weapon);
        SoundManager.PlaySound(m_audio, false);
        TrackEntry entry = m_spine.AnimationState.SetAnimation(0, "blue", false);
        entry.Complete += (entry) => ShowComlete();
    }

    void ShowComlete()
    {
        m_showing = false;
        m_blockInput.SetActive(false);
        m_spine.gameObject.SetActive(false);
    }
}
