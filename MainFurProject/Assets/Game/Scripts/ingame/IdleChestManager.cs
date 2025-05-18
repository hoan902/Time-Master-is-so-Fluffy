using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System;

public class IdleChestManager : MonoBehaviour
{
    [SerializeField] SkeletonGraphic m_chestSpine;
    [SerializeField] SkeletonGraphic m_skinRewardSpine;
    [SerializeField] int[] m_chestConditions; // minutes
    [SerializeField] IdleChestReward[] m_chestReward; // 0 = skin, 1 = coin, 2 = hearts
    [SerializeField] Vector2 m_chestStartPos;
    [SerializeField] Vector2 m_chestEndPos;
    [SerializeField] Ease m_moveChestEase = Ease.Linear;
    [SerializeField] TextMeshProUGUI m_timerText;
    [SerializeField] Animator m_animator;
    [SerializeField] RectTransform m_avatar;
    [SerializeField] RectTransform m_itemHeart;
    [SerializeField] GameObject m_coinHeartDisplay;
    [SerializeField] Sprite[] m_rewardIcons;
    [SerializeField] Button m_button;

    [SerializeField] private Transform m_coinLevelStartPos;
    [SerializeField] private RectTransform m_coinCompleteHolder;
    [SerializeField] Transform m_coinCompleteFinishPos;
    [SerializeField] AudioClip m_audioCollectCoin;
    [SerializeField] AudioClip m_audioRainCoin;

    private int m_defaultDuration;
    private RectTransform m_RT;
    private Tween m_moveChestTween;
    private Tween m_fadeChestTween;
    private int m_currentChestIndex;
    private List<GameObject> m_coins;
    private GameObject m_soundRainCoin;
    private string m_skinReward;
    private RectTransform m_skinEff;
    private TextMeshProUGUI m_coinText;
    private Image m_coinHeartIcon;
    private bool m_claiming;
    private bool m_inCutscene;
    private bool m_onRightSide;

    public bool Claiming{get => m_claiming;}

    private void Awake() 
    {
        m_inCutscene = false;
        m_defaultDuration = m_chestConditions[0];    
        m_RT = GetComponent<RectTransform>();
        m_skinEff = m_skinRewardSpine.transform.parent.GetComponent<RectTransform>();
        m_currentChestIndex = PlayerPrefs.GetInt(DataKey.IDLE_CHEST_INDEX, 0);
        m_coinText = m_coinHeartDisplay.transform.Find("text").GetComponent<TextMeshProUGUI>();
        m_coinHeartIcon = m_coinHeartDisplay.transform.Find("icon").GetComponent<Image>();

        m_chestSpine.AnimationState.Complete += OnAnimComplete;
    }
    private void Start() 
    {
        StartCoroutine(IScheduleChest());
        m_chestSpine.SetSkin((m_currentChestIndex + 1).ToString());

        m_onRightSide = false;
        m_RT.anchorMin = m_onRightSide ? new Vector2(1, 1) : new Vector2(0, 1);
        m_RT.anchorMax = m_onRightSide ? new Vector2(1, 1) : new Vector2(0, 1);
        m_RT.anchoredPosition = new Vector2(m_onRightSide ? -130 : 130, m_onRightSide ? -280 : -320);
        m_coinCompleteHolder.anchorMin = m_onRightSide ? new Vector2(1, 1) : new Vector2(0, 1);
        m_coinCompleteHolder.anchorMax = m_onRightSide ? new Vector2(1, 1) : new Vector2(0, 1);
        m_coinCompleteHolder.anchoredPosition = new Vector2(m_onRightSide ? -130 : 130, m_onRightSide ? -280 : -320);

        CutSceneController.transitionEvent += OnCutScene;
        GameController.activateUIGameSceneEvent += OnActive;
    }
    private void OnDestroy() 
    {
        m_chestSpine.AnimationState.Complete -= OnAnimComplete;
        CutSceneController.transitionEvent -= OnCutScene;
        GameController.activateUIGameSceneEvent -= OnActive;

        if(m_soundRainCoin != null)
            Destroy(m_soundRainCoin);
        
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        string animName = trackEntry.Animation.Name;
        if(animName == "open")
        {
            OpenChest();
            StartCoroutine(DelayHideChest());
        }
    }
    void OnCutScene(bool isFull, float duration, bool finish)
    {
        if(!finish)
            m_inCutscene = true;
    }
    void OnActive(bool active)
    {
        if(m_inCutscene)
        {
            ShowChest(m_currentChestIndex);
            m_inCutscene = false;
        }
    }

