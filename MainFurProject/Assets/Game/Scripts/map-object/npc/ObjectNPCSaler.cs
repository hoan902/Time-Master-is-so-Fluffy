using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using DG.Tweening;

public class ObjectNPCSaler : ObjectNPC
{
    [SerializeField] private GameObject m_fxExplosion;
    [SerializeField] private AnimationReferenceAsset m_animBye;

    [SerializeField] private AudioClip m_audioHello;
    [SerializeField] private AudioClip m_audioWelcome;
    [SerializeField] private AudioClip m_audioBye;

    private bool m_opened;

    public override void Awake() 
    {
        base.Awake();

        MainController.closePopupEvent += OnClosePopup;
    }
    private void OnDestroy() 
    {
        MainController.closePopupEvent -= OnClosePopup;
    }

    public override void PlayerIn()
    {
        if(isActived)
            return;
        SoundManager.PlaySound(m_audioHello, false);
        SoundManager.PlaySound(m_audioWelcome, false);
        isActived = true;
        if (funnyAnimations != null)
        {
            TrackEntry trackEntry = spine.AnimationState.SetAnimation(0, funnyAnimations[0], false);
            trackEntry.Complete += (trackEntry) => spine.AnimationState.SetAnimation(0, animIdle, true);
        }
    }

    public void OpenShop()
    {
        if (m_opened)
            return;
        m_opened = true;
        MainController.OpenPopup(PopupType.ShopIngame);
    }

    void OnClosePopup(PopupType popupType)
    {
        if(popupType == PopupType.ShopIngame)
        {
            SoundManager.PlaySound(m_audioBye, false);
            GameObject go = Instantiate(m_fxExplosion);
            go.transform.SetParent(transform.parent, false);
            go.transform.position = transform.position;
            Destroy(gameObject);
        }
    }

    IEnumerator DelayOpenStore()
    {
        yield return new WaitForSeconds(1f);
        MainController.OpenPopup(PopupType.ShopIngame);
    }
}
