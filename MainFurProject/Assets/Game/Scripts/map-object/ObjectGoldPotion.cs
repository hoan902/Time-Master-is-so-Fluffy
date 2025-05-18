using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;

public class ObjectGoldPotion : Dropable
{
    [SerializeField] float m_showTime = 3f;
    [SerializeField] float m_radius;
    [SerializeField] private bool m_isFree = false;
    [SerializeField] private GameObject m_iconAds;
    [SerializeField] private SkeletonAnimation m_background;
    [SerializeField] private GameObject m_explodeEff;

    [Header("Please Dont Touch")] [SerializeField]
    CircleCollider2D m_effColiider;

    [SerializeField] ParticleSystem m_claimEff;
    [SerializeField] ParticleSystem m_spreadEff;
    [SerializeField] ParticleSystem m_coreSpreadEff;
    [SerializeField] ParticleSystem m_core2SpreadEff;
    [Header("Audio")] [SerializeField] AudioClip m_audioCollect;
    [SerializeField] AudioClip m_audioSpread;
    [SerializeField] private AudioClip m_audioEatItem;

    private Tween m_spreadTween;
    private bool m_onGround;
    private GameObject m_collectSound;
    private GameObject m_spreadSound;
    private GameObject m_adsTokenIcon;

    private void Awake()
    {
        m_effColiider.gameObject.SetActive(false);
        m_onGround = false;
    }

    void Start()
    {
        hasAds = false;

        var coreMain = m_coreSpreadEff.main;
        var core2Main = m_core2SpreadEff.main;
        coreMain.startLifetime = m_showTime;
        coreMain.startSize = m_radius * 2;
        core2Main.startLifetime = m_showTime;
        core2Main.startSize = m_radius * 2;

        m_adsTokenIcon = transform.Find("token").gameObject;
        if (!m_isFree)
            m_adsTokenIcon.SetActive(true);

        m_background.AnimationState.Complete += OnAnimComplete;
    }

    private void OnDestroy()
    {
        m_background.AnimationState.Complete -= OnAnimComplete;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag != GameTag.PLAYER)
            return;
        if (Application.isEditor || m_isFree)
        {
            SoundManager.PlaySound(m_audioEatItem, false);
            GetComponent<BoxCollider2D>().enabled = false;
            if (m_explodeEff)
                Instantiate(m_explodeEff, transform.position, Quaternion.identity, transform.parent);
            Broken();
        }
        else
        {
            SoundManager.PlaySound(m_audioEatItem, false);
            PlayAnimClaim();
        }
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        string animName = trackEntry.Animation.Name;
        switch (animName)
        {
            case "close-wings":
                Instantiate(m_explodeEff, transform.position, Quaternion.identity, transform.parent);
                Broken();
                break;
        }
    }

    void PlayAnimClaim()
    {
        m_background.AnimationState.SetAnimation(0, "close-wings", false);
        GetComponent<BoxCollider2D>().enabled = false;
    }

    public void Broken()
    {
        if (!m_isFree)
        {
            // GameController.UpdateAdsToken(1, m_adsTokenIcon.transform.position);
            m_adsTokenIcon.SetActive(false);
        }

        GameController.EatItem();
        m_background.gameObject.SetActive(false);
        m_iconAds.gameObject.SetActive(false);
        m_claimEff.Play();
        m_spreadEff.Play();
        m_collectSound = SoundManager.PlaySound3D(m_audioCollect, 5, false, transform.position);
        m_spreadSound = SoundManager.PlaySound3D(m_audioSpread, 5, false, transform.position);

        m_effColiider.radius = 0;
        m_effColiider.gameObject.SetActive(true);
        float tempRadius = 0;
        m_spreadTween = DOTween.To(() => tempRadius, x => tempRadius = x, m_radius, m_showTime).SetAutoKill(false)
            .SetEase(Ease.Linear).OnUpdate(() => { m_effColiider.radius = tempRadius; }).OnComplete(() =>
            {
                if (m_collectSound != null)
                    Destroy(m_collectSound);
                if (m_spreadSound != null)
                    Destroy(m_spreadSound);
                Destroy(gameObject);
            });
    }

    public void UpdateForm()
    {
        m_background.AnimationState.SetAnimation(0, m_isFree ? "Free-form" : "Try-form", true);
        m_iconAds.SetActive(!m_isFree);
    }

    public void UpdateRadius()
    {
        m_effColiider.radius = m_radius;
    }
}