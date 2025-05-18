using UnityEngine;
using Spine.Unity;
using UnityEngine.UI;
using System.Collections;
using System;
using Spine;
using DG.Tweening;
using TMPro;
using System.Text;

public class PopupUnlockSkin : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic m_character;
    [SerializeField] private Button m_buttonBack;
    [SerializeField] private Button m_buttonNoThanks;
    [SerializeField] private Button m_buttonBuyWithAds;
    [SerializeField] private Button m_buttonBuyWithCoin;
    [SerializeField] private WeaponAndAnim[] m_weaponAndAnims;
    [SerializeField] private Image m_adsProcess;
    [SerializeField] private TextMeshProUGUI m_adsProcessText;
    [SerializeField] private ParticleSystem m_effectClaim;
    [SerializeField] private GameObject m_countdownObj;
    [SerializeField] private TextMeshProUGUI m_timerText;
    [SerializeField] private Image m_bg;
    [SerializeField] private Image m_iconS;
    [SerializeField] private TextMeshProUGUI m_heroNameText;
    [SerializeField] private Transform m_fire;
    [SerializeField] private Transform m_platform;
    [SerializeField] private AudioClip m_audioProcess;
    [SerializeField] private AudioClip m_audioClaim;
    [SerializeField] private AudioClip m_audioHeroName;
    [SerializeField] private AudioClip m_audioTier;
    [SerializeField] private AudioClip m_audioHeroJump;
    [SerializeField] private AudioClip m_audioFireVFX;

    private string m_skin;
    private string m_weapon;
    private RectTransform m_panel;
    private int m_cost = 20000;
    private AnimationReferenceAsset m_animation;
    private int m_adsCounter;
    private Tweener m_doProcessTweener;
    private string m_heroName;
    private int m_adsCondition;

    [Serializable]
    private class WeaponAndAnim
    {
        public WeaponName weapon;
        public string weaponSkin;
        public AnimationReferenceAsset animation;
        public string heroName;
    }

    private void Awake()
    {
        m_panel = transform.Find("panel").GetComponent<RectTransform>();
        int targetPanelYPos = 0;
#if UNITY_IOS
        if(!MainModel.isTablet)
            targetPanelYPos = 100;
#endif
        m_panel.anchoredPosition = new Vector2(0, targetPanelYPos);
        //m_adsCondition = RemoteConfig.skinComboAdsCondition;
        m_adsCondition = 0;
    }

    IEnumerator Start()
    {
        m_skin = MainModel.availableSkinToUnlock;
        m_weapon = MainModel.availableWeaponToUnlock;
        UpdateSkin();
        m_adsCounter = PlayerPrefController.GetAdsWatchedKeepCombo(m_heroName); //MainModel.availableComboAdsWatched;
        m_adsProcess.fillAmount = m_adsCounter / (float)m_adsCondition;
        m_adsProcessText.text = m_adsCounter + "/" + m_adsCondition;
        if (m_skin == "" || m_weapon == "")
        {
            MainController.ClosePopup(PopupType.UnlockSkin);
            yield break;
        }

        MainModel.skinComboJustShowed = true;
        StartCoroutine(IEffectAppearance());
        yield return new WaitForSeconds(4f);
        m_buttonNoThanks.gameObject.SetActive(true);
    }

    IEnumerator ScheduleCharacterAnimation()
    {
        SoundManager.PlaySound(m_audioHeroJump, false);
        m_character.DOFade(1, 0.3f);

        TrackEntry trackEntry;
        trackEntry = m_character.AnimationState.SetAnimation(0, "xuat_hien", true);
        trackEntry.Complete += (trackEntry) => { m_character.AnimationState.SetAnimation(0, "idle", true); };
        yield return new WaitForSeconds(trackEntry.AnimationEnd - 0.2f);
        m_platform.DOShakePosition(0.2f, new Vector3(0, -50), 10);
        while (true)
        {
            yield return new WaitForSeconds(3f);
            trackEntry = m_character.AnimationState.SetAnimation(0, m_animation, false);
            trackEntry.Complete += (trackEntry) => { m_character.AnimationState.SetAnimation(0, "idle", true); };
        }
    }

    IEnumerator IEffectAppearance()
    {
        m_heroNameText.DOFade(0, 0);
        m_heroNameText.transform.localScale = Vector3.one * 3;
        m_iconS.DOFade(0, 0);
        m_iconS.transform.localScale = Vector3.one * 3;
        m_heroNameText.gameObject.SetActive(true);
        m_iconS.gameObject.SetActive(true);

        m_heroNameText.DOFade(1, 0.5f).SetEase(Ease.InQuint);
        m_heroNameText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InQuint).OnComplete(() =>
        {
            SoundManager.PlaySound(m_audioHeroName, false);
            GameObject fakeHeroName = Instantiate(m_heroNameText.gameObject, m_heroNameText.transform.position,
                Quaternion.identity, transform);
            fakeHeroName.GetComponent<TextMeshProUGUI>().DOFade(0, 1);
            fakeHeroName.transform.DOScale(Vector3.one * 1.3f, 1f);

            m_iconS.DOFade(1, 0.5f).SetEase(Ease.InQuint);
            m_iconS.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InQuint).OnComplete(() =>
            {
                SoundManager.PlaySound(m_audioTier, false);
                GameObject fakeIconS = Instantiate(m_iconS.gameObject, m_iconS.transform.position, Quaternion.identity,
                    transform);
                fakeIconS.GetComponent<Image>().DOFade(0, 1);
                fakeIconS.transform.DOScale(Vector3.one * 1.3f, 1f);
            });
        });

        m_bg.transform.DOScale(Vector3.one * 1.2f, 2f);
        m_platform.DOScale(Vector3.one, 2f);
        m_fire.transform.localPosition = new Vector2(0, -800);

        yield return new WaitForSeconds(2f);
        StartCoroutine(ScheduleCharacterAnimation());
        SoundManager.PlaySound(m_audioFireVFX, false);
        m_fire.DOLocalMoveY(280, 1f).SetEase(Ease.InQuint);
        yield return new WaitForSeconds(1f);
        m_buttonBuyWithAds.gameObject.SetActive(true);
        m_buttonBuyWithCoin.gameObject.SetActive(true);
        StartCoroutine(ICountdown());
    }

    public void UpdateSkin()
    {
        string wSkin = m_weaponAndAnims[0].weaponSkin;
        m_animation = m_weaponAndAnims[0].animation;
        foreach (WeaponAndAnim waa in m_weaponAndAnims)
        {
            if (waa.weaponSkin == m_weapon)
            {
                wSkin = waa.weaponSkin;
                m_heroName = waa.heroName;
                m_heroNameText.text = waa.heroName;
                m_animation = waa.animation;
            }
        }

        m_character.SetMixSkin(m_skin, wSkin);
        m_character.DOFade(0, 0);
    }

    public void BuyWithAds()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        if (m_skin == "" || m_weapon == "")
            return;
        if (Application.isEditor)
        {
            m_adsCounter++;
            m_adsProcessText.text = m_adsCounter + "/" + m_adsCondition;
            //MainModel.SetAdsWatchedToCollectCombo(m_adsCounter);
            PlayerPrefController.SetAdsWatchedKeepCombo(m_heroName, m_adsCounter);
            DoProcess();
            if (m_adsCounter >= m_adsCondition)
            {
                GetCombo();
            }
        }
        else
            m_adsCounter++;

        m_adsProcessText.text = m_adsCounter + "/" + m_adsCondition;
        //MainModel.SetAdsWatchedToCollectCombo(m_adsCounter);
        PlayerPrefController.SetAdsWatchedKeepCombo(m_heroName, m_adsCounter);
        DoProcess();
        if (m_adsCounter >= m_adsCondition)
        {
            GetCombo();
        }
    }

    public void BuyWithCoin()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        if (m_skin == "" || m_weapon == "")
            return;
        if (MainModel.totalCoin < m_cost)
        {
            MainController.OpenPopup(PopupType.WatchAds);
            return;
        }

        MainController.UpdateCoin(-1 * m_cost);
        GetCombo();
    }

    void GetCombo()
    {
        MainController.BuySkinWithCoin(m_skin);
        MainController.SelectSkin(m_skin);
        MainController.BuyWeaponWithCoin(m_weapon);
        MainController.SelectWeapon(m_weapon);
        MainModel.IntroduceButtonKeepSkin();
        MainModel.SetAvailableSkinToCollect("");
        MainModel.SetAvailableWeaponToCollect("");

        PlayEffectClaimCombo();
        StartCoroutine(IDelayClosePopup());
        StartCoroutine(IDelayHideButton(0.5f));
    }

    void DoProcess()
    {
        m_doProcessTweener?.Kill();
        float targetFill = (float)m_adsCounter / (float)m_adsCondition;
        m_doProcessTweener = m_adsProcess.DOFillAmount(targetFill, 0.5f);
        SoundManager.PlaySound(m_audioProcess, false);
    }

    void PlayEffectClaimCombo()
    {
        m_effectClaim.gameObject.SetActive(true);
        SoundManager.PlaySound(m_audioClaim, false);
    }

    IEnumerator IDelayClosePopup()
    {
        m_buttonNoThanks.interactable = false;
        m_buttonBuyWithAds.interactable = false;
        m_buttonBuyWithCoin.interactable = false;
        yield return new WaitForSeconds(2f);
        MainController.ClosePopup(PopupType.UnlockSkin);
    }

    IEnumerator IDelayHideButton(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        m_buttonBuyWithAds.gameObject.SetActive(false);
        m_buttonBuyWithCoin.gameObject.SetActive(false);
        m_buttonNoThanks.gameObject.SetActive(false);
    }

    IEnumerator ICountdown()
    {
        m_countdownObj.SetActive(true);
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime now = DateTime.UtcNow;
        double time = (now - startTime).TotalSeconds;
        double timeStart = double.Parse(PlayerPrefs.GetString(DataKey.KEEP_SKIN_START_TIME, time.ToString()));
        double count = double.Parse(PlayerPrefs.GetString(DataKey.KEEP_SKIN_COUNTDOWN_TIME,
            "0"));
        PlayerPrefs.SetString(DataKey.KEEP_SKIN_START_TIME, timeStart.ToString());
        PlayerPrefs.SetString(DataKey.KEEP_SKIN_COUNTDOWN_TIME, count.ToString());
        PlayerPrefs.Save();
        //
        count -= (now - startTime.AddSeconds(timeStart)).TotalSeconds;
        while (count > 0)
        {
            TimeSpan t = TimeSpan.FromSeconds(count);
            if (count < 86400)
                m_timerText.text = "End in: " + t.ToString(@"hh\:mm\:ss");
            else
            {
                int day = (int)count / 86400;
                int hour = ((int)count - (day * 86400)) / 3600;
                m_timerText.text = "End in: " + day + "d " + hour + "h";
            }

            yield return new WaitForSeconds(1f);
            count--;
        }

        MainModel.SetAvailableSkinToCollect("");
        MainModel.SetAvailableWeaponToCollect("");
        MainModel.ResetAdsWatchedToCollectCombo();
        m_countdownObj.SetActive(false);
        MainController.ClosePopup(PopupType.UnlockSkin);
    }

    public void NothankOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.ClosePopup(PopupType.UnlockSkin);
    }
}