    void ShowChest(int chestIndex)
    {
        m_chestSpine.SetSkin((m_currentChestIndex + 1).ToString());
        StartCoroutine(IScheduleChest());

        string animToPlay = "idle-chest-show";
        if(!m_onRightSide)
            animToPlay = "idle-chest-show-left";
        m_animator.enabled = true;
        m_animator.Play(animToPlay);
    }
    void MoveChestToCenter()
    {
        m_RT.DOLocalMove(m_coinCompleteHolder.localPosition, 2f).SetUpdate(true).SetEase(m_moveChestEase).OnComplete(() => {
            m_chestSpine.AnimationState.SetAnimation(0, "open", false);
        });
        transform.DOScale(new Vector3(2, 2, 1), 2f);
    }
    void HideChest(int chestIndex)
    {


        string animToPlay = "idle-chest-hide";
        if(!m_onRightSide)
            animToPlay = "idle-chest-hide-left";
        m_animator.enabled = true;
        m_animator.Play(animToPlay);
        m_coinHeartDisplay.SetActive(false);
    }
    IEnumerator DelayHideChest()
    {
        yield return new WaitForSeconds(2f);
        HideChest(m_currentChestIndex);
    }
    IEnumerator IScheduleChest()
    {
        m_timerText.gameObject.SetActive(true);
        m_chestSpine.AnimationState.SetAnimation(0, "idle", true);
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime now = DateTime.UtcNow;
        double time = (now - startTime).TotalSeconds;
        double timeStart =  double.Parse(PlayerPrefs.GetString(DataKey.CHEST_START_TIME, time.ToString()));
        double count = double.Parse(PlayerPrefs.GetString(DataKey.CHEST_COUNTDOWN_TIME, (m_chestConditions[m_currentChestIndex]).ToString()));
        PlayerPrefs.SetString(DataKey.CHEST_START_TIME, timeStart.ToString());
        PlayerPrefs.SetString(DataKey.CHEST_COUNTDOWN_TIME, count.ToString());
        PlayerPrefs.Save();
        //
        count -= (now - startTime.AddSeconds(timeStart)).TotalSeconds;
        while(count > 0)
        {
            TimeSpan t = TimeSpan.FromSeconds(count);
            m_timerText.text = t.ToString(@"mm\:ss");
            yield return new WaitForSeconds(1f);
            count--;
        }
        m_button.interactable = true;
        m_chestSpine.AnimationState.SetAnimation(0, "ready", true);
        m_timerText.text = "Ready";
    }
    void OpenChest()
    {
        IdleChestReward reward = m_chestReward[m_currentChestIndex];
        switch(reward.rewardType)
        {
            case IdleRewardType.Heart:
                CreateHeart(reward.value);
                ShowCoinHeartDisplay(reward.value, 0);
                break;
            case IdleRewardType.Coin:
                ShowCoinHeartDisplay(reward.value, 1);
                StartCoroutine(CreateCompleteCoin(reward.value));
                break;
        }
    }
    void OpenChestComplete()
    {
        PlayerPrefs.DeleteKey(DataKey.CHEST_START_TIME);
        PlayerPrefs.DeleteKey(DataKey.CHEST_COUNTDOWN_TIME);
        m_currentChestIndex += 1;
        if(m_currentChestIndex >= m_chestConditions.Length)
            m_currentChestIndex = 0;
        PlayerPrefs.SetInt(DataKey.IDLE_CHEST_INDEX, m_currentChestIndex);
    }
    void ShowCoinHeartDisplay(int value, int iconIndex)
    {
        m_coinHeartDisplay.SetActive(true);
        m_coinHeartDisplay.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        m_coinText.text = "+" + value;
        m_coinHeartIcon.sprite = m_rewardIcons[iconIndex];
        m_coinHeartIcon.SetNativeSize();
        m_coinHeartDisplay.transform.localScale = Vector3.zero;
        m_coinHeartDisplay.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        m_coinHeartDisplay.GetComponent<RectTransform>().DOAnchorPosY(100, 0.5f);
    }
    bool HasChestReward()
    {
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime now = DateTime.UtcNow;
        double time = (now - startTime).TotalSeconds;
        double timeStart =  double.Parse(PlayerPrefs.GetString(DataKey.CHEST_START_TIME, time.ToString()));
        double count = double.Parse(PlayerPrefs.GetString(DataKey.CHEST_COUNTDOWN_TIME, (m_chestConditions[m_currentChestIndex]).ToString()));
        count -= (now - startTime.AddSeconds(timeStart)).TotalSeconds;
        return count < 0;
    }
    IEnumerator CoinMoveFinish(int coin)
    {
        for(int i = 0; i < m_coins.Count; i++)
        {            
            GameObject go = m_coins[i];
            Vector3 pos1 =  go.transform.position;
            Vector3 pos2 = pos1 + new Vector3(-1,1);
            Vector3 pos3 = m_coinCompleteFinishPos.position;
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
        yield return new WaitForSeconds(0.1f);
        MainModel.UpdateTotalCoin(coin);
        OpenChestComplete();
    }
    public IEnumerator CreateCompleteCoin(int count)
    {
        if(count == 0)
        {
            yield break;
        }
        m_soundRainCoin = SoundManager.PlaySound(m_audioRainCoin, true);
        m_coins = new List<GameObject>();
        int rewardValue = count;
        int counter = 0;
        count = count > 10 ? 10 : count;
        GameObject coinObj = m_coinCompleteHolder.Find("coin").gameObject;
        for(int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(coinObj);
            m_coins.Add(go);
            go.SetActive(true);
            go.transform.SetParent(m_coinCompleteHolder, false);
            go.transform.position = m_coinLevelStartPos.position;
            float x = UnityEngine.Random.Range(m_coinCompleteHolder.sizeDelta.x/2 * (m_onRightSide ? -1 : 1),  0);
            float y = UnityEngine.Random.Range(-m_coinCompleteHolder.sizeDelta.y/2,  m_coinCompleteHolder.sizeDelta.y/2);
            Vector2 pos = new Vector2(x, y);
            go.transform.DOLocalMove(pos, 0.1f).OnComplete(()=>{
                counter++;
                if(counter >= count)
                {
                    if(m_soundRainCoin != null)
                        Destroy(m_soundRainCoin);
                    StartCoroutine(CoinMoveFinish(rewardValue));
                }
            });
            yield return new WaitForSeconds(0.05f);
        }
    }
    private void CreateHeart(int value)
    {
        RectTransform item = m_itemHeart;
        item.anchoredPosition = Vector2.zero;
        item.localScale = Vector3.zero;
        item.gameObject.SetActive(true);    
        item.GetComponent<Image>().color = Color.white;    
        item.DOScale(Vector3.one, 0.1f).OnComplete(()=>{
        item.DOMove(m_avatar.position, 0.8f).OnComplete(()=>{                    
                item.DOScale(new Vector3(0,0,1), 0.5f);
                item.GetComponent<Image>().DOFade(0, 0.5f).OnComplete(()=>{
                    item.gameObject.SetActive(false);
                    MainModel.UpdateHeart(value);
                    GameController.updateHeartEvent?.Invoke(MainModel.totalHeart, null);
                    OpenChestComplete();
                }); 
            }).SetEase(Ease.InCubic);
        });
    }

    public void OnChestClick()
    {
        if(!HasChestReward())
            return;
        m_claiming = true;
        m_button.interactable = false;
        m_timerText.gameObject.SetActive(false);
        m_chestSpine.AnimationState.SetAnimation(0, "open", false);
    }
    public void HideChestComplete()
    {
        m_claiming = false;
        if(m_inCutscene)
        {
            gameObject.SetActive(false);
        }
        else  
            ShowChest(m_currentChestIndex);
    }
}

[System.Serializable]
public class IdleChestReward
{
    public IdleRewardType rewardType;
    public int value;
}
public enum IdleRewardType
{
    Coin,
    Heart,
    Skin
}
