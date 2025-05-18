
using TMPro;
using UnityEngine;

public class PopupShop : MonoBehaviour
{
    [SerializeField] private IAPProductUI m_buttonAds;
    [SerializeField] private IAPProductUI m_buttonAdsPlus;
    [SerializeField] private GameObject m_buttonSubscribe;
    [SerializeField] private GameObject m_buttonRetore;
    [SerializeField] private TextMeshProUGUI m_textAdsPlus;
    [SerializeField] private GameObject m_iconRemoveAds;

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
            case IAPManager.PRODUCT_ID_VIP_SUBSCRIPTION:
                MainController.SubscribeVip(true);
                MainController.ClosePopup(PopupType.Subscription);
                Init();
                break;
            case IAPManager.PRODUCT_ID_REMOVE_ADS:
                MainController.RemoveAds();
                Init();
                break;
            case IAPManager.PRODUCT_ID_REMOVE_ADS_PLUS:
                MainController.RemoveAds();
                MainController.UpdateCoin(70000);
                Init();
                break;
        }
    }

    public void OnclickBecomeVip()
    {
        if(MainModel.subscription)
        {
            MainController.ShowNotice("You bought this package");
            return;
        }
        MainController.OpenPopup(PopupType.Subscription);
    }

    public void OnclickRemoveAdsPlus()
    {
        if(MainModel.removeAds)
        {
            MainController.ShowNotice("You bought this package");
            return;
        }
        IAPManager.api.BuyProductID(IAPManager.PRODUCT_ID_REMOVE_ADS_PLUS);
#if UNITY_IOS
        MainController.ActiveLoading(true, 0);
#endif
    }

    public void OnclickRemoveAds()
    {
        if(MainModel.removeAds)
        {
            MainController.ShowNotice("You bought this package");
            return;
        }
        IAPManager.api.BuyProductID(IAPManager.PRODUCT_ID_REMOVE_ADS);
#if UNITY_IOS
        MainController.ActiveLoading(true, 0);
#endif
    }

    public void CloseOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.ClosePopup(PopupType.Shop);
    }

    public void RetoreOnclick()
    {
        if(MainModel.removeAds)
            return;
        IAPManager.api.RestorePurchases();
    }

    void Init()
    {
        m_buttonSubscribe.SetActive(!MainModel.subscription);
        m_buttonAds.gameObject.SetActive(!MainModel.subscription && !MainModel.removeAds);
        m_buttonAdsPlus.gameObject.SetActive(!MainModel.subscription && !MainModel.removeAds);
        m_buttonRetore.SetActive(Application.platform == RuntimePlatform.IPhonePlayer);
        //
        m_buttonAds.Init(IAPManager.api.GetProduct(IAPManager.PRODUCT_ID_REMOVE_ADS));
        m_buttonAdsPlus.Init(IAPManager.api.GetProduct(IAPManager.PRODUCT_ID_REMOVE_ADS_PLUS));
        //
        m_textAdsPlus.text = Application.platform == RuntimePlatform.IPhonePlayer ? "+70000 COINS" : "REMOVE ADS\n+70000 COINS";
        m_iconRemoveAds.SetActive(Application.platform != RuntimePlatform.IPhonePlayer);
    }
}
