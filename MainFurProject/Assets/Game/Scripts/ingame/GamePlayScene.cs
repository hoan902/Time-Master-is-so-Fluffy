using DG.Tweening;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayScene : MonoBehaviour
{
    public static Vector3 keyPosition
    {
        get
        {
            Vector2 viewportPoint = m_camera.WorldToViewportPoint(m_keyPosition);
            return Camera.main.ViewportToWorldPoint(viewportPoint);
        }
    }
    public static Quaternion keyRotation;
    private static Camera m_camera;
    private static Vector3 m_keyPosition;

    [SerializeField] private Image m_avatar;
    [SerializeField] private Slider m_bossPoint;
    [SerializeField] private Transform m_progresHp;
    [SerializeField] private Transform m_pointObject;
    [SerializeField] private Image m_bossTarget;
    [SerializeField] private Transform m_keyTarget;
    [SerializeField] private SkeletonGraphic m_effectKey;
    [SerializeField] private TextMeshProUGUI m_textKey;
    [SerializeField] private TextMeshProUGUI m_textHeart;
    [SerializeField] private TextMeshProUGUI m_levelCoinText;
    [SerializeField] private CanvasGroup m_objectTreasureKeys;
    [SerializeField] private Sprite[] m_spriteKeys;
    [SerializeField] private Sprite[] m_spriteStars;//0 = on, 1 = off
    [SerializeField] private Material m_materialGray;
    [SerializeField] private GameObject[] m_hiddenCutSceneUI;
    [SerializeField] private GameObject m_buttonIdleChest;
    [SerializeField] private GameObject m_buttonPause;
    [SerializeField] private RectTransform m_coinCompleteHolder;
    [SerializeField] private GameObject m_inputController;
    [SerializeField] private QuestListUI m_questListUI;
    [SerializeField] private GameObject m_liveNormalPanel;
    [SerializeField] private GameObject m_liveStickPanel;

    // stick mode
    [SerializeField] private Image m_healthBar;

    [Header("Audio")]
    [SerializeField] private AudioClip m_musicNormal;//music-game-2
    [SerializeField] private AudioClip m_musicBoss;//music_boss
    [SerializeField] private AudioClip m_musicWinBoss;
    [SerializeField] private AudioClip m_musicBigMike;
    [SerializeField] private AudioClip m_audioCollectCoin;
    [SerializeField] private AudioClip m_audioRainCoin;

    private int m_keyQuantity;
    private int m_maxPoint;
    private bool m_isBossLevel;
    private bool m_canShowScoreUI;
    private GameObject m_soundTiktokObject;
    private List<GameObject> m_coins;
    private GameObject m_soundRainCoin;
    private GameObject m_musicBossObj;
    private int m_currentCoinDisplay;
    private Tween m_coinTextTween;
    private QuestList m_questList;
    private bool m_stop;
    private Coroutine m_createCoinRoutine;
    private STObjectBoss[] m_bosses;

    // only for stick mode
    private Tweener m_stickHPTweener;
    private STObjectBoss m_bossStick;
    
    //Temporate
    int m_levelShowBtnPause = 2;

    public bool IsBossLevel{get => m_isBossLevel;}

    private void Awake()
    {        
        GameController.activateUIGameSceneEvent += ActivateUI;
        GameController.initUIEvent += InitUI;
        GameController.updatePointEvent += OnUpdatePoint;
        GameController.updateHealthEvent += OnUpdateHealth;
        GameController.levelLoadedEvent += OnReady;
        GameController.updateTreasureKeyEvent += OnUpdateTreasureKey;
        GameController.buffHeartEvent += OnBuffHeart;
        GameController.quitEvent += OnShowHome;
        GameController.updateHeartEvent += OnUpdateHeart;
        GameController.getCoinAdsEvent += OnUpdateCoin;
        GameController.updateBossHpEvent += OnUpdateBossHealth;
        GameController.bossAppearEvent += OnBossAppear;
        GameController.updateLevelCoinEvent += OnUpdateLevelCoin;

        MainController.readySceneEvent += OnSceneReady;

        CutSceneController.transitionEvent += OnCutScene;

        STGameController.updatePlayerHpEvent += OnUpdateStickHP;
        GameController.changeSkinEvent += OnChangeSkin;
    }

    void Start()
    {
        m_effectKey.AnimationState.Complete += ActiveKey;
        //
        m_keyQuantity = 0;
        //
        GameController.Init();
        //  
        keyRotation = m_keyTarget.rotation;
        m_keyPosition = m_keyTarget.position;
        m_camera = transform.root.GetComponent<Canvas>().rootCanvas.worldCamera;
        //
        PlayMode playMode = MainModel.gameInfo.playMode;
        m_pointObject.gameObject.SetActive(playMode is PlayMode.Normal);
        //
        m_buttonIdleChest.SetActive(ConfigLoader.instance.fakeMapLevel >= m_levelShowBtnPause);
        m_buttonPause.SetActive(ConfigLoader.instance.fakeMapLevel >= m_levelShowBtnPause);
        //
        m_currentCoinDisplay = 0;
        m_levelCoinText.text = m_currentCoinDisplay.ToString();
        UpdateDefaultStarSprites();

        // active stick or normal lives panel
        m_liveNormalPanel.SetActive(false);
        m_liveStickPanel.SetActive(true);
        if(true)
        {
            Transform stickPanel = m_liveStickPanel.transform;
            m_avatar = stickPanel.Find("avatar").GetComponent<Image>();
            m_pointObject = stickPanel.Find("points");
            m_levelCoinText = stickPanel.Find("level-coin").Find("level-coin-text").GetComponent<TextMeshProUGUI>();
            m_textHeart = stickPanel.Find("heart-display").Find("lb-heart-value").GetComponent<TextMeshProUGUI>();
            m_textHeart.text = MainModel.totalHeart.ToString();
        }
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))   
        {
            GameController.Finish(Vector3.zero, Vector3.zero);
        }
    }
