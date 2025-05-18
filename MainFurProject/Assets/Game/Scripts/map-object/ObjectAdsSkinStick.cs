using Spine.Unity;
using Spine;
using UnityEngine;

public class ObjectAdsSkinStick : MonoBehaviour
{
    [Range(1, 4)] [SerializeField] private int m_skinID = 1;
    [SerializeField] private bool m_isFree = false;
    [SerializeField] private SkeletonAnimation m_spine;
    [SerializeField] private SkeletonAnimation m_background;
    [SerializeField] private GameObject m_iconAds;
    [SerializeField] private GameObject m_explodeEff;
    [SerializeField] private AudioClip m_audioEatItem;

    private GameObject m_adsTokenIcon;
    private BoxCollider2D m_boxCollider2D;

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
        m_background.AnimationState.Complete += OnAnimComplete;

        m_spine.SetSkin(m_skinID.ToString());
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
                TrySkin();
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
            TrySkin();
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

    void TrySkin()
    {
        if (!m_isFree)
        {
            m_adsTokenIcon.SetActive(false);
        }

        MainController.TrySkin(m_skinID.ToString());
        Destroy(gameObject);
    }

    public void UpdateSkin()
    {
        m_spine.SetSkin(m_skinID.ToString());
    }

    public void UpdateForm()
    {
        m_background.AnimationState.SetAnimation(0, m_isFree ? "Free-form" : "Try-form", true);
        m_iconAds.SetActive(!m_isFree);
    }
}