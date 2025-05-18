using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine;
using Spine.Unity;

public class ShopIngameItem : MonoBehaviour
{
    [Header("Config")] [SerializeField] private IngameShopItemType m_boosterType;
    [SerializeField] private bool m_buyWithCoin;
    [SerializeField] private int m_condition;
    [SerializeField] private int m_value;

    [Header("References (Please dont touch)")] [SerializeField]
    private Image m_iconImage;

    [SerializeField] private TextMeshProUGUI m_conditionText;
    [SerializeField] private TextMeshProUGUI m_valueText;
    [SerializeField] private Material m_grayMaterial;
    [SerializeField] private Button m_getButton;
    [SerializeField] private Image m_getProcess;
    [SerializeField] private Image m_mask;
    [SerializeField] private Sprite[] m_iconSprites;
    [SerializeField] private WeaponAndIcon[] m_weaponAndIcons;
    [SerializeField] private AudioClip m_audioProcess;
    [SerializeField] private AudioClip m_audioClaim;

    private int m_adsCounter;
    private int m_index;
    private PopupShopIngame m_store;
    private bool m_soldOut;
    private GameObject m_soundProcess;
    private GameObject m_soundClaim;
    private PopupShopIngame m_shopIngame;

    [System.Serializable]
    public class WeaponAndIcon
    {
        public string weaponSkinName;
        public Sprite icon;
    }

    private void OnDestroy()
    {
        if (m_soundClaim != null)
            Destroy(m_soundClaim);
        if (m_soundProcess != null)
            Destroy(m_soundProcess);
    }

    public void Init(int index, PopupShopIngame shopIngame)
    {
        m_shopIngame = shopIngame;
        m_soldOut = false;
        if (m_buyWithCoin && MainModel.totalCoin < m_condition)
            m_soldOut = true;
        InitComponent(m_soldOut, index);
        UpdateInfor();
        UpdateMaskColor(m_soldOut ? 0.5f : 0, 0);

        m_getButton.gameObject.SetActive(true);
        m_getButton.interactable = !m_soldOut;
        m_getButton.GetComponent<Image>().material = m_soldOut ? m_grayMaterial : null;
    }

    void InitComponent(bool soldOut, int index)
    {
        m_soldOut = soldOut;
        m_index = index;
        m_store = GetComponentInParent<PopupShopIngame>();
        m_valueText.text = m_boosterType == IngameShopItemType.Weapon ? "" : "x" + m_value;

        if ((int)m_boosterType == 2)
            m_iconImage.sprite = GetWeaponIcon();
        else
            m_iconImage.sprite = m_iconSprites[(int)m_boosterType];
        m_iconImage.SetNativeSize();

        m_adsCounter = 0;
        m_getProcess.fillAmount = (float)m_adsCounter / (float)m_condition;
    }

    void UpdateMaskColor(float transparent, float fadeTime)
    {
        Color color = Color.black;
        color.a = transparent;
        if (fadeTime == 0)
        {
            m_mask.color = color;
            return;
        }

        m_mask.DOFade(transparent, fadeTime);
    }

    void WatchAdsComplete()
    {
        m_adsCounter++;
        //PlayerPrefController.SetAdsWatchedHeartItem(m_index, m_adsCounter);
        UpdateInfor();
        if (m_condition > 1)
        {
            if (m_soundProcess != null)
                Destroy(m_soundProcess);
            m_soundProcess = SoundManager.PlaySound(m_audioProcess, false);
            float targetFill = (float)m_adsCounter / (float)m_condition;
            if (targetFill == 1)
            {
                m_store.UpdateSoldOut(m_index);
            }

            m_getProcess.DOKill();
            m_getProcess.DOFillAmount(targetFill, 1f).OnComplete(() =>
            {
                if (targetFill == 1)
                    Claim((int)m_boosterType == 2);
                else
                    m_getButton.interactable = true;
            });
        }
        else if (m_adsCounter >= m_condition)
        {
            Claim();
            m_store.UpdateSoldOut(m_index);
        }
    }

    void BuyWithCoin()
    {
        if (MainModel.totalCoin < m_condition)
        {
            MainController.OpenPopup(PopupType.WatchAds);
            return;
        }

        MainController.UpdateCoin(-m_condition);
        m_store.UpdateSoldOut(m_index);
        UpdateInfor();
        SaveAmount(m_value);
        m_getProcess.DOKill();
        m_getProcess.DOFillAmount(1f, 1f).OnComplete(() => { Claim(false); });
    }

    void Claim(bool needToAddAmount = true)
    {
        if (m_soundClaim != null)
            Destroy(m_soundClaim);
        m_soundClaim = SoundManager.PlaySound(m_audioClaim, false);
        SaveAmount(needToAddAmount ? m_value : 0);
        m_getButton.interactable = false;
        m_getButton.GetComponent<Image>().material = m_grayMaterial;
        UpdateMaskColor(0.5f, 1f);
        CreateCloneIcons();
    }

    void CreateCloneIcons()
    {
        if (m_boosterType == IngameShopItemType.Weapon)
            return;
        float randXPos;
        float randYPos;
        float randMoveTime;
        for (int i = 0; i < m_value; i++)
        {
            randXPos = Random.Range(-200, 200);
            randYPos = Random.Range(-200, 200);
            randMoveTime = Random.Range(0.5f, 1.5f);
            GameObject go = Instantiate(m_iconImage.gameObject, transform);
            RectTransform goRT = go.GetComponent<RectTransform>();
            goRT.localScale = Vector3.one;
            goRT.anchoredPosition = Vector2.zero;
            go.GetComponent<Image>().color = Color.white;
            goRT.DOAnchorPos(new Vector2(randXPos, randYPos), randMoveTime).OnComplete(() =>
            {
                go.transform.SetParent(m_store.transform);
                goRT.transform.DOMove(m_store.GetEffDestination(), 1f).OnComplete(() =>
                {
                    goRT.DOScale(new Vector3(1.5f, 1.5f, 1), 0.5f);
                    go.GetComponent<Image>().DOFade(0, 0.5f).OnComplete(() => Destroy(go));
                });
            });
        }
    }

    void SaveAmount(int value)
    {
        if (m_boosterType == IngameShopItemType.Heart)
            GameController.UpdateHeart(value, null);
        else if (m_boosterType == IngameShopItemType.Heal)
            STGameController.UpdatePlayerHp(100);
        else
            GameController.CollectWeapon("w" + value);
    }

    void UpdateInfor()
    {
        if (m_condition <= 1)
        {
            m_conditionText.text = "";
            return;
        }

        if (m_buyWithCoin)
            m_conditionText.text = m_condition.ToString();
        else
            m_conditionText.text = "";
    }

    Sprite GetWeaponIcon()
    {
        foreach (WeaponAndIcon weaponAndIcon in m_weaponAndIcons)
        {
            if (("w" + m_value) == weaponAndIcon.weaponSkinName)
                return weaponAndIcon.icon;
        }

        return null;
    }

    public void GetOnClick()
    {
        if (m_adsCounter >= m_condition)
            return;
        // m_getButton.interactable = false;
        if (Application.isEditor)
        {
            if (m_buyWithCoin)
                BuyWithCoin();
            else
                WatchAdsComplete();
            return;
        }

        if (m_buyWithCoin)
            BuyWithCoin();
        else
        {
            m_shopIngame.WatchedAds = true;
            WatchAdsComplete();
        }
    }
}

public enum IngameShopItemType
{
    Heal = 0,
    Heart = 1,
    Weapon = 2
}