#endif

    void OnDestroy()
    {
        GameController.activateUIGameSceneEvent -= ActivateUI;
        GameController.initUIEvent -= InitUI;
        GameController.updatePointEvent -= OnUpdatePoint;
        GameController.updateHealthEvent -= OnUpdateHealth;
        GameController.levelLoadedEvent -= OnReady;
        GameController.updateTreasureKeyEvent -= OnUpdateTreasureKey;
        GameController.buffHeartEvent -= OnBuffHeart;
        GameController.quitEvent -= OnShowHome;
        GameController.updateHeartEvent -= OnUpdateHeart;
        GameController.getCoinAdsEvent -= OnUpdateCoin;
        GameController.updateBossHpEvent -= OnUpdateBossHealth;
        GameController.bossAppearEvent -= OnBossAppear;
        GameController.updateLevelCoinEvent -= OnUpdateLevelCoin;

        MainController.readySceneEvent -= OnSceneReady;

        CutSceneController.transitionEvent -= OnCutScene;

        STGameController.updatePlayerHpEvent -= OnUpdateStickHP;
        GameController.changeSkinEvent -= OnChangeSkin;
    }

    void OnUpdateLevelCoin()
    {
        m_coinTextTween?.Kill();
        int targetValue = (int)(MainModel.gameInfo.levelCoin*MapConstant.COIN_RATIO * (MainModel.hasBonusLevelCoin ? 1.3f : 1));
        int tempValue = m_currentCoinDisplay;
        DOTween.To(() => tempValue, x => tempValue = x, targetValue, 0.5f).OnUpdate(() => {
            m_currentCoinDisplay = tempValue;
            m_levelCoinText.text = m_currentCoinDisplay + "";
        });
    }

    private void OnCutScene(bool isFull, float duration, bool finish)
    {
        if(finish)
            return;
        foreach(GameObject go in m_hiddenCutSceneUI)
        {
            if(go == m_buttonPause)
            {
                go.SetActive(ConfigLoader.instance.fakeMapLevel >= m_levelShowBtnPause && finish);
                continue;
            }
            go.SetActive(finish);
        }
    }
    private void ActivateUI(bool toActive)
    {
        foreach(GameObject go in m_hiddenCutSceneUI)
        {
            if(go == m_liveNormalPanel)
                continue;
            if(go == m_buttonPause)
            {
                go.SetActive(ConfigLoader.instance.fakeMapLevel >= m_levelShowBtnPause && toActive);
                continue;
            }
            else if(go == m_bossPoint.transform.parent.gameObject)
            {
                if(m_bossPoint.value < 0.1f && toActive)
                {
                    m_bossPoint.GetComponent<RectTransform>().DOAnchorPosY(200, 0.5f).SetDelay(0.5f);
                    continue;
                }
            }
            go.SetActive(toActive);
        }
    }

    private void OnBossAppear()
    {
        AudioClip musicToPlay = ConfigLoader.instance.GetBossMusicByMode(MainModel.gameInfo.world, MainModel.gameInfo.level, m_musicBoss, MainModel.gameInfo.playMode);
        SoundManager.PlaySound(musicToPlay, true, true);
        RectTransform rect = m_bossPoint.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, 200);
        rect.DOAnchorPosY(0, 0.5f);
    }

    private void OnUpdateBossHealth(int max, int hp)
    {
        int maxHP = 0;
        int currentHP = 0;

        foreach (STObjectBoss boss in m_bosses)
        {
            maxHP += (int)boss.maxHP;
            currentHP += (int)boss.currentHP;
        }

        m_bossPoint.DOValue(1f*currentHP / maxHP, 0.5f).SetUpdate(true).OnComplete(()=> { 
            if(currentHP < 1)
            {
                m_bossPoint.GetComponent<RectTransform>().DOAnchorPosY(200, 0.5f).SetDelay(0.5f);
            }
        });
        if(currentHP < 1)
        {
            if(m_bossTarget.sprite != null)
            {
                string bossName = m_bossTarget.sprite.name;
                m_musicBossObj = SoundManager.PlaySound(m_musicWinBoss, false, true);
                StartCoroutine(DelayPlayNormalMusic());
            }
        }
    }
    IEnumerator DelayPlayNormalMusic()
    {
        yield return new WaitForSeconds(4.5f);
        SoundManager.PlaySound(m_musicNormal, true, true);
    }

    private void OnSceneReady(SceneType sceneType)
    {
        if (sceneType != SceneType.Game)
            return;
        GameController.Ready();
    }

    private void OnShowHome(QuitGameReason reason, bool force)
    {
        if(force || !m_stop)
            SceneOut(reason);
        m_stop = true;
    }

    private void OnBuffHeart(Vector2 savePoint)
    {        
        StartCoroutine(BuffHeart(savePoint, m_avatar.transform.position));
    }

    //void FadeAdsTokenDisplay(float alpha, float duration, float delayTime)
    //{
    //    Image frame = m_adsTokenText.transform.parent.GetComponent<Image>();
    //    Image tokenIcon = frame.transform.Find("token-icon").GetComponent<Image>();

    //    frame?.DOKill();
    //    tokenIcon?.DOKill();
    //    m_adsTokenText?.DOKill();

    //    frame.DOFade(alpha, duration).SetDelay(delayTime);
    //    tokenIcon.DOFade(alpha, duration).SetDelay(delayTime);
    //    m_adsTokenText.DOFade(alpha, duration).SetDelay(delayTime);
    //}

    IEnumerator BuffHeart(Vector2 savePoint, Vector2 startPoint)
    {
        yield return new WaitForSeconds(1f);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(savePoint);
        Vector3 worldPos = m_camera.ScreenToWorldPoint(screenPos);
        //
        Transform item = m_avatar.transform.parent.Find("item-heart");
        Vector3 targetPos = m_avatar.transform.parent.InverseTransformPoint(worldPos);
        item.position = startPoint;
        item.localScale = Vector3.zero;
        
        item.gameObject.SetActive(true);  
        Image img = item.GetComponent<Image>();
        Color color = img.color;
        color.a = 0;
        img.color = color;
        item.DOScale(Vector3.one, 0.5f);
        img.DOFade(1, 0.5f).OnComplete(()=>{
            item.DOLocalMove(targetPos, 0.8f).SetEase(Ease.InCubic).OnComplete(()=>{
                item.gameObject.SetActive(false);
                GameController.DoRevival();
                m_textHeart.text = MainModel.totalHeart.ToString();
            });
        });
    }

    private void OnUpdateTreasureKey(int value, Vector3? startPos, int direction)
    {
        if (startPos != null)
        {
            m_objectTreasureKeys.DOKill();
            m_objectTreasureKeys.DOFade(1, 0.5f);
        }
        //
        Transform last = null;
        Transform effect = m_objectTreasureKeys.transform.Find("effect");
        Transform keys = m_objectTreasureKeys.transform.Find("keys");
        for(int i = 0; i < keys.childCount; i++)
        {
            Transform t = keys.GetChild(i);
            if(i < value)
            {
                t.GetComponent<Image>().sprite = m_spriteKeys[0];
                t.GetComponent<Image>().SetNativeSize();
                last = t;
            }else
            {
                t.GetComponent<Image>().sprite = m_spriteKeys[1];
                t.GetComponent<Image>().SetNativeSize();
            }
        }
        if(last != null)
        {
            if(startPos != null)
            {
                Camera cam = GetComponent<RectTransform>().root.GetComponent<Canvas>().rootCanvas.worldCamera;
                Vector2 viewportPoint = Camera.main.WorldToViewportPoint(startPos.Value);
                Vector2 pos = cam.ViewportToWorldPoint(viewportPoint);

                last.GetComponent<Image>().sprite = m_spriteKeys[1];
                last.GetComponent<Image>().SetNativeSize();
                Transform keyTemp = m_objectTreasureKeys.transform.Find("key-temp");
                keyTemp.position = pos;

                Vector3 pos1 = keyTemp.localPosition;                
                Vector3 pos2 = pos1 + new Vector3(direction*100, 100);
                keyTemp.position = last.position;
                Vector3 pos3 = keyTemp.localPosition;
                Vector3[] path = new Vector3[]{pos1, pos2, pos3};                
                keyTemp.localPosition = pos1;               
                keyTemp.gameObject.SetActive(true);
                keyTemp.localScale = Vector3.zero;
                keyTemp.DOKill();
                keyTemp.DOScale(Vector3.one, 0.1f).OnComplete(()=>{
                    keyTemp.DOLocalPath(path, 0.8f, PathType.CatmullRom).OnComplete(()=>{
                        last.GetComponent<Image>().sprite = m_spriteKeys[0];
                        last.GetComponent<Image>().SetNativeSize();
                        keyTemp.DOScale(new Vector3(2,2,1), 0.5f);
                        keyTemp.GetComponent<Image>().DOFade(0, 0.5f).OnComplete(()=>{
                            keyTemp.gameObject.SetActive(false);
                            //
                            m_objectTreasureKeys.DOKill();
                            m_objectTreasureKeys.DOFade(0, 0.5f).SetDelay(5f);
                        });                      
                    }).SetEase(Ease.InCubic);
                });
            }
        }
    }

    private void OnUpdateHeart(int value, Vector3? itemPos)
    {
        if(itemPos == null)
            m_textHeart.text = value.ToString();
        else
        {
            Camera cam = GetComponent<RectTransform>().root.GetComponent<Canvas>().rootCanvas.worldCamera;
            Vector2 viewportPoint = Camera.main.WorldToViewportPoint(itemPos.Value);
            Vector2 pos = cam.ViewportToWorldPoint(viewportPoint);

            Transform item = m_avatar.transform.parent.Find("item-heart");
            item.position = pos;
            item.localScale = Vector3.zero;
            item.gameObject.SetActive(true);    
            item.GetComponent<Image>().color = Color.white;    
            item.DOScale(Vector3.one, 0.1f).OnComplete(()=>{
            item.DOLocalMove(m_avatar.transform.localPosition, 0.8f).OnComplete(()=>{                    
                    item.DOScale(new Vector3(0,0,1), 0.5f);
                    item.GetComponent<Image>().DOFade(0, 0.5f).OnComplete(()=>{
                        m_textHeart.text = value.ToString();
                        item.gameObject.SetActive(false);
                    }); 
                }).SetEase(Ease.InCubic);
            });
        }
    }

    private void OnUpdateCoin(int value, Vector3? objAdsPos)
    {
        Camera cam = GetComponent<RectTransform>().root.GetComponent<Canvas>().rootCanvas.worldCamera;
        Vector2 viewportPoint = Camera.main.WorldToViewportPoint(objAdsPos.Value);
        Vector2 pos = cam.ViewportToWorldPoint(viewportPoint);
        if(m_createCoinRoutine != null)
            return;
        m_createCoinRoutine = StartCoroutine(CreateCoins(value, pos));
    }
    IEnumerator CreateCoins(int value, Vector3? coinStartPos)
    {
        if(m_soundRainCoin != null)
            Destroy(m_soundRainCoin);
        m_soundRainCoin = SoundManager.PlaySound(m_audioRainCoin, true);
        m_coins = new List<GameObject>();
        int counter = 0;
        value = value > 10 ? 10 : value;
        GameObject item = m_avatar.transform.parent.Find("item-coin").gameObject;
        for(int i = 0; i < value; i++)
        {
            GameObject go = Instantiate(item);
            go.transform.SetParent(m_avatar.transform.parent);
            go.transform.position = coinStartPos.Value;
            go.SetActive(true);
            go.transform.localScale = new Vector3(0.5f, 0.5f, 1);
            m_coins.Add(go);
            RectTransform goRT = go.GetComponent<RectTransform>();
            float x = UnityEngine.Random.Range(-m_coinCompleteHolder.sizeDelta.x/2,  m_coinCompleteHolder.sizeDelta.x/2);
            float y = UnityEngine.Random.Range(-m_coinCompleteHolder.sizeDelta.y/2,  m_coinCompleteHolder.sizeDelta.y/2);
            Vector2 pos = new Vector2(goRT.anchoredPosition.x + x, goRT.anchoredPosition.y + y);
            go.transform.DOLocalMove(pos, 0.1f).OnComplete(()=>{
                counter++;
                if(counter >= value)
                {
                    if(m_soundRainCoin != null)
                        Destroy(m_soundRainCoin);
                    StartCoroutine(CoinMoveFinish(value*MapConstant.COIN_RATIO));
                }
            });
            yield return new WaitForSeconds(0.05f);
        }
    }
    IEnumerator CoinMoveFinish(int coin)
    {
        for(int i = 0; i < m_coins.Count; i++)
        {            
            GameObject go = m_coins[i];
            Vector3 pos1 =  go.transform.position;
            Vector3 pos2 = pos1 + new Vector3(-1,1);
            Vector3 pos3 = m_avatar.transform.position;
            Vector3[] path = new Vector3[]{pos1, pos2, pos3};
            go.transform.DOPath(path, 0.8f, PathType.CatmullRom).SetEase(Ease.InCubic).OnComplete(()=>{
                SoundManager.PlaySound(m_audioCollectCoin, false);
                go.transform.DOScale(new Vector3(3,3,1), 0.5f);
                go.GetComponent<Image>().DOFade(0, 0.5f).OnComplete(()=>{
                    Destroy(go);
                }); 
            });
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnReady()
    {
        m_objectTreasureKeys.DOKill();
        m_objectTreasureKeys.alpha = 0;

        m_questList = FindObjectOfType<QuestList>();
        m_questListUI.Init(m_questList);
    }

    void InitUI(int maxPoint)
    {
        ConfigLoader loader = ConfigLoader.instance;
        GameInfo info = MainModel.gameInfo;
        switch (info.playMode)
        {
            case PlayMode.Normal:
                AudioClip music = loader.GetBackgroundMusic(info.world, m_musicNormal);
                if(music != null)
                    m_musicNormal = music;
                SoundManager.PlaySound(m_musicNormal, true, true);
                break;
            case PlayMode.Boss:
                AudioClip musicToPlay = loader.config.bossLevels[info.level].defaultMusic;
                SoundManager.PlaySound(musicToPlay, true, true);
                break;
        }
        LevelConfig levelConfig = loader.GetLevel(info.world, info.level, info.playMode);
        m_isBossLevel = ConfigLoader.IsBossLevel(levelConfig.levelPath);
        if (m_isBossLevel)
        {
            m_bossTarget.sprite = loader.FindBossAvatar(ConfigLoader.GetBossName(levelConfig.levelPath));
            m_bosses = FindObjectsOfType<STObjectBoss>();
        }
        m_bossTarget.SetNativeSize();
        //
        m_maxPoint = maxPoint;
        m_bossPoint.value = 1;
        m_keyTarget.gameObject.SetActive(false);
        m_effectKey.gameObject.SetActive(false);
        m_textKey.gameObject.SetActive(false);
        m_avatar.sprite = loader.config.GetPlayerAvatar(MainModel.CurrentSkin);
        m_avatar.SetNativeSize();
        //
        GameController.ReadyPlay();
    }

    private void OnUpdatePoint(int remainPoint, Vector3? startPos, int direction)
    {
        if (startPos == null)
            UpdatePointProgres(remainPoint);
        else
        {
            int activeIndex = m_maxPoint - remainPoint;
            if (activeIndex > 0 && activeIndex <= 3) // hard-code
            {
                Camera cam = transform.root.GetComponent<Canvas>().rootCanvas.worldCamera;
                Vector2 viewportPoint = Camera.main.WorldToViewportPoint(startPos.Value);
                Vector2 pos = cam.ViewportToWorldPoint(viewportPoint);

                Transform activePoint = m_pointObject.Find("points").GetChild(activeIndex - 1);

                Transform temp = m_pointObject.Find("point-temp");
                GameObject pointTemp = Instantiate(temp.gameObject);
                pointTemp.GetComponent<Image>().sprite = m_spriteStars[0];
                pointTemp.transform.SetParent(m_pointObject.Find("point-temp-parent"));
                pointTemp.gameObject.SetActive(true);
                pointTemp.GetComponent<Image>().SetNativeSize();
                pointTemp.transform.position = pos;
                pointTemp.transform.localScale = new Vector3(2, 2, 1);

                Vector3 pos1 = pointTemp.transform.localPosition;
                Vector3 pos2 = pos1 + new Vector3(direction * 100, 100);
                Vector3 pos3 = activePoint.localPosition;
                Vector3[] path = new Vector3[] { pos1, pos2, pos3};
                pointTemp.transform.DOScale(Vector3.one, 0.8f);
                pointTemp.transform.DOLocalPath(path, 0.8f, PathType.CatmullRom).OnComplete(() =>
                {
                    UpdatePointProgres(remainPoint);
                    Destroy(pointTemp);
                }).SetEase(Ease.InCubic);
            }
        }
    }

    void UpdatePointProgres(int remainPoint)
    {
        Transform points = m_pointObject.Find("points");
        //Transform effect = m_pointObject.Find("effect");
        int activeIndex = m_maxPoint - remainPoint;        
        if(activeIndex > 0 && activeIndex <= 3)
        {
            Transform p = points.GetChild(activeIndex - 1);
            p.GetComponent<Image>().sprite = m_spriteStars[0];
        }
    }
    void UpdateDefaultStarSprites()
    {
        Transform pointContainer = m_pointObject.Find("points");
        float targetScale = 1.5f;
        foreach(Transform child in pointContainer)
        {
            Image childImage = child.GetComponent<Image>();
            childImage.sprite = m_spriteStars[1];
            childImage.SetNativeSize();
            child.transform.localScale = new Vector3(targetScale, targetScale, 1);
        }
    }

    private void OnUpdateHealth(int point)
    {
        for(int i = 0; i < m_progresHp.childCount; i++)
        {
            if (i < point)
                m_progresHp.GetChild(i).GetComponent<Image>().material = null;
            else
                m_progresHp.GetChild(i).GetComponent<Image>().material = m_materialGray;
        }
    }
    private void OnUpdateStickHP(int last, int current)
    {
        m_stickHPTweener?.Kill();
        float healthFraction = (float)current / (float)STGameConstant.PLAYER_MAX_HEALTH;
        m_healthBar.DOFillAmount(healthFraction, 0.5f);
    }

    private void OnChangeSkin(string skin)
    {
        m_avatar.sprite = ConfigLoader.instance.config.GetPlayerAvatar(skin);
        m_avatar.SetNativeSize();
    }

    void ActiveKey(TrackEntry entry)
    {
        m_effectKey.gameObject.SetActive(false);
        m_keyTarget.gameObject.SetActive(m_keyQuantity > 0);
        m_textKey.gameObject.SetActive(m_keyQuantity > 1);
        m_textKey.text = "X" + m_keyQuantity;
    }

    public void ShowPanelPauseOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.OpenPopup(PopupType.Pause);
    }

    void SceneOut(QuitGameReason reason)
    {
        GameController.StopPlayer();
        MainController.DoSceneTrasition(false, () =>
        {
            switch (reason)
            {
                case QuitGameReason.Back:
                        MainController.OpenScene(SceneType.Home);
                    break;
                case QuitGameReason.Fail:
                        MainController.OpenScene(SceneType.Home);
                    break;
                case QuitGameReason.Win:
                        MainController.OpenScene(SceneType.Home);
                    break;
                case QuitGameReason.Restart:
                        MainController.OpenScene(SceneType.Game);
                    break;
                case QuitGameReason.Skip:
                    MainController.OpenScene(SceneType.Game);
                    break;
            }
        });
    }
}
