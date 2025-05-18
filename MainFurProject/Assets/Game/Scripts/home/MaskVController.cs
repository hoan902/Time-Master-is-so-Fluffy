using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MaskVController : MonoBehaviour
{
    [SerializeField] private SceneTransition m_sceneTransition;
    [SerializeField] private Animator m_animator;
    [SerializeField] private Image m_mask;
    [SerializeField] private Image m_bg;
    [SerializeField] private Image m_vImage;
    [SerializeField] private float m_resumeMusicDuration = 1.5f;
    [SerializeField] private AudioClip m_audioIn;
    [SerializeField] private AudioClip m_audioOut;

    private Tween m_adjustMusicVolumeTween;
    private GameObject m_currentSound;

    void ShowHideChild(bool toShow, bool toShowVFake)
    {
        Color vColor = m_mask.color;
        Color bgColor = m_bg.color;
        Color vImageColor = m_vImage.color;

        vColor.a = toShow ? 1 : 0;
        bgColor.a = toShow ? 1 : 0;
        vImageColor.a = toShowVFake ? 1 : 0;

        m_mask.color = vColor;
        m_bg.color = bgColor;
        m_vImage.color = vImageColor;
    }

    public void TriggerMask(string key)
    {
        m_animator.StopPlayback();
        bool isIn = key == "in" ? false : true;
        ShowHideChild(true, isIn);
        m_animator.SetTrigger(key);

        float currentMusicVolume = SoundManager.GetCurrentMusicVolume();
        m_adjustMusicVolumeTween?.Kill();
        m_adjustMusicVolumeTween = DOTween.To(() => currentMusicVolume, x => currentMusicVolume = x, 0, 0.5f).OnUpdate(() => {
            SoundManager.AdjustVolumeMusic(currentMusicVolume);
        });

        if(m_currentSound != null)
            Destroy(m_currentSound);
        m_currentSound = SoundManager.PlaySound(isIn ? m_audioIn : m_audioOut, false);
    }

    IEnumerator DelayPlayAnim(string key)
    {
        yield return null;
        m_animator.SetTrigger(key);
    }

    public void MaskCloseComplete()
    {
        m_sceneTransition.MaskVCloseComplete();
        ShowHideChild(true, true);
        // SoundManager.AdjustVolumeMusic(1);
    }
    public void MaskOpenComplete()
    {
        m_sceneTransition.MaskVOpenComplete();
        ShowHideChild(false, false);

        float currentMusicVolume = SoundManager.GetCurrentMusicVolume();
        m_adjustMusicVolumeTween?.Kill();
        m_adjustMusicVolumeTween = DOTween.To(() => currentMusicVolume, x => currentMusicVolume = x, 1, m_resumeMusicDuration).OnUpdate(() => {
            SoundManager.AdjustVolumeMusic(currentMusicVolume);
        });
    }

}
