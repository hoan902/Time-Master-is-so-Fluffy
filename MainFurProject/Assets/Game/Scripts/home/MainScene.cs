using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameConstant;
using Random = UnityEngine.Random;

public class MainScene : MonoBehaviour
{
    public static Transform m_avatarObject; //to show effect bonus heart
    public static Transform m_coinObject; //to show effect bonus coin

    [Header("UI-Text")] [SerializeField] private TextMeshProUGUI m_textHeart;
    [SerializeField] private TextMeshProUGUI m_textLevel;
    [SerializeField] private TextMeshProUGUI m_textLevelButtonPlay;
    [SerializeField] private TextMeshProUGUI m_textLevelButtonNext;
    [SerializeField] private TextMeshProUGUI m_textKeepComboCountdown;

    [Header("UI-Objects")] [SerializeField]
    private Image m_avatar;

    [SerializeField] private GameObject m_objComplete;
    [SerializeField] private GameObject m_objFail;
    [SerializeField] private GameObject m_buttonPlay;
    [SerializeField] private Transform m_coinLevelStartPos;
    [SerializeField] private RectTransform m_coinCompleteHolder;
    [SerializeField] private Transform m_coinCompleteFinishPos;
    [SerializeField] private GameObject m_effectWin;
    [SerializeField] private GameObject m_buttonModeSelection;
    [SerializeField] private GameObject m_buttonKeepCombo;
    [SerializeField] private SkeletonGraphic m_hand;
    [SerializeField] private SkeletonGraphic m_characterSpine;
    [SerializeField] private GameObject[] m_playButtons;
    [SerializeField] private Image m_background;
    
    [Header("Sounds")] [SerializeField] private AudioClip m_music; //music-main
    [SerializeField] private AudioClip m_audioCollectCoin; //collect-coin
    [SerializeField] private AudioClip m_audioVictory; //victory
    [SerializeField] private AudioClip m_audioRainCoin; //rain-coin
    [SerializeField] private AudioClip m_audioGameOver; //gameover

    private bool m_ready;
    private bool m_showWorldTarget;
    private List<GameObject> m_coins;
    private LevelResult m_levelResult;
    private bool m_hasPopup;
    private static bool m_showDaily = false;
    private Coroutine m_luckyTimer;
    private GameObject m_soundRainCoin;
    private Coroutine m_showNoThanksRoutine;
    private GameObject m_soundBGObject;
    private Tweener m_adjustMusicVolumeTween;

    void Awake()
    {
        MainController.updateCharacterEvent += OnChangeSkin;
        MainController.finishPopupEvent += OnFinishPopup;
        MainController.closePopupEvent += OnClosePopup;
        MainController.updateHeartEvent += OnUpdateHeart;
        MainController.updateLevelResultEvent += OnUpdateLevelResult;
        MainController.resetUiEvent += OnResetUi;
        MainController.notifyRewardsEvent += OnUpdateNotiReward;
        MainController.worldChangeEvent += OnWorldChange;
        MainController.readySceneEvent += OnReady;
        MainController.activeReadyEvent += OnActiveReady;
        MainController.activeHandOnButtonEvent += OnHandActivate;
        MainController.activateEventModeView += OnEventModeViewActivated;
        //
        m_avatarObject = m_avatar.transform;
        m_coinObject = m_coinCompleteFinishPos;
        //
        //m_buttonModeSelection.SetActive(ConfigLoader.instance.fakeMapLevel > RemoteConfig.levelShowButtonBossCollection);
        m_buttonModeSelection.SetActive(true);
    }

    void OnDestroy()
    {
        MainController.updateCharacterEvent -= OnChangeSkin;
        MainController.finishPopupEvent -= OnFinishPopup;
        MainController.closePopupEvent -= OnClosePopup;
        MainController.updateHeartEvent -= OnUpdateHeart;
        MainController.updateLevelResultEvent -= OnUpdateLevelResult;
        MainController.resetUiEvent -= OnResetUi;
        MainController.notifyRewardsEvent -= OnUpdateNotiReward;
        MainController.worldChangeEvent -= OnWorldChange;
        MainController.readySceneEvent -= OnReady;
        MainController.activeReadyEvent -= OnActiveReady;
        MainController.activeHandOnButtonEvent -= OnHandActivate;
        MainController.activateEventModeView -= OnEventModeViewActivated;
        //
        if (m_soundRainCoin != null)
            Destroy(m_soundRainCoin);
    }

