using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController
{
    //camera
    public static Action<float> updateTargetYCameraEvent;
    public static Action<Vector3> updateRevivalCameraEvent;
    public static Action<float, float> zoomCameraEvent;//<ratio, time>
    public static Action shakeCameraEvent;
    public static Action<float> vibrateCameraEvent;
    public static Action vibrateCameraYEvent;
    public static Action<Vector3, float> vibrateCustomEvent;
    public static Action shakeCameraWeakEvent;
    public static Action<float> shakeCameraLoopEvent;
    public static Action<float> updateBlendTimeEvent;
    public static Action<Season, float> changeBackgroundEvent;
    public static Action<ObjectChangeCameraOffset, bool> changeCameraOffsetEvent;
    //player
    public static Action<bool> stopPlayerEvent;//<isFinish>
    public static Action resumePlayerEvent;
    public static Action<string> changeSkinEvent;
    public static Action<string, string> showCollectWeaponEffectEvent;

    public static Action<int, Vector3?> updateHeartEvent;
    public static Action showProgressLevelEvent;
    public static Action levelLoadedEvent;
    public static Action readyEvent;
    public static Action readyPlayEvent;
    public static Action<Vector2, Vector2> finishEvent;
    public static Action loadMapEvent;
    public static Action<int> initUIEvent;//<max star>
    public static Action<int, Vector3?, int> updatePointEvent;//<quantity, start pos, direction>
    public static Action<int> updateHealthEvent;//percent
    public static Action<Vector2?> loadSavePointEvent;
    public static Action checkedPointEvent;//call when player trigger checkpoint
    public static Action<Vector2> buffHeartEvent;
    public static Action<string, bool, GameObject?> triggerEvent;
    public static Action<int> keyPickupEvent;
    public static Action<string, Vector2> keyActiveEvent;
    public static Action<bool> ballHurtEvent;//<isDead>    
    public static Action activeMagnetEvent;
    public static Action<object> activeBoostItemEvent;
    public static Action<object> useBoostItemEvent;
    public static Action boostItemEvent;
    public static Action<int, Vector3?, int> updateTreasureKeyEvent;//<quantity, start pos, direction>
    public static Action playerWinEvent; //player play win
    public static Action<QuitGameReason, bool> quitEvent;
    public static Action<int, int> updateBossHpEvent;//<max, current>
    public static Action<bool> activeInputEvent;
    public static Action bossReadyEvent;
    public static Action bossAppearEvent;
    public static Action<int, Vector3?> getCoinAdsEvent;
    public static Action<Vector3> teleportEvent;
    public static Action<Vector3> maskTeleClosedEvent;
    public static Action skipCutsceneEvent;
    public static Action maskSkipCutsceneClosedEvent;
    public static Action eatItemEvent;
    public static Action<Weather> updateWeatherEvent;
    public static Action<bool> activateUIGameSceneEvent;
    public static Action updateLevelCoinEvent;
    public static Action updateScoreEvent;
    public static Action activeTextEffectScoreEvent;
    public static Action<GameObject> monsterDeadEvent;
    public static Action<GameObject> objectDestroyedEvent;
    public static Action<InputTutorialType, InputTutorialTarget, bool> activateInputTutorialEvent; 

    public static void Init()
    {
        // only play normal mode from begin
        if (MainModel.gameInfo == null)
            MainModel.InitGameInfo(ConfigLoader.instance.mapLevel, ConfigLoader.instance.worldLevel, PlayMode.Normal);
        GameInfo info = MainModel.gameInfo;
        updateHealthEvent?.Invoke(info.health);
        loadMapEvent?.Invoke();
        updateTreasureKeyEvent?.Invoke(info.treasureKeys, null, 0);
        updateHeartEvent?.Invoke(MainModel.totalHeart, null);
        ObjectFollowable.followableObjectsCollectedCount = 0;
        //
        STGameController.Init();
    }

    public static void UpdatePoint(int value, Vector3? itemPos, int direction)
    {
        MainModel.levelResult.point += value;
        if(MainModel.gameInfo.playMode == PlayMode.Normal)
            UpdatePointStars(value);
        updatePointEvent?.Invoke(MainModel.levelResult.remainPoint, itemPos, direction);
    }

    public static void UpdateBossHp(int max, int current)
    {
        updateBossHpEvent?.Invoke(max, current);
    }

    public static void UpdateCoin(int value)
    {
        MainModel.gameInfo.levelCoin += value;
        updateLevelCoinEvent?.Invoke();
    }

    public static void ChangeSkin(string skin)
    {
        changeSkinEvent?.Invoke(skin);
    }
    
    public static void Finish(Vector3 startFinishPoint, Vector3 finishPoint)
    {
        finishEvent?.Invoke(startFinishPoint, finishPoint);
        StopPlayer(true);
    }

    public static void SavePoint(Vector2 position)
    {
        MainModel.gameInfo.savePoint = position;
        checkedPointEvent?.Invoke();
    }

    public static void UpdateHealth(int heath)
    {
        if (MainModel.gameInfo.health <= 0 || MainModel.gameInfo.immortal)
            return;
        MainModel.gameInfo.startImmortalTime = Time.time;
        MainModel.gameInfo.health += heath;
        if (MainModel.gameInfo.health > 3)
            MainModel.gameInfo.health = 3;
        updateHealthEvent?.Invoke(MainModel.gameInfo.health);
        if (heath < 0)
            ballHurtEvent?.Invoke(MainModel.gameInfo.health <= 0);
    }

    public static void RestartGame(bool force = false)
    {
        MainModel.InitGameInfo(MainModel.gameInfo.level, MainModel.gameInfo.world, MainModel.gameInfo.playMode);
        Quit(QuitGameReason.Restart, force);
    }

    public static void RestartLevel()
    {
        StopPlayer();
        if (MainModel.totalHeart > 0 || (MainModel.levelResult != null && MainModel.levelResult.bonusHeart > 0))
        {
            if (MainModel.levelResult != null && MainModel.levelResult.bonusHeart < 1)
                MainController.UpdateHeart(-1);
            Revive();
        }
        else
        {
            MainController.OpenPopup(PopupType.ReviveStick);
        }
            
    }

    public static void Revive()
    {
        if (MainModel.gameInfo.savePoint == null)
        {
            RestartGame();
            return;
        }
        ActiveInput(true);
        MainModel.ResetSavePoint();
        updateHealthEvent?.Invoke(MainModel.gameInfo.health);
        if (MainModel.gameInfo.savePoint == null)
            DoRevival();
        else
            updateRevivalCameraEvent?.Invoke(MainModel.gameInfo.savePoint.Value);
    }

    public static void BuffHeart()
    {
        buffHeartEvent?.Invoke(MainModel.gameInfo.savePoint.Value);
    }

    public static void DoRevival()
    {
        loadSavePointEvent?.Invoke(MainModel.gameInfo.savePoint);
        STGameController.UpdatePlayerImmunity(true, false);
        STGameController.UpdatePlayerHp(STGameConstant.PLAYER_MAX_HEALTH);
    }

    public static void ResumePlayer()
    {
        resumePlayerEvent?.Invoke();
    }

    public static void StopPlayer(bool isFinish = false)
    {
        stopPlayerEvent?.Invoke(isFinish);
    }

    public static void DoTrigger(string key, bool state, GameObject triggerSource = null)
    {
        triggerEvent?.Invoke(key, state, triggerSource);
    }

    public static void PickupKey(string key)
    {
        MainModel.gameInfo.AddKey(key);
        keyPickupEvent?.Invoke(MainModel.gameInfo.keys.Count);
    }

    public static void ActiveKey(string key, Vector2 position)
    {
        if (!MainModel.gameInfo.HasKey(key))
            return;
        keyActiveEvent?.Invoke(key, position);
    }

    public static void RemoveKey(string key)
    {
        MainModel.gameInfo.RemoveKey(key);
        keyPickupEvent?.Invoke(MainModel.gameInfo.keys.Count);
    }

    public static void LevelReady()
    {
        int maxPoint = GameObject.FindGameObjectsWithTag(GameTag.STAR).Length;
        MainModel.levelResult.maxPoint = maxPoint;
        initUIEvent?.Invoke(maxPoint);
        updatePointEvent?.Invoke(MainModel.levelResult.remainPoint, null, 0);
        levelLoadedEvent?.Invoke();
        MainController.UpdateHeart(0);
        //
        switch(MainModel.gameInfo.playMode)
        {
            case PlayMode.Normal:
                break;
        }
    }

    public static void Ready()
    {
        readyEvent?.Invoke();
        if (MainModel.gameInfo.playMode == PlayMode.Normal && !(PlayerPrefs.GetInt(DataKey.MAP_LEVEL) == 0 && PlayerPrefs.GetInt(DataKey.WORLD_LEVEL) == 0))
            ShowProgressLevel();
    }

    public static void CompleteLevel()
    {
        if (MainModel.levelResult.isComplete)
            return;
        GameInfo info = MainModel.gameInfo;
        ConfigLoader loader = ConfigLoader.instance;
        //apply result
        LevelResult result = MainModel.levelResult;
        result.isComplete = true;
        result.oldCoin = MainModel.totalCoin;
        result.coin = info.levelCoin;
        result.mapLevel = info.level;
        result.worldLevel = info.world;
        //apply reward        
        MainModel.UpdateTotalCoin((int)(info.levelCoin * MapConstant.COIN_RATIO * (MainModel.hasBonusLevelCoin ? 1.3f : 1)));
        switch(info.playMode)
        {
            case PlayMode.Normal:
                MainModel.UpdateTotalStar(result.point);
                loader.SaveMapLevel(info.level, info.world, result.point, info.playMode);
                int levelBackToMain = 3;
                if(ConfigLoader.IsBossLevel(info.levelPath))
                    MainModel.UpdateBossCollection(ConfigLoader.GetBossName(info.levelPath));
                if (!result.isReplay && result.fakeLevelIndex < levelBackToMain)
                {
                    MainModel.InitGameInfo(loader.mapLevel, loader.worldLevel, info.playMode);
                    Quit(QuitGameReason.Restart);
                }
                else
                    Quit(QuitGameReason.Win);
                //
                break;
            case PlayMode.Boss:
                Quit(QuitGameReason.Win);
                break;
        }
    }

    public static void FailLevel()
    {
        LevelResult result = MainModel.levelResult;
        result.isComplete = false;
        result.oldCoin = MainModel.totalCoin;
        result.coin = 0;
        result.mapLevel = MainModel.gameInfo.level;
        result.worldLevel = MainModel.gameInfo.world;
    }

    public static void QuitLevel()
    {
        MainModel.levelResult = null;
        Quit(QuitGameReason.Back, true);
    }

    public static void ZoomCamera(float ratio, float time)
    {
        zoomCameraEvent?.Invoke(ratio, time);
    }

    public static void ActiveMagnet()
    {
        activeMagnetEvent?.Invoke();
    }

    public static void ActiveBoostItem(object item)
    {
        activeBoostItemEvent?.Invoke(item);
        if (item is BoostHealthItem)
            MainModel.gameInfo.health *= (item as BoostHealthItem).ratio;
    }

    public static void UseBoostItem(object item)
    {
        useBoostItemEvent?.Invoke(item);
    }

    public static void ReadyPlay()
    {
        readyPlayEvent?.Invoke();
    }

    public static void ShakeCamera()
    {
        shakeCameraEvent?.Invoke();
    }
    public static void VibrateCamera(float force)
    {
        vibrateCameraEvent?.Invoke(force);
    }
    public static void VibrateCameraY()
    {
        vibrateCameraYEvent?.Invoke();
    }

    public static void VibrateCustom(Vector3 vibration, float duration)
    {
        vibrateCustomEvent?.Invoke(vibration, duration);
    }

    public static void ShakeCameraWeak()
    {
        shakeCameraWeakEvent?.Invoke();
    }

    public static void ShakeCameraLoop(float time)
    {
        shakeCameraLoopEvent?.Invoke(time);
    }

    public static void UpdateBlendTime(float time)
    {
        updateBlendTimeEvent?.Invoke(time);
    }

    public static void BoostItem()
    {
        boostItemEvent?.Invoke();
    }

    public static void UpdateKeyTreasure(int value, Vector3 itemPos, int direction)
    {
        MainModel.gameInfo.treasureKeys += value;
        updateTreasureKeyEvent?.Invoke(MainModel.gameInfo.treasureKeys, itemPos, direction);
    }

    public static void PlayerWin()
    {
        playerWinEvent?.Invoke();
    }

    public static void Quit(QuitGameReason reason, bool force = false)
    {
        quitEvent?.Invoke(reason, force);
    }

    public static void SkipLevel()
    {
        GameInfo info = MainModel.gameInfo;
        ConfigLoader.instance.SaveMapLevel(info.level, info.world, 0, info.playMode);
        if(ConfigLoader.IsBossLevel(info.levelPath))
            MainModel.UpdateBossCollection(ConfigLoader.GetBossName(info.levelPath));
        MainModel.InitGameInfo(ConfigLoader.instance.mapLevel, ConfigLoader.instance.worldLevel, info.playMode);
        Quit(QuitGameReason.Skip, true);
    }

    public static void UpdateHeart(int heart, Vector3? itemPos)
    {
        MainModel.UpdateHeart(heart);
        updateHeartEvent?.Invoke(MainModel.totalHeart, itemPos);
    }

    public static void ShowProgressLevel()
    {
        showProgressLevelEvent?.Invoke();
    }

    public static void ActiveInput(bool active)
    {
        activeInputEvent?.Invoke(active);
    }

    public static void BossReady()
    {
        ActiveInput(true);
        bossReadyEvent?.Invoke();
    }

    public static void BossAppear()
    {
        ActiveInput(false);
        bossAppearEvent?.Invoke();
    }

    public static void UpdateTargetYCamera(float y)
    {
        updateTargetYCameraEvent?.Invoke(y);
    }
    
    public static void GetCoinAdsIngame(int value, Vector3? objAdsPos)
    {
        getCoinAdsEvent?.Invoke(value, objAdsPos);
    }
    public static void Teleport(Vector3 destination)
    {
        teleportEvent?.Invoke(destination);
    }
    public static void MaskTeleClosed(Vector3 destination)
    {
        maskTeleClosedEvent?.Invoke(destination);
    }
    public static void SkipCutscene()
    {
        skipCutsceneEvent?.Invoke();
    }
    public static void MaskSkipCutsceneClosed()
    {
        maskSkipCutsceneClosedEvent?.Invoke();
    }
    public static void EatItem()
    {
        eatItemEvent?.Invoke();
    }
    public static void UpdateWeather(Weather weather)
    {
        updateWeatherEvent?.Invoke(weather);
    }
    public static void ActivateUI(bool toActive)
    {
        activateUIGameSceneEvent?.Invoke(toActive);
    }

    public static void ChangeBackground(Season targetSeason, float fadeTime)
    {
        changeBackgroundEvent?.Invoke(targetSeason, fadeTime);
    }

    public static void ChangeCameraOffset(ObjectChangeCameraOffset objectChangeCameraOffset, bool change)
    {
        changeCameraOffsetEvent?.Invoke(objectChangeCameraOffset, change);
    }

    public static void UpdatePointMonster(int point)
    {
        updateScoreEvent?.Invoke();
    }

    public static void UpdatePointStars(int star)
    {
        activeTextEffectScoreEvent?.Invoke();
        updateScoreEvent?.Invoke();
    }

    public static void MonsterDead(GameObject monster)
    {
        monsterDeadEvent?.Invoke(monster);
    }

    public static void ObjectDestroyed(GameObject obj)
    {
        objectDestroyedEvent?.Invoke(obj);
    }

    public static void KeepSkin(string skin)
    {
        MainModel.levelResult.skin = skin;
        MainModel.SetAvailableSkinToCollect(skin);
    }

    public static void KeepWeapon(string weaponSkinName)
    {
        MainModel.levelResult.weapon = weaponSkinName;
        MainModel.SetAvailableWeaponToCollect(weaponSkinName);
    }

    public static void ShowCollectWeaponEffect(string skin, string weapon)
    {
        showCollectWeaponEffectEvent?.Invoke(skin, weapon);
    }

    public static void CollectWeapon(string weaponSkinName)
    {
        MainModel.BuyWeapon(weaponSkinName);
        MainModel.SetCurrentWeapon(weaponSkinName);
        MainController.EquipWeapon(ConfigLoader.instance.config.GetWeaponBySkinName(weaponSkinName).weaponName);
    }

    public static void ActivateInputTutorial(InputTutorialType inputTutorialType, InputTutorialTarget inputTutorialTarget, bool toActive)
    {
        activateInputTutorialEvent?.Invoke(inputTutorialType, inputTutorialTarget, toActive);
    }
}
