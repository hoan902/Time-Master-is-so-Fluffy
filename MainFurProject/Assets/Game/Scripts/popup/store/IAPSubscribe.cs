
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPSubscribe : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_price;

    void Start()
    {
        Init();
    }

    void Init()
    {
        Product product = IAPManager.api.GetProduct(IAPManager.PRODUCT_ID_VIP_SUBSCRIPTION);
        if (product == null)
            m_price.text = "NA";
        else
            m_price.text = product.metadata.localizedPriceString + "/WEEk";
    }

    public void SubscribeOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        if(Application.isEditor)
        {
            MainController.SubscribeVip(true);
            MainController.ClosePopup(PopupType.Subscription);
            IAPManager.purchaseResultEvent?.Invoke(IAPManager.PRODUCT_ID_VIP_SUBSCRIPTION, true);
        }
        else
        {
            IAPManager.api.BuyProductID(IAPManager.PRODUCT_ID_VIP_SUBSCRIPTION);
#if UNITY_IOS
            MainController.ActiveLoading(true, 0);
#endif
        }
    }

    public void CloseOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.ClosePopup(PopupType.Subscription);
    }

    public void OnclickPolicy()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        Application.OpenURL(GameConstant.URL_POLICY);
    }

    public void OnclickTerm()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        Application.OpenURL(GameConstant.URL_TERM);
    }
}