    void Start()
    {
        InitUI();
        MainController.UpdateUI();
        StartCoroutine(SceneIn());
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            MainController.OpenPopup(PopupType.SkinSelection, false);
            // MainController.IntroduceNewMode(PlayMode.RedBlue, true);
            // MainController.IntroduceNewMode(PlayMode.StickFight, true);
            // MainController.IntroduceNewMode(PlayMode.Ramadan, true);
        }
        // else if(Input.GetKeyDown(KeyCode.F2))
        //     MainController.IntroduceNewMode(PlayMode.StickFight, true);
        // else if(Input.GetKeyDown(KeyCode.F3))
        //     MainController.IntroduceNewMode(PlayMode.Ramadan, true);
    }
#endif

    void InitUI()
    {
        m_characterSpine.SetMixSkin(MainModel.currentSkin, MainModel.currentWeapon);
    }

    private void OnReady(SceneType sceneType)
    {
        if (sceneType != SceneType.Home)
            return;
        MainController.ActiveReady(true);
    }

    private void OnActiveReady(bool ready)
    {
        m_ready = ready;
    }

    private void OnWorldChange(bool init)
    {
        if (init)
        {
            int targetWorldIndex = ConfigLoader.instance.worldLevel % ConfigLoader.instance.worlds.Count;
            /*if (RemoteConfig.worldLoopBegin > 0 && targetWorldIndex >= RemoteConfig.worldLoopBegin)
            {
                targetWorldIndex = targetWorldIndex % RemoteConfig.worldLoopBegin;
            }*/

            m_background.sprite = ConfigLoader.instance.worlds[targetWorldIndex].background;
        }
    }

    private void OnHandActivate(PopupType popupType, bool toActive)
    {
        if (!toActive)
        {
            m_hand.gameObject.SetActive(false);
            return;
        }

        m_hand.gameObject.SetActive(true);
        m_hand.AnimationState.SetAnimation(0, "click1", true);
        switch (popupType)
        {
            case PopupType.Treasure:
                foreach (GameObject button in m_playButtons)
                {
                    if (button.activeInHierarchy)
                    {
                        m_hand.transform.SetParent(button.transform);
                        m_hand.GetComponent<RectTransform>().anchoredPosition = new Vector2(80, -100);
                        m_hand.transform.eulerAngles = new Vector3(0, 0, 45);
                        m_hand.transform.localScale = new Vector3(80, 80, 1);
                        break;
                    }
                }
                break;
            case PopupType.ModeSelection:
                m_hand.transform.SetParent(m_buttonModeSelection.transform);
                m_hand.GetComponent<RectTransform>().anchoredPosition = m_buttonModeSelection.transform.Find("spine")
                    .GetComponent<RectTransform>().anchoredPosition;
                m_hand.transform.localScale = new Vector3(80, 80, 1);
                m_hand.transform.eulerAngles = new Vector3(0, 0, 45);
                break;
            default:
                m_hand.gameObject.SetActive(false);
                break;
        }
    }

    private void OnResetUi()
    {
        m_objComplete.transform.Find("button-select-level").gameObject.SetActive(false);
        m_objComplete.transform.Find("button-x2-reward").gameObject.SetActive(false);
        m_objComplete.transform.Find("button-x2-reward-ads").gameObject.SetActive(false);
        m_objComplete.transform.Find("text-nothanks").gameObject.SetActive(false);
        m_buttonPlay.SetActive(true);
        // m_buttonSelectLevel.SetActive(true);
    }

    private void OnUpdateLevelResult(LevelResult obj)
    {
        m_levelResult = obj;
        int mapLevel = ConfigLoader.instance.mapLevel;
        int worldLevel = ConfigLoader.instance.worldLevel;
        string levelName = ConfigLoader.GetLevelString(worldLevel, mapLevel);
        m_textLevel.text = levelName;
        m_textLevelButtonPlay.text = levelName;
        m_textLevelButtonNext.text = levelName;

        if (obj == null || obj.playMode != PlayMode.Normal)
        {
            AudioClip musicByWorld = ConfigLoader.instance.GetHomeMusicByWorld(ConfigLoader.instance.worldLevel, m_music);
            m_soundBGObject = SoundManager.PlaySound(musicByWorld, true, true);
            m_objComplete.SetActive(false);
            m_objFail.SetActive(false);
            m_buttonPlay.SetActive(true);
            // m_buttonSelectLevel.SetActive(true);
        }
        else
        {
            m_buttonPlay.SetActive(false);
            if (obj.isComplete)
            {
                m_objComplete.SetActive(true);
                m_objFail.SetActive(false);
            }
            else
            {
                //OnUpdateCoin(m_currentCoins);
                m_objComplete.SetActive(false);
                m_objFail.SetActive(true);
            }
        }

        //
        if (SystemVariable.updateReady)
            MainController.OpenPopup(PopupType.Update);
    }

    private void OnUpdateHeart(int heart)
    {
        m_textHeart.text = heart + "";
    }

    private void OnChangeSkin()
    {
        m_avatar.sprite = ConfigLoader.instance.config.GetPlayerAvatar(MainModel.currentSkin);
        m_avatar.SetNativeSize();
        m_characterSpine.SetMixSkin(MainModel.currentSkin, MainModel.currentWeapon);
    }

    private void OnFinishPopup()
    {
        /*AdsManager.api.ShowInterstitial("popup_back", () =>
        {
            if (m_levelResult != null)
            {
                if (m_levelResult.isComplete)
                    ShowEffectWin(m_levelResult);
                m_levelResult = null;
            }
            else if (SystemVariable.updateReady)
                MainController.OpenPopup(PopupType.Update);
        });*/
        if (m_levelResult != null)
        {
            if (m_levelResult.isComplete)
                ShowEffectWin(m_levelResult);
            m_levelResult = null;
        }
        else if (SystemVariable.updateReady)
            MainController.OpenPopup(PopupType.Update);
        //
        OnUpdateNotiReward();
        IntroduceButtonKeepCombo();
    }
    void OnClosePopup(PopupType popupType)
    {
        switch (popupType)
        {
            case PopupType.UnlockSkin:
                m_buttonKeepCombo.gameObject.SetActive(MainModel.availableWeaponToUnlock != "" && MainModel.availableSkinToUnlock != "");
                break;
        }
    }

    void OnEventModeViewActivated(PlayMode playMode, bool toShow)
    {
        //AudioClip musicToPlay = toShow ? ConfigLoader.instance.GetHomeMusicByMode(playMode, m_music) : m_music;
        //if(m_soundBGObject != null)
        //{
        //    if(m_soundBGObject.GetComponent<AudioSource>().clip.name == musicToPlay.name) // ignore if same music playing
        //        return;
        //    float currentMusicVolume = SoundManager.GetCurrentMusicVolume();
        //    m_adjustMusicVolumeTween?.Kill();
        //    m_adjustMusicVolumeTween = DOTween.To(() => currentMusicVolume, x => currentMusicVolume = x, 0, 0.5f).OnUpdate(() => {
        //        SoundManager.AdjustVolumeMusic(currentMusicVolume);
        //    }).OnComplete(() => {
        //        m_soundBGObject = SoundManager.PlaySound(musicToPlay, true, true);
        //        m_adjustMusicVolumeTween = DOTween.To(() => currentMusicVolume, x => currentMusicVolume = x, 1, 0.5f).OnUpdate(() => {
        //            SoundManager.AdjustVolumeMusic(currentMusicVolume);
        //        });
        //    });
        //}
        //else
        //    m_soundBGObject = SoundManager.PlaySound(musicToPlay, true, true);
    }
    public void PlayOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        if (!m_ready)
            return;
        MainController.ActiveReady(false);
        MainController.PlayGame(ConfigLoader.instance.worldLevel, ConfigLoader.instance.mapLevel, PlayMode.Normal);
        //AdsManager.Instance.HideBanner();
    }

    public void RetryOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        if (!m_ready)
            return;
        MainController.ActiveReady(false);
        MainController.PlayGame(MainModel.levelResult.worldLevel, MainModel.levelResult.mapLevel,
            MainModel.levelResult.playMode);
    }

    public void ModeSelectionOnClick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.OpenPopup(PopupType.BossCollection);
    }
    
    public void DailyRewardOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.OpenPopup(PopupType.DailyReward);
    }

    public void HeartStoreOnClick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.OpenPopup(PopupType.HeartStore);
    }

    public void KeepSkinOnClick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.OpenPopup(PopupType.UnlockSkin, false);
    }

    public void SkinSelectionOnClick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.OpenPopup(PopupType.SkinSelection, false);
    }

    public void UpdateInfoUserOnclick()
    {
        //TrackingManager.Impression("IMPRESSION_AVATAR");
    }

    public void HeartAdsOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.BonusHeart(GameConstant.BONUS_HEART_ADS);
    }

    public void SkipLevelOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.SkipLevel();
    }

    public void X2LevelRewardOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.X2LevelReward();
        if (m_showNoThanksRoutine != null)
            StopCoroutine(m_showNoThanksRoutine);
    }

    public void X2FreeLevelRewardOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.X2LevelReward();
        if (m_showNoThanksRoutine != null)
            StopCoroutine(m_showNoThanksRoutine);
    }

    void ShowLevelComplete(LevelResult result) // show main scene sau khi complete game
    {
        /*if (!result.isReplay)
        {
            //check remove ads
            if (result.fakeLevelIndex == RemoteConfig.levelShowPopupRemoveAds)
            {
                m_hasPopup = true;
                StartCoroutine(ScheduleShowRemoveAds());
            }

            // heart store
            if (RemoteConfig.levelsShowBoosterStore.Contains(result.fakeLevelIndex + ""))
            {
                m_hasPopup = true;
                StartCoroutine(ScheduleShowHeartStore());
            }
        }*/

        if (m_hasPopup)
            return;
        ShowEffectWin(result);
    }

    void ShowEffectWin(LevelResult result) //
    {
        m_levelResult = null;
        SoundManager.PlaySound(m_audioVictory, false);
        StartCoroutine(IShowWinEffect());
        /*if (result.isReplay || result.fakeLevelIndex > RemoteConfig.levelShowX2Reward)
        {
            GameObject buttonX2CoinAds = m_objComplete.transform.Find("button-x2-reward-ads").gameObject;
            if (RemoteConfig.levelModX2Button <= 0)
                RemoteConfig.levelModX2Button = 1;
            bool hasX2Ads = result.fakeLevelIndex % RemoteConfig.levelModX2Button == 0;

            buttonX2CoinAds.SetActive(hasX2Ads);
            m_objComplete.transform.Find("button-x2-reward").gameObject.SetActive(false);
            if (m_showNoThanksRoutine != null)
                StopCoroutine(m_showNoThanksRoutine);
            m_showNoThanksRoutine = StartCoroutine(ScheduleShowNoThanks());
        }
        else
        {
            m_objComplete.transform.Find("button-x2-reward-ads").gameObject.SetActive(false);
            Transform x2Free = m_objComplete.transform.Find("button-x2-reward");
            x2Free.gameObject.SetActive(true);
            x2Free.Find("hand").gameObject.SetActive(result.fakeLevelIndex < 3);
            if (result.fakeLevelIndex > 1)
            {
                if (m_showNoThanksRoutine != null)
                    StopCoroutine(m_showNoThanksRoutine);
                m_showNoThanksRoutine = StartCoroutine(ScheduleShowNoThanks());
            }
        }*/

        StartCoroutine(SchedulePlaySoundBG(4f));
        //
        float time = ((result.coin > 20 ? 20 : result.coin) + 1) * 0.1f + 1.5f;
        StartCoroutine(CreateCompleteCoin(result.coin));
        //
        /*if (result.fakeLevelIndex == RemoteConfig.levelShowPopupRateUs ||
            RemoteConfig.levelsShowRateUs.ToList().Contains(result.fakeLevelIndex.ToString()))
            InAppReviewManager.instance.Rate();
        else */if (SystemVariable.updateReady)
            MainController.OpenPopup(PopupType.Update);

    }

    void IntroduceNewMode()
    {
        List<PlayMode> introducedModes = ConfigLoader.instance.LoadIntroducedModes();
    }

    void IntroduceButtonKeepCombo()
    {
        if (MainModel.buttonKeepSkinIntroduced || !MainModel.skinComboJustShowed || MainModel.availableWeaponToUnlock == "" || MainModel.availableSkinToUnlock == "")
            return;
        MainModel.skinComboJustShowed = false;
        MainModel.IntroduceButtonKeepSkin();
        StartCoroutine(ScheduleShowButtonKeepCombo());
    }

    IEnumerator IShowWinEffect()
    {
        m_effectWin.SetActive(true);
        yield return new WaitForSeconds(5f);
        m_effectWin.SetActive(false);
    }

    IEnumerator ScheduleShowRemoveAds()
    {
        yield return new WaitForSeconds(0.5f);
        MainController.OpenPopup(PopupType.RemoveAds);
    }

    IEnumerator ScheduleShowHeartStore()
    {
        yield return new WaitForSeconds(0.6f);
        MainController.OpenPopup(PopupType.HeartStore);
    }

    IEnumerator SchedulePlaySoundBG(float time)
    {
        if (m_soundBGObject != null)
            yield break;
        yield return new WaitForSeconds(time);
        AudioClip musicByWorld = ConfigLoader.instance.GetHomeMusicByWorld(ConfigLoader.instance.worldLevel, m_music);
        m_soundBGObject = SoundManager.PlaySound(musicByWorld, true, true);
    }

    IEnumerator ScheduleShowNoThanks()
    {
        //yield return new WaitForSeconds(RemoteConfig.timeDelayNothanks / 1000f);
        yield return new WaitForSeconds(4f / 1000f);
        GameObject nothanks = m_objComplete.transform.Find("text-nothanks").gameObject;
        nothanks.SetActive(true);
    }

    IEnumerator ScheduleShowButtonKeepCombo()
    {
        yield return new WaitForSeconds(0.5f);
        m_buttonKeepCombo.SetActive(true);
        SkeletonGraphic buttonIcon = m_buttonKeepCombo.transform.Find("spine").GetComponent<SkeletonGraphic>();
        buttonIcon.DOFade(0, 0f);
        GameObject chestItem = Instantiate(buttonIcon.gameObject, transform);
        chestItem.transform.position = m_background.transform.position;
        SkeletonGraphic chestSpine = chestItem.GetComponent<SkeletonGraphic>();
        chestSpine.DOFade(1, 0);

        chestItem.transform.DOMove(buttonIcon.transform.position, 1f).OnComplete(() =>
        {
            chestItem.transform.DOScale(new Vector3(1.5f, 1.5f, 1), 0.5f);
            chestSpine.DOFade(0, 0.5f);
            buttonIcon.color = Color.white;
            StartCoroutine(ICountdownKeepCombo());
        });
    }
    IEnumerator ICountdownKeepCombo()
    {
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime now = DateTime.UtcNow;
        double time = (now - startTime).TotalSeconds;
        double timeStart = double.Parse(PlayerPrefs.GetString(DataKey.KEEP_SKIN_START_TIME, time.ToString()));
        //double count = double.Parse(PlayerPrefs.GetString(DataKey.KEEP_SKIN_COUNTDOWN_TIME, RemoteConfig.skinComboLifetime.ToString()));
        double count = double.Parse(PlayerPrefs.GetString(DataKey.KEEP_SKIN_COUNTDOWN_TIME, "6000"));
        PlayerPrefs.SetString(DataKey.KEEP_SKIN_START_TIME, timeStart.ToString());
        PlayerPrefs.SetString(DataKey.KEEP_SKIN_COUNTDOWN_TIME, count.ToString());
        PlayerPrefs.Save();
        //
        count -= (now - startTime.AddSeconds(timeStart)).TotalSeconds;
        while (count > 0)
        {
            TimeSpan t = TimeSpan.FromSeconds(count);
            if (count < 86400)
                m_textKeepComboCountdown.text = t.ToString(@"hh\:mm\:ss");
            else
            {
                int day = (int)count / 86400;
                int hour = ((int)count - (day * 86400)) / 3600;
                m_textKeepComboCountdown.text = day + "d " + hour + "h";
            }
            yield return new WaitForSeconds(1f);
            count--;
        }
        MainModel.SetAvailableSkinToCollect("");
        MainModel.SetAvailableWeaponToCollect("");
        m_buttonKeepCombo.SetActive(false);
    }

    IEnumerator CoinMoveFinish(int coin)
    {
        int last = MainModel.totalCoin - coin;
        int ratio = coin / m_coins.Count;
        for (int i = 0; i < m_coins.Count; i++)
        {
            GameObject go = m_coins[i];
            Vector3 pos1 = go.transform.position;
            Vector3 pos2 = pos1 + new Vector3(-1, 1);
            Vector3 pos3 = m_coinCompleteFinishPos.position;
            Vector3[] path = new Vector3[] { pos1, pos2, pos3 };
            go.transform.DOPath(path, 0.8f, PathType.CatmullRom).SetEase(Ease.InCubic).OnComplete(() =>
            {
                SoundManager.PlaySound(m_audioCollectCoin, false);
                go.transform.DOScale(new Vector3(3, 3, 1), 0.5f);
                go.GetComponent<Image>().DOFade(0, 0.5f).OnComplete(() =>
                {
                    MainController.SimulateUpdateCoin(last + Mathf.Min((i + 1) * ratio, coin));
                    Destroy(go);
                });
            });
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator CreateCompleteCoin(int count)
    {
        if (count == 0)
        {
            yield break;
        }

        m_soundRainCoin = SoundManager.PlaySound(m_audioRainCoin, true);
        m_coins = new List<GameObject>();
        int counter = 0;
        count = count > 20 ? 20 : count;
        GameObject coinObj = m_coinCompleteHolder.Find("coin").gameObject;
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(coinObj);
            m_coins.Add(go);
            go.SetActive(true);
            go.transform.SetParent(m_coinCompleteHolder, false);
            go.transform.position = m_coinLevelStartPos.position;
            float x = Random.Range(-m_coinCompleteHolder.sizeDelta.x / 2, m_coinCompleteHolder.sizeDelta.x / 2);
            float y = Random.Range(-m_coinCompleteHolder.sizeDelta.y / 2, m_coinCompleteHolder.sizeDelta.y / 2);
            Vector2 pos = new Vector2(x, y);
            go.transform.DOLocalMove(pos, 0.1f).OnComplete(() =>
            {
                counter++;
                if (counter >= count)
                {
                    if (m_soundRainCoin != null)
                        Destroy(m_soundRainCoin);
                    StartCoroutine(CoinMoveFinish(count * MapConstant.COIN_RATIO));
                }
            });
            yield return new WaitForSeconds(0.05f);
        }
    }

    void ShowLevelFail()
    {
        SoundManager.PlaySound(m_audioGameOver, false);
        StartCoroutine(SchedulePlaySoundBG(3.5f));
    }

    IEnumerator SceneIn()
    {
        m_hasPopup = false;
        m_showWorldTarget = false;
        StartCoroutine(SchedulePlaySoundBG(0.1f));
        //unlock skin
        if (m_levelResult != null && m_levelResult.isComplete)
        {
            if (MainModel.levelResult.skin != "" && !m_levelResult.isReplay)
            {
                m_hasPopup = true;
                MainController.OpenPopup(PopupType.UnlockSkin, false);
            }
        }
        //
        m_buttonKeepCombo.SetActive(MainModel.availableSkinToUnlock != "" && MainModel.availableWeaponToUnlock != "" && MainModel.buttonKeepSkinIntroduced);
        if(m_buttonKeepCombo.activeSelf)
            StartCoroutine(ICountdownKeepCombo());
        PlayMode mode = PlayMode.Normal;
        if (MainModel.gameInfo == null) //go home from first open and will show normal mode
            mode = PlayMode.Normal;
        else
            mode = MainModel.gameInfo.playMode;
        switch (mode)
        {
            case PlayMode.Normal:
                // daily reward
                if (PopupDailyReward.hasDailyReward && !m_showWorldTarget) // check renew booster store at the same time
                {
                    if (!m_showDaily)
                    {
                        m_showDaily = true;
                        m_hasPopup = true;
                        MainController.OpenPopup(PopupType.DailyReward);
                    }
                }
                if (m_levelResult == null || m_hasPopup)
                    yield break;
                while (!m_ready)
                {
                    yield return null;
                }
                if (m_levelResult.isComplete)
                    ShowLevelComplete(m_levelResult);
                else
                    ShowLevelFail();
                break;
            case PlayMode.Boss:
                MainController.OpenPopup(PopupType.BossCollection, false);
                break;
        }
    }

    void OnUpdateNotiReward()
    {
        //daily
        Transform dailyNoti = transform.Find("main-panel/button-daily-reward/noti");
        dailyNoti.gameObject.SetActive(PopupDailyReward.hasDailyReward);
        //boss
        GameObject bossNoti = m_buttonModeSelection.transform.Find("noti").gameObject;
        bossNoti.SetActive(!string.IsNullOrEmpty(MainModel.newBossUnlock));
    }
}