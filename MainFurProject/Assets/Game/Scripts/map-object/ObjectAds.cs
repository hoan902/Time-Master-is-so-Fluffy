using DG.Tweening;
using Spine;
using Spine.Unity;
using System.Collections;
using TMPro;
using UnityEngine;

public class ObjectAds : MonoBehaviour
{
    enum Type
    {
        Heart = 0,
        Coin = 1,
        Magnet = 2,
        BigMike = 3
    }

    [SerializeField] private Type m_spriteType = Type.Heart;
    [SerializeField] private int m_quantity = 1;
    [SerializeField] private bool m_isFree = false;
    [SerializeField] private SpriteRenderer m_sprite;
    [SerializeField] private TextMeshPro m_text;
    [SerializeField] private Sprite[] m_sprites;
    [SerializeField] private SkeletonAnimation m_background;
    [SerializeField] private GameObject m_textCointEffect;
    [SerializeField] private GameObject m_iconAds;
    [SerializeField] private GameObject m_explodeEff;
    [SerializeField] private AudioClip m_audioEatItem;

    private GameObject m_adsTokenIcon;
    private BoxCollider2D m_boxCollider2D;

    void Start()
    {
        if (MainModel.subscription)
            m_isFree = true;
        UpdateText();
        StartCoroutine(DelayLoadForm());
        m_adsTokenIcon = transform.Find("token").gameObject;
        if (!m_isFree)
            m_adsTokenIcon.SetActive(true);
        m_boxCollider2D = GetComponent<BoxCollider2D>();
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
                CollectReward();
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
            CollectReward();
        }
        else
        {
            switch (m_spriteType)
            {
                case Type.Heart:
                    SoundManager.PlaySound(m_audioEatItem, false);
                    PlayAnimClaim();
                    break;
                case Type.Coin:
                    SoundManager.PlaySound(m_audioEatItem, false);
                    PlayAnimClaim();
                    break;
                case Type.Magnet:
                    SoundManager.PlaySound(m_audioEatItem, false);
                    PlayAnimClaim();
                    break;
                case Type.BigMike:
                    SoundManager.PlaySound(m_audioEatItem, false);
                    PlayAnimClaim();
                    break;
            }
        }
    }

    void PlayAnimClaim()
    {
        m_background.AnimationState.SetAnimation(0, "close-wings", false);
        m_boxCollider2D.enabled = false;
    }

    void CollectReward()
    {
        if (!m_isFree)
        {
            // GameController.UpdateAdsToken(1, m_adsTokenIcon.transform.position);
            m_adsTokenIcon.SetActive(false);
        }

        switch (m_spriteType)
        {
            case Type.Heart:
                GameController.UpdateHeart(m_quantity, transform.position);
                Destroy(gameObject);
                break;
            case Type.Coin:
                GameController.UpdateCoin(m_quantity);
                ShowCoinEffect();
                GameController.GetCoinAdsIngame(m_quantity, transform.position);
                Destroy(gameObject);
                break;
            case Type.Magnet:
                GameController.ActiveMagnet();
                Destroy(gameObject);
                break;
            case Type.BigMike:
                DropItemBigMike();
                break;
        }
    }

    void DropItemBigMike()
    {
        GetComponent<ItemDropper>().Drop(transform.position);
        Destroy(gameObject);
    }

    public void UpdateSprite()
    {
        m_sprite.sprite = m_sprites[(int)m_spriteType];
        UpdateText();
    }

    public void UpdateText()
    {
        switch (m_spriteType)
        {
            case Type.Heart:
                m_text.text = m_quantity < 1 ? "" : ("+" + m_quantity);
                break;
            case Type.Coin:
                m_text.text = m_quantity < 1 ? "" : ("+" + m_quantity * MapConstant.COIN_RATIO);
                break;
            case Type.Magnet:
                m_text.text = m_quantity <= 1 ? "" : ("+" + m_quantity);
                break;
            case Type.BigMike:
                m_text.text = m_quantity <= 1 ? "" : ("+" + m_quantity);
                break;
        }
    }

    public void UpdateForm()
    {
        m_background.AnimationState.SetAnimation(0, m_isFree ? "Free-form" : "Try-form", true);
        m_iconAds.SetActive(!m_isFree);
    }

    IEnumerator DelayLoadForm()
    {
        yield return null;
        yield return null;
        yield return null;
        UpdateForm();
    }

    void ShowCoinEffect()
    {
        TextMeshPro text = m_textCointEffect.GetComponent<TextMeshPro>();
        text.text = "+" + m_quantity * MapConstant.COIN_RATIO;
        m_textCointEffect.SetActive(true);
        m_textCointEffect.transform.SetParent(transform.parent, true);
        m_textCointEffect.transform.localEulerAngles = Vector3.zero;
        m_textCointEffect.transform.DOMoveY(transform.position.y + 1.5f, 0.5f).OnComplete(() =>
        {
            text.DOFade(0, 0.5f).OnComplete(() =>
            {
                m_textCointEffect.transform.DOKill();
                text.DOKill();
                Destroy(m_textCointEffect);
            });
        });
    }
}