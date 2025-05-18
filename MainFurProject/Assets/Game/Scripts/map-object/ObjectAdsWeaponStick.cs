using Spine.Unity;
using Spine;
using UnityEngine;
using System;
using System.Collections;

public class ObjectAdsWeaponStick : MonoBehaviour
{
    [SerializeField] private WeaponName m_weapon;
    [SerializeField] private bool m_isFree = false;
    [SerializeField] private SkeletonAnimation m_spine;
    [SerializeField] private SkeletonAnimation m_background;
    [SerializeField] private GameObject m_iconAds;
    [SerializeField] private GameObject m_explodeEff;
    [SerializeField] private AudioClip m_audioEatItem;
    [SerializeField] private WeaponAndAnim[] m_weaponAndAnims;

    private GameObject m_adsTokenIcon;
    private BoxCollider2D m_boxCollider2D;
    private AnimationReferenceAsset m_animation;

    [Serializable]
    private class WeaponAndAnim
    {
        public WeaponName weapon;
        public string weaponSkin;
        public AnimationReferenceAsset animation;
    }

    void Start()
    {
        if (MainModel.subscription)
            m_isFree = true;
        UpdateSkin();
        UpdateForm();
        m_adsTokenIcon = transform.Find("token").gameObject;
        if (!m_isFree)
            m_adsTokenIcon.SetActive(true);
        m_boxCollider2D = GetComponent<BoxCollider2D>();
        // StartCoroutine(ScheduleAnimation());
        m_spine.AnimationState.SetAnimation(0, m_animation, true);

        m_background.AnimationState.Complete += OnAnimComplete;
    }

    private void OnDestroy()
    {
        m_background.AnimationState.Complete -= OnAnimComplete;
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        string animName = trackEntry.Animation.Name;
        switch (animName)
        {
            case "close-wings":
                Instantiate(m_explodeEff, transform.position, Quaternion.identity, transform.parent);
                TryWeapon();
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag != GameTag.PLAYER)
            return;
        if (collider.offset.y < 0.5f)
            return;
        if (Application.isEditor || m_isFree)
        {
            SoundManager.PlaySound(m_audioEatItem, false);
            m_boxCollider2D.enabled = false;
            Instantiate(m_explodeEff, transform.position, Quaternion.identity, transform.parent);
            TryWeapon();
        }
        else
        {
            SoundManager.PlaySound(m_audioEatItem, false);
            PlayAnimClaim();
            GameController.ResumePlayer();
        }
    }

    void PlayAnimClaim()
    {
        m_background.AnimationState.SetAnimation(0, "close-wings", false);
        m_boxCollider2D.enabled = false;
    }

    void TryWeapon()
    {
        if (!m_isFree)
        {
            m_adsTokenIcon.SetActive(false);
        }

        MainController.TryWeapon(m_weapon);
        GameController.ShowCollectWeaponEffect(MainModel.CurrentSkin, MainModel.CurrentWeapon);
        Destroy(gameObject);
    }

    public void UpdateSkin()
    {
        string wSkin = m_weaponAndAnims[0].weaponSkin;
        m_animation = m_weaponAndAnims[0].animation;
        foreach (WeaponAndAnim waa in m_weaponAndAnims)
        {
            if (waa.weapon == m_weapon)
            {
                wSkin = waa.weaponSkin;
                m_animation = waa.animation;
            }
        }

        m_spine.SetSkin(wSkin);
    }

    public void UpdateForm()
    {
        m_background.AnimationState.SetAnimation(0, m_isFree ? "Free-form" : "Try-form", true);
        m_iconAds.SetActive(!m_isFree);
    }
}