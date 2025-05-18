using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class PopupDailyReward : MonoBehaviour
{
    private const string KEY_COUNT = "daily-rewards-count";
    private const string KEY_TIME = "daily-rewards-time";

    [SerializeField] private GameObject[] m_days;
    [SerializeField] private Button m_buttonClaim;
    [SerializeField] private Button m_buttonClaimX2;
    [SerializeField] private Button m_buttonClaimSkin;
    [SerializeField] private Button m_buttonNothanks;

    private int m_count;
    private bool m_claimed;
    private RectTransform m_panel;

    void Start()
    {
        m_panel = transform.Find("panel").GetComponent<RectTransform>();
        int targetPanelYPos = 100;
#if UNITY_IOS
        if(!MainModel.isTablet)
            targetPanelYPos = 150;
#endif
        m_panel.anchoredPosition = new Vector2(0, targetPanelYPos);

        MainController.claimX2DailyRewardEvent += OnClaimX2;
        InitRewards();
    }

    void OnDestroy()
    {
        MainController.claimX2DailyRewardEvent -= OnClaimX2;
        StopAllCoroutines();
    }

    private void OnClaimX2()
    {
        ClaimReward(1);
        InitRewards();
    }

    public void CloseOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.ClosePopup(PopupType.DailyReward);
    }

    public void ClaimOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        m_buttonClaimX2.gameObject.SetActive((m_count != 1 && m_count != 4));
        if (m_buttonClaimX2.gameObject.activeSelf)
            StartCoroutine(DelayClaim());
        else
            MainController.ClosePopup(PopupType.DailyReward);
        ClaimReward(1);
        InitRewards();
        m_claimed = true;
    }

    public void ClaimX2Onclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        if (Application.isEditor)
        {
            MainController.ClaimX2DailyReward();
            return;
        }

        MainController.ClaimX2DailyReward();
    }

    void ClaimReward(int ratio)
    {
        switch (m_count)
        {
            case 0:
                MainController.BonusCoin(1000 * ratio);
                break;
            case 1:
                // MainController.BuyCoinSkin(0, GameConstant.DAILY_SKIN_2);
                MainController.BonusHeart(2 * ratio);
                break;
            case 2:
                MainController.BonusHeart(5 * ratio);
                break;
            case 3:
                MainController.BonusCoin(5000 * ratio);
                break;
            case 4:
                // MainController.BuyCoinSkin(0, GameConstant.DAILY_SKIN_5);
                MainController.BonusHeart(5 * ratio);
                break;
            case 5:
                MainController.BonusHeart(10 * ratio);
                break;
            case 6:
                MainController.BonusCoin(10000 * ratio);
                CoroutineHelper.NewCoroutine(DelayClaimDay7(ratio));
                break;
        }

        //
        ClaimDailyReward(m_count);
        if (m_claimed)
            MainController.ClosePopup(PopupType.DailyReward);
    }

    IEnumerator DelayClaimDay7(int ratio)
    {
        yield return new WaitForSeconds(1.5f);
        MainController.BonusHeart(10 * ratio);
    }

    void InitRewards()
    {
        //
        // Image skin2 = m_days[1].transform.Find("icon").GetComponent<Image>();
        // skin2.sprite = ConfigLoader.instance.config.GetPlayerAvatar(GameConstant.DAILY_SKIN_2, PlayMode.Normal);
        // skin2.SetNativeSize();
        //
        // Image skin5 = m_days[4].transform.Find("icon").GetComponent<Image>();
        // skin5.sprite = ConfigLoader.instance.config.GetPlayerAvatar(GameConstant.DAILY_SKIN_5, PlayMode.Normal);
        // skin5.SetNativeSize();
        //
        bool active = hasDailyReward;
        m_count = PlayerPrefs.GetInt(DataKey.DAILY_REWARD_COUNT, -1);
        m_count += active ? 1 : 0;
        for (int i = 0; i < m_days.Length; i++)
        {
            ActiveDayMask(m_days[i], i < m_count);
            ActiveDayLock(m_days[i], i > m_count);
            if (i == m_count)
            {
                ActiveDayCheck(m_days[i], !active);
            }
            else
                ActiveDayCheck(m_days[i], i < m_count);
        }

        //
        if (m_count > 6)
            active = false;
        m_buttonClaimSkin.gameObject.SetActive((m_count == 1 || m_count == 4) && active);
        m_buttonClaim.gameObject.SetActive(m_count != 1 && m_count != 4 && active);
        m_claimed = !active;
    }

    IEnumerator DelayClaim()
    {
        yield return new WaitForSeconds(2f);
        // m_buttonClaim.gameObject.SetActive(true);
        m_buttonNothanks.gameObject.SetActive(true);
    }

    void ActiveDayMask(GameObject go, bool active)
    {
        GameObject glow = go.transform.Find("mask").gameObject;
        if (glow == null)
            return;
        glow.SetActive(active);
    }

    void ActiveDayCheck(GameObject go, bool active)
    {
        GameObject check = go.transform.Find("check").gameObject;
        if (check == null)
            return;
        check.SetActive(active);
    }

    void ActiveDayLock(GameObject go, bool active)
    {
        GameObject l = go.transform.Find("lock").gameObject;
        if (l == null)
            return;
        l.SetActive(active);
    }

    void ClaimDailyReward(int dayCount)
    {
        DateTime now = DateTime.Now;
        PlayerPrefs.SetString(DataKey.DAILY_REWARD_TIME, now.ToString("MM/dd/yyyy"));
        PlayerPrefs.SetInt(DataKey.DAILY_REWARD_COUNT, dayCount);
        PlayerPrefs.Save();
    }

    public static bool hasDailyReward
    {
        get
        {
            int count = PlayerPrefs.GetInt(DataKey.DAILY_REWARD_COUNT, -1);
            if (count > 5)
                return false;
            string lastTimeStr = PlayerPrefs.GetString(DataKey.DAILY_REWARD_TIME, "");
            if (lastTimeStr == "")
                return true;
            DateTime lastTime;
            bool result = DateTime.TryParseExact(lastTimeStr, "MM/dd/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out lastTime);
            if (!result)
            {
                lastTime = DateTime.Now;
                PlayerPrefs.DeleteKey(DataKey.DAILY_REWARD_TIME);
                PlayerPrefs.Save();
            }

            DateTime now = DateTime.Now;
            return now.Year != lastTime.Year ||
                   now.DayOfYear !=
                   lastTime.DayOfYear; //LevelLoader.instance.fakeMapLevel > RemoteConfig.levelShowDailyReward
        }
    }
}