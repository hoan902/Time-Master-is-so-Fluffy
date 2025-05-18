using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupShopIngame : MonoBehaviour
{
    [SerializeField] private ShopIngameItem[] m_items;
    [SerializeField] private GameObject m_liveObj;
    [SerializeField] private TextMeshProUGUI m_heartText;
    [SerializeField] private Transform m_destination;
    [SerializeField] private Image m_avatar;

    private bool m_watchedAds;

    public bool WatchedAds { get => m_watchedAds; set => m_watchedAds = value; }
    
    void Start()
    {
        for (int i = 0; i < m_items.Length; i++)
        {
            m_items[i].Init(i, this);
        }
        m_liveObj.SetActive(false);
        m_heartText.text = MainModel.totalHeart + "";
        
        m_avatar.sprite = ConfigLoader.instance.config.GetPlayerAvatar(MainModel.CurrentSkin);
        m_avatar.SetNativeSize();

        GameController.updateHeartEvent += OnUpdateHeart;
    }
    private void OnDestroy()
    {
        GameController.updateHeartEvent -= OnUpdateHeart;
    }

    private void OnUpdateHeart(int heart, Vector3? itemPos)
    {
        m_heartText.text = heart + "";
    }
    public void UpdateSoldOut(int claimedIndex)
    {
        //m_soldOut.Add(claimedIndex);
        //PlayerPrefController.SetSoldOutHeartList(m_soldOut);
        m_liveObj.SetActive(true);
    }
    public void BackOnClick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.ClosePopup(PopupType.ShopIngame);
    }

    public Vector3 GetEffDestination()
    {
        return m_destination.position;
    }
}
