using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupShopHeart : MonoBehaviour
{
    [SerializeField] private HeartItemUI[] m_items;
    [SerializeField] private GameObject m_liveObj;
    [SerializeField] private TextMeshProUGUI m_heartText;
    [SerializeField] private Transform m_destination;
    [SerializeField] private Image m_avatar;

    private List<int> m_soldOut;

    public static bool hasToRenew
    {
        set 
        {
            if(value == true)
            {
                PlayerPrefs.DeleteKey(DataKey.SOLD_OUT_HEART);
                for(int i = 0; i < 3; i++) // fix later
                {
                    PlayerPrefController.SetAdsWatchedHeartItem(i, 0);
                }
                PlayerPrefs.Save();
            }
        }
    }

    void Start()
    {
        hasToRenew = true;
        m_soldOut = PlayerPrefController.GetSoldOutHeartList();
        for(int i = 0; i < m_items.Length; i++)
        {
            m_items[i].Init(m_soldOut.Contains(i), i);
        }
        m_liveObj.SetActive(false);
        m_heartText.text = MainModel.totalHeart + "";
        
        m_avatar.sprite = ConfigLoader.instance.config.GetPlayerAvatar(MainModel.currentSkin);
        m_avatar.SetNativeSize();

        MainController.updateHeartEvent += OnUpdateHeart;
    }
    private void OnDestroy() 
    {
        MainController.updateHeartEvent -= OnUpdateHeart;
    }

    private void OnUpdateHeart(int heart)
    {
        m_heartText.text = heart + "";
    }
    public void UpdateSoldOut(int claimedIndex)
    {
        m_soldOut.Add(claimedIndex);
        PlayerPrefController.SetSoldOutHeartList(m_soldOut);
        m_liveObj.SetActive(true);
    }
    public void BackOnClick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.ClosePopup(PopupType.HeartStore);
        // if(MainModel.currentSceneType == SceneType.Game)
        // {
        //     AdsManager.api.ShowInterstitial("CHECK_POINT_SHOP");
        // }
    }

    public Vector3 GetEffDestination()
    {
        return m_destination.position;
    }
}
