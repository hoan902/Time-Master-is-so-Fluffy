using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupRemoveAds : MonoBehaviour
{
    [SerializeField] private IAPProductUI m_buttonBuy;
    void OnEnable()
    {
        IAPManager.purchaseResultEvent += OnPurchaseResult;
        //
        Init();
    }

    void OnDisable()
    {
        IAPManager.purchaseResultEvent -= OnPurchaseResult;
    }

    private void OnPurchaseResult(string productId, bool status)
    {
        if(!status)
            return;
        switch(productId)
        {
            case IAPManager.PRODUCT_ID_REMOVE_ADS:
                MainController.RemoveAds();
                MainController.ClosePopup(PopupType.RemoveAds);
                break;
        }
    }

    void Init()
    {
        m_buttonBuy.Init(IAPManager.api.GetProduct(IAPManager.PRODUCT_ID_REMOVE_ADS));
    }

    public void CloseOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.ClosePopup(PopupType.RemoveAds);
    }

    public void OnclickRemoveAds()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        IAPManager.api.BuyProductID(IAPManager.PRODUCT_ID_REMOVE_ADS);
    }
}
