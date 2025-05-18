
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MainController
{
    public static Action<SceneType> openSceneEvent;
    public static Action<SceneType> readySceneEvent;
    public static Action<bool, Action> sceneTransitionEvent;
    public static Action<PopupType, bool> openPopupEvent;
    public static Action<PopupType> closePopupEvent;
    public static Action finishPopupEvent;//finish show popup in queue
    public static Action<bool> activeReadyEvent;
    public static Action<int> updateCoinEvent;
    public static Action<int> updateCoinFakeEvent;
    public static Action<int> updateHeartEvent;
    public static Action<LevelResult> updateLevelResultEvent;
    public static Action claimX2DailyRewardEvent;
    public static Action<string> noticeEvent;
    public static Action<int> bonusHeartEvent;
    public static Action<int> bonusCoinEvent;
    public static Action resetUiEvent;//after claim x2 level reward
    public static Action<bool, int> activeLoadingEvent;
    public static Action updateSpinEvent;
    public static Action updateKeyTreasureEvent;
    public static Action notifyRewardsEvent;
    public static Action<int, int> showWorldLevelsEvent;//<world index>
    public static Action<bool> worldChangeEvent;//<init>
    public static Action<LevelResult> showLevelResultInLevelSelectionEvent;
    public static Action<PopupType, bool> activeHandOnButtonEvent;
    public static Action<PlayMode, bool> activateEventModeView;
    public static Action<PlayMode, bool> introduceNewModeEvent;
    public static Action updateCharacterEvent;
    public static Action<string> equipWeaponEvent;
    
    public static void LoadConfig(string storeConfig)
    {
        MainModel.LoadConfig(storeConfig);
    }

    public static void UpdateUI()
    {
        // changeSkinEvent?.Invoke("", MainModel.currentSkin);
        updateCharacterEvent?.Invoke();
        updateLevelResultEvent?.Invoke(MainModel.levelResult);
        updateHeartEvent?.Invoke(MainModel.totalHeart);
        updateSpinEvent?.Invoke();
        updateKeyTreasureEvent?.Invoke();
        ChangeWorld(true);
    }

    public static void OpenPopup(PopupType type, bool playAnim = true)
    {
        openPopupEvent?.Invoke(type, playAnim);        
    }

    public static void ClosePopup(PopupType type)
    {
        closePopupEvent?.Invoke(type);
    }

    public static void UpdateCoin(int coin)
    {
        MainModel.UpdateTotalCoin(coin);
        updateCoinEvent?.Invoke(MainModel.totalCoin);
        NotifyRewards();
    }

    public static void UpdateHeart(int heart)
    {
        MainModel.UpdateHeart(heart);
        updateHeartEvent?.Invoke(MainModel.totalHeart);
    }

    public static void BuySkinWithAds(string skin)
    {
        MainModel.BuySkin(skin);
    }

    public static void BuySkinWithCoin(string skin)
    {
        MainModel.BuySkin(skin);
    }
    
    public static void BuyWeaponWithAds(string weaponSkinName)
    {
        MainModel.BuyWeapon(weaponSkinName);
    }

    public static void BuyWeaponWithCoin(string weaponSkinName)
    {
        MainModel.BuyWeapon(weaponSkinName);
    }

    public static void TrySkin(string skin)
    {
        MainModel.trySkin = skin;
        GameController.ChangeSkin(skin);
    }

    public static void TryWeapon(WeaponName weaponName)
    {
        MainModel.tryWeapon = ConfigLoader.instance.config.GetWeapon(weaponName).weapon.skin;
        EquipWeapon(weaponName);
    }
    
    public static void EquipWeapon(WeaponName newWeapon)
    {
        equipWeaponEvent?.Invoke(ConfigLoader.instance.config.GetWeapon(newWeapon).weapon.skin);
        if(!MainModel.equipedWeapons.Contains(newWeapon.ToString()))
        {
            switch (newWeapon)
            {
                case WeaponName.BattleAxe:
                case WeaponName.DeathBattleAxe:
                    GameController.ActivateInputTutorial(InputTutorialType.Hold, InputTutorialTarget.Fight, true);
                    break;
                default:
                    break;
            }
            MainModel.SaveEquipWeapons(newWeapon);
        }
    }

    // public static void TrySkin(string skin, bool tracking = true)
    // {
    //     SelectSkin(skin);
    //     //
    //     if(tracking)
    //         TrackingManager.WatchAdsTrySkin(skin);
    // }

    public static void SelectSkin(string skin)
    {
        MainModel.SetCurrentSkin(skin);
        updateCharacterEvent?.Invoke();
    }

    public static void SelectWeapon(string weaponSkinName)
    {
        MainModel.SetCurrentWeapon(weaponSkinName);
        updateCharacterEvent?.Invoke();
    }

    public static void FinishPopup()
    {
        finishPopupEvent?.Invoke();
    }

    public static void ClaimX2DailyReward()
    {
        claimX2DailyRewardEvent?.Invoke();
    }

    public static void SubscribeVip(bool active)
    {
        MainModel.Subscribe(active);
    }

    public static void RemoveAds()
    {
        MainModel.RemoveAds();
    }

    public static void SkipLevel()
    {
        Action cb = () =>
        {
            GameInfo info = MainModel.gameInfo;
            ConfigLoader.instance.SaveMapLevel(ConfigLoader.instance.mapLevel, ConfigLoader.instance.worldLevel, 0, info.playMode);
            if(ConfigLoader.IsBossLevel(info.levelPath))
                MainModel.UpdateBossCollection(ConfigLoader.GetBossName(info.levelPath));
            MainModel.InitGameInfo(ConfigLoader.instance.mapLevel, ConfigLoader.instance.worldLevel, info.playMode);
            DoSceneTrasition(false, () =>
            {
                OpenScene(SceneType.Game);
            });
        };
            cb();
    }

    public static void X2LevelReward()
    {
        BonusCoin(MainModel.levelResult.coin*MapConstant.COIN_RATIO);
        resetUiEvent?.Invoke();
    }

    public static void ShowNotice(string message)
    {
        noticeEvent?.Invoke(message);
    }

    public static void BonusHeart(int heart)
    {
        MainModel.UpdateHeart(heart);
        bonusHeartEvent?.Invoke(heart);
    }

    public static void BonusCoin(int coin)
    {
        MainModel.UpdateTotalCoin(coin);
        bonusCoinEvent?.Invoke(coin);
        NotifyRewards();
    }

    public static void ActiveLoading(bool active, int childIndex)
    {
        activeLoadingEvent?.Invoke(active, childIndex);
    }

    public static void UpdateFakeCoin(int finalCoin)
    {
        updateCoinFakeEvent?.Invoke(finalCoin);
    }

    public static void DoSceneTrasition(bool isOpen, Action action)
    {
        sceneTransitionEvent?.Invoke(isOpen, action);
    }

    public static void NotifyRewards()
    {
        notifyRewardsEvent?.Invoke();
    }

    public static void SimulateUpdateCoin(int value)
    {
        updateCoinEvent?.Invoke(value);
    }

    public static void SimulateUpdateHeart(int value)
    {
        updateHeartEvent?.Invoke(value);
    }

    public static void ShowWorldLevels(int worldIndex)
    {
        showWorldLevelsEvent?.Invoke(worldIndex, 0);
    }

    public static void ChangeWorld(bool init)
    {
        worldChangeEvent?.Invoke(init);
    }

    public static void PlayGame(int world, int level, PlayMode playMode)
    {
        MainModel.InitGameInfo(level, world, playMode);
        switch (playMode)
        {
            case PlayMode.Normal:
                break;
            case PlayMode.Boss:
                string bossName = ConfigLoader.instance.config.bossLevels[level].bossName;
                MainModel.ClearNotiBossUnlock(bossName);
                break;
        }
        TransitionSceneGame();
    }

    public static void TransitionSceneGame()
    {
        DoSceneTrasition(false, () => { OpenScene(SceneType.Game); });
    }

    public static void OpenScene(SceneType sceneType)
    {
        openSceneEvent?.Invoke(sceneType);
    }

    public static void SceneReady(SceneType sceneType)
    {
        readySceneEvent?.Invoke(sceneType);
    }

    public static void ActiveReady (bool ready)
    {
        activeReadyEvent?.Invoke(ready);
    }

    public static void ShowResultInLevelSelection(LevelResult result)
    {
        showLevelResultInLevelSelectionEvent?.Invoke(result);
    }

    public static void ActiveHandOnButton(PopupType targetButton, bool toActive)
    {
        activeHandOnButtonEvent?.Invoke(targetButton, toActive);
    }

    public static void ActivateEventModeView(PlayMode playMode, bool toShow)
    {
        activateEventModeView?.Invoke(playMode, toShow);
    }

    public static void IntroduceNewMode(PlayMode mode, bool toShow)
    {
        introduceNewModeEvent?.Invoke(mode, toShow);
        ConfigLoader.instance.SaveIntroducedModes(mode);
    }

    public static void ReplayGame(int world, int level, PlayMode playMode)
    {
        MainModel.InitGameInfo(level, world, playMode);
        DoSceneTrasition(false, () => { OpenScene(SceneType.Game); });
    }
}
