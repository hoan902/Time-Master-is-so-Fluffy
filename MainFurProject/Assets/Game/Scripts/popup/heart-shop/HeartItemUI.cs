using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class HeartItemUI : MonoBehaviour
{
    [Header("Config")] [SerializeField] private bool m_buyWithCoin;
    [SerializeField] private int m_condition;
    [SerializeField] private int m_value;

    [Header("References (Please dont touch)")] [SerializeField]
    private Image m_iconImage;

    [SerializeField] private TextMeshProUGUI m_conditionText;
    [SerializeField] private TextMeshProUGUI m_valueText;
    [SerializeField] private TextMeshProUGUI m_soldOutText;
    [SerializeField] private Button m_getButton;
    [SerializeField] private Image m_getProcess;
    [SerializeField] private Image m_mask;
    [SerializeField] private AudioClip m_audioProcess;
    [SerializeField] private AudioClip m_audioClaim;

    private int m_adsCounter;
    private int m_index;
    private PopupShopHeart m_store;
    private bool m_soldOut;
    private GameObject m_soundProcess;
    private GameObject m_soundClaim;

    private void OnDestroy()
    {
        if (m_soundClaim != null)
            Destroy(m_soundClaim);
        if (m_soundProcess != null)
            Destroy(m_soundProcess);
    }

    // Start is called before the first frame update
    public void Init(bool soldOut, int index)
    {
        InitComponent(soldOut, index);
        UpdateInfor();
        UpdateMaskColor(m_soldOut ? 0.5f : 0, 0);

        m_getButton.gameObject.SetActive(!soldOut);
        m_soldOutText.gameObject.SetActive(soldOut);
    }

    void InitComponent(bool soldOut, int index)
    {
        m_soldOut = soldOut;
        m_index = index;
        m_store = GetComponentInParent<PopupShopHeart>();
        m_valueText.text = "x" + m_value;

        m_adsCounter = PlayerPrefController.GetAdsWatchedHeartItem(m_index);
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
        PlayerPrefController.SetAdsWatchedHeartItem(m_index, m_adsCounter);
        UpdateInfor();
        if (m_condition > 1)
        {
            if (m_soundProcess != null)
                Destroy(m_soundProcess);
            m_soundProcess = SoundManager.PlaySound(m_audioProcess, false);
            float targetFill = (float)m_adsCounter / (float)m_condition;
            if (targetFill == 1)
            {
                SaveAmount(m_value);
                m_store.UpdateSoldOut(m_index);
            }

            m_getProcess.DOKill();
            m_getProcess.DOFillAmount(targetFill, 1f).OnComplete(() =>
            {
                if (targetFill == 1)
                    Claim(false);
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
        m_getButton.gameObject.SetActive(false);
        m_soldOutText.gameObject.SetActive(true);
        UpdateMaskColor(0.5f, 1f);
        CreateCloneIcons();
    }

    void CreateCloneIcons()
    {
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
        PlayerPrefs.Save();
        MainController.UpdateHeart(value);
        // GameController.UpdateSoldOutBooster();
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
            m_conditionText.text = ""; //m_adsCounter + "/" + m_condition;
    }

    void Tracking()
    {
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
            WatchAdsComplete();
            Tracking();
        }
    }
}