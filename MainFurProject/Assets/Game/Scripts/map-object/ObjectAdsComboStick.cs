using System;
using System.Collections;
using Spine;
using Spine.Unity;
using Unity.Mathematics;
using UnityEngine;

public class ObjectAdsComboStick : MonoBehaviour
{
    // config
    [Range(5, 9)] [SerializeField] private int m_skinID;
    [SerializeField] private WeaponName m_weapon;
    [SerializeField] private bool m_isFree;
    [SerializeField] private WeaponAndAnim[] m_weaponAndAnims;
    [SerializeField] private bool m_canKeepSkin;

    // references
    [SerializeField] private SkeletonAnimation m_spine;
    [SerializeField] private SkeletonAnimation m_background;
    [SerializeField] private GameObject m_iconAds;
    [SerializeField] private GameObject m_explodeEff;
    [SerializeField] private AudioClip m_audioEatItem;

    private BoxCollider2D m_boxCollider2D;
    private AnimationReferenceAsset m_animation;

    [Serializable]
    private class WeaponAndAnim
    {
        public WeaponName weapon;
        public string weaponSkin;
        public AnimationReferenceAsset animation;
    }

    private void Start()
    {
        if (m_skinID < 5)
        {
            m_skinID = UnityEngine.Random.Range(5, 9);
            m_weapon = (WeaponName)UnityEngine.Random.Range(6, 11);
        }

        if (m_canKeepSkin)
            m_canKeepSkin = false;
        if (MainModel.subscription)
            m_isFree = true;
        UpdateSkin();
        UpdateForm();
        StartCoroutine(ScheduleAnimation());

        m_boxCollider2D = GetComponent<BoxCollider2D>();

        m_background.AnimationState.Complete += OnAnimComplete;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        m_background.AnimationState.Complete -= OnAnimComplete;
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        string animName = trackEntry.Animation.Name;
        switch (animName)
        {
            case "close-wings":
                Instantiate(m_explodeEff, transform.position, Quaternion.identity, transform.parent);
                TryCombo();
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
            TryCombo();
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

    void TryCombo()
    {
        MainController.TrySkin(m_skinID.ToString());
        MainController.TryWeapon(m_weapon);
        GameController.ShowCollectWeaponEffect(MainModel.CurrentSkin, MainModel.CurrentWeapon);
        if (m_canKeepSkin)
        {
            GameController.KeepSkin(m_skinID.ToString());
            GameController.KeepWeapon(ConfigLoader.instance.config.GetWeapon(m_weapon).weapon.skin);
            MainModel.ResetAdsWatchedToCollectCombo();
        }

        Destroy(gameObject);
    }

    IEnumerator ScheduleAnimation()
    {
        TrackEntry trackEntry;

        m_spine.AnimationState.SetAnimation(0, "idle", true);
        while (true)
        {
            yield return new WaitForSeconds(3f);
            trackEntry = m_spine.AnimationState.SetAnimation(0, m_animation, false);
            trackEntry.Complete += (trackEntry) => { m_spine.AnimationState.SetAnimation(0, "idle", true); };
        }
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

        m_spine.initialSkinName = m_skinID.ToString();
        m_spine.SetMixSkin(m_skinID.ToString(), wSkin);
    }

    public void UpdateForm()
    {
        m_background.AnimationState.SetAnimation(0, m_isFree ? "Free-form" : "Try-form", true);
        m_iconAds.SetActive(!m_isFree);
    }
}