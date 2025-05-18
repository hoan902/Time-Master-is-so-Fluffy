using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinBarUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_textCoin;


    private int m_currentCoins;
    private Tweener m_coinTween;

    void Awake()
    {
        MainController.updateCoinEvent += OnUpdateCoin;        
    }

    private void Start()
    {
        MainController.UpdateCoin(0);
    }

    private void OnDestroy()
    {
        MainController.updateCoinEvent -= OnUpdateCoin;
    }

    private void OnUpdateCoin(int coin)
    {
        //m_currentCoins = coin;
        m_coinTween?.Kill();
        if (m_currentCoins == coin)
            m_textCoin.text = GameUtils.CoinToString(m_currentCoins);
        else
        {
            m_coinTween = DOTween.To(() => m_currentCoins, x => {
                m_currentCoins = x;
                m_textCoin.text = GameUtils.CoinToString(x);
            }, coin, 1f);
        }
    }

    public void ShopOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.OpenPopup(PopupType.Shop);
    }
}
