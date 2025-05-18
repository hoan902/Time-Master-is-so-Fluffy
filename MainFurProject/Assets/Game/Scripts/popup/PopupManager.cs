
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private GameObject m_mask;
    [SerializeField] private AssetReference m_shop;
    [SerializeField] private AssetReference m_dailyReward;
    [SerializeField] private AssetReference m_subscription;
    [SerializeField] private AssetReference m_removeAds;
    [SerializeField] private AssetReference m_unlockSkin;
    [SerializeField] private AssetReference m_watchAds;
    [SerializeField] private AssetReference m_boostItems;
    [SerializeField] private AssetReference m_pause;
    [SerializeField] private AssetReference m_reviveRedBlue;
    [SerializeField] private AssetReference m_reviveStick;
    [SerializeField] private AssetReference m_update;
    [SerializeField] private AssetReference m_shopHeart;
    [SerializeField] private AssetReference m_shopIngame;
    [SerializeField] private AssetReference m_bossCollection;
    [SerializeField] private AssetReference m_skinComboSelection;

    [SerializeField] private GameObject m_cheat;    
    [SerializeField] private GameObject[] m_cheatButtons; 

    private List<PopupData> m_queue;            
    private List<PopupType> m_prev;
    private Dictionary<PopupType, GameObject> m_popups;

    private int m_cheatCounter;
    private int m_cheatLeaderBoardCounter;

    void Awake()
    {
        m_queue = new List<PopupData>();
        m_prev = new List<PopupType>();
        m_popups = new Dictionary<PopupType, GameObject>();
        //
        MainController.openPopupEvent += OnOpenPopup;
        MainController.closePopupEvent += OnClosePopup;
        MainController.worldChangeEvent += OnWorldChange;
        MainController.openSceneEvent += OnOpenScene;
        //
        m_cheatCounter = -1;
        m_cheatLeaderBoardCounter = -1;
    }

    void OnDestroy()
    {
        MainController.openPopupEvent -= OnOpenPopup;
        MainController.closePopupEvent -= OnClosePopup;
        MainController.worldChangeEvent -= OnWorldChange;
        MainController.openSceneEvent -= OnOpenScene;
    }

    private void OnOpenScene(SceneType type)
    {
        foreach(GameObject btn in m_cheatButtons)
        {
            btn.SetActive(type == SceneType.Home);
        }
    }

    private void OnWorldChange(bool init)
    {
        m_mask.GetComponent<Image>().sprite = ConfigLoader.instance.currentWorld.background;
    }

    private void OnClearAllPrevPopup ()
    {
        m_queue = new List<PopupData>();
        m_prev = new List<PopupType>();
    }

    private void OnClosePopup(PopupType type)
    {    
        ClosePopup(type);
    }

    private void ClosePopup(PopupType type)
    {
        if (!m_popups.ContainsKey(type))
            return;
        GameObject go = m_popups[type];
        m_popups.Remove(type);
        if (go != null)
        {
            GameObject panel = go.transform.Find("panel").gameObject;
            panel.transform.localScale = Vector3.one;
            float time = 0.25f;

            Action cb = () =>
            {
                m_prev.Remove(type);
                Destroy(go);
                if (type == PopupType.WatchAds)
                {
                    if (m_prev.Count < 1 && m_queue.Count < 0)
                    {
                        m_mask.SetActive(false);
                        MainController.FinishPopup();
                    }
                    return;
                }
                if (m_prev.Count < 1)
                {
                    if (m_queue.Count > 0)
                    {
                        PopupData p = m_queue[0];
                        m_queue.RemoveAt(0);
                        OpenPopup(p.pType, true);
                    }
                    else
                    {
                        m_mask.SetActive(false);
                        MainController.FinishPopup();
                    }
                }
            };
            if (Mathf.Approximately(0, time))
                cb();
            else
                panel.transform.DOScale(Vector3.zero, time).SetEase(Ease.InBack).OnComplete(() =>
                {
                    cb();
                }).SetUpdate(true);
        }
    }

    private void OnOpenPopup(PopupType type, bool playAnim)
    {
        m_mask.SetActive(true);
        bool foreShow = type == PopupType.Subscription || type == PopupType.WatchAds;
        if (m_prev.Count > 0 && !foreShow)
        {
            m_queue.Add(new PopupData()
            {
                pType = type
            });
            return;
        }
        OpenPopup(type, playAnim);
    }

    void OpenPopup(PopupType type, bool playAnim)
    {
        m_mask.GetComponent<Image>().color = Color.white;
        if(type == PopupType.ShopIngame || type == PopupType.ReviveStick || type == PopupType.HeartStore || type == PopupType.Shop || type == PopupType.Pause || type == PopupType.DailyReward || type == PopupType.Shop)
        {
            Color tempColor = Color.black;
            tempColor.a = 0.8f;
            m_mask.GetComponent<Image>().color = tempColor;
        }
        // else if(type == PopupType.ReviveStick && MainModel.gameInfo.playMode == PlayMode.Normal)
        // {
        //     Color tempColor = Color.black;
        //     tempColor.a = 0;
        //     m_mask.GetComponent<Image>().color = tempColor;
        // }
        AssetReference go = null;
        switch (type)
        {
            case PopupType.Shop:
                go = m_shop;
                break;
            case PopupType.DailyReward:
                go = m_dailyReward;
                break;
            case PopupType.Subscription:
                go = m_subscription;
                break;
            case PopupType.RemoveAds:
                go = m_removeAds;
                break;
            case PopupType.UnlockSkin:            
                go = m_unlockSkin;
                break;
            case PopupType.WatchAds:
                go = m_watchAds;
                break;
            case PopupType.BoostItems:
                go = m_boostItems;
                break;
            case PopupType.Pause:
                go = m_pause;
                break;
            case PopupType.ReviveRedBlue:
                go = m_reviveRedBlue;
                break;
            case PopupType.ReviveStick:
                go = m_reviveStick;
                break;
            case PopupType.Update:
                go = m_update;
                break;
            case PopupType.HeartStore:
                go = m_shopHeart;
                break;
            case PopupType.ShopIngame:
                go = m_shopIngame;
                break;
            case PopupType.BossCollection:
                go = m_bossCollection;
                break;
            case PopupType.SkinSelection:
                go = m_skinComboSelection;
                break;
        }
        StartCoroutine(IOpen(go, type, playAnim));
    }

    IEnumerator IOpen(AssetReference go, PopupType type, bool playAnim)
    {
        if(go == null)
            yield break;
        m_prev.Add(type);
        GameObject sample = null;
        if(go.Asset == null)       
        {     
            AsyncOperationHandle<GameObject> operation = go.LoadAssetAsync<GameObject>();
            yield return operation;
            sample = operation.Result;
        }
        else
            sample = go.Asset as GameObject;
        GameObject popup = Instantiate(sample, transform, false);
        m_popups.Add(type, popup);
        GameObject panel = popup.transform.Find("panel").gameObject;
        panel.transform.localScale = Vector3.zero;
        float time = playAnim ? 0.25f : 0 ;
        panel.transform.DOScale(Vector3.one, time).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private class PopupData
    {
        public PopupType pType;
        public object data;
    }

    public void CheatLeftOnclick()
    {
        if(m_cheatCounter != -1)
            return;
        StopAllCoroutines();
        m_cheatCounter = 0;        
        StartCoroutine(DelayResetCheat());
    }

    public void CheatRightOnclick()
    {
        if(m_cheatCounter != 1)
            return;
        m_cheatCounter = -1;
        StopAllCoroutines();
        m_cheat.SetActive(true);
    }

    public void CheatCenterOnclick()
    {
        if(m_cheatCounter != 0)
            return;
        StopAllCoroutines();
        m_cheatCounter = 1;        
        StartCoroutine(DelayResetCheat());
    }

    public void CheatLeaderBoardCenterOnClick ()
    {
        //if (m_cheatLeaderBoardCounter == 5)
        //{
        //    m_cheatLeaderBoardCounter = -1;
        //    StopAllCoroutines();
        //    m_cheatLeaderBoard.SetActive(true);
        //}
        //if (m_cheatLeaderBoardCounter < 5)
        //{
        //    StopAllCoroutines();
        //    m_cheatLeaderBoardCounter++;
        //    StartCoroutine(DelayResetCheatLeaderBoard());
        //}
    }

    IEnumerator DelayResetCheat()
    {
        yield return new WaitForSeconds(1f);
        m_cheatCounter = -1;
    }

    IEnumerator DelayResetCheatLeaderBoard()
    {
        yield return new WaitForSeconds(1f);
        m_cheatLeaderBoardCounter = -1;
    }
}
