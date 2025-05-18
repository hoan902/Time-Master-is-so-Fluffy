
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : IStoreListener
{
    private static IAPManager m_api;

    public static IAPManager api => m_api ?? (m_api = new IAPManager());

    public static Action<string, bool> purchaseResultEvent;

    private static IStoreController m_storeController;
    private static IExtensionProvider m_storeExtensionProvider;

    public const string PRODUCT_ID_VIP_SUBSCRIPTION = "com.platform.adventure.sword.knight.sub";
    public const string PRODUCT_ID_REMOVE_ADS_PLUS = "com.platform.adventure.sword.knight.pack2";
    public const string PRODUCT_ID_REMOVE_ADS = "com.platform.adventure.sword.knight.pack1";
    public const string PRODUCT_ID_SPECIAL_OFFER = "com.platform.adventure.sword.knight.specialoffer";

    private const string M_PRODUCT_VIP_SUBSCRIPTION_APPLE_ID = "com.platform.adventure.sword.knight.sub";
    private const string M_PRODUCT_VIP_SUBSCRIPTION_GOOGLE_ID = "com.platform.adventure.sword.knight.sub";

    public void Init()
    {
        InitService();
    }

    public void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(PRODUCT_ID_SPECIAL_OFFER, ProductType.Consumable);
        builder.AddProduct(PRODUCT_ID_REMOVE_ADS_PLUS, ProductType.Consumable);
        builder.AddProduct(PRODUCT_ID_REMOVE_ADS, ProductType.Consumable);
        builder.AddProduct(PRODUCT_ID_VIP_SUBSCRIPTION, ProductType.Subscription, new IDs(){
                        { M_PRODUCT_VIP_SUBSCRIPTION_APPLE_ID, AppleAppStore.Name },
                        { M_PRODUCT_VIP_SUBSCRIPTION_GOOGLE_ID, GooglePlay.Name },
                    });
        UnityPurchasing.Initialize(this, builder);
    }

    async void InitService()
    {
        try
        {
            var options = new InitializationOptions().SetEnvironmentName("production");
            await UnityServices.InitializeAsync(options);
            if (m_storeController == null)
                InitializePurchasing();
        }
        catch (Exception exception)
        {
            Debug.Log("IAPManager - Initialize service error: " + exception.Message);
        }
    }

    private bool IsInitialized()
    {
        return m_storeController != null && m_storeExtensionProvider != null;
    }

    public Product GetProduct(string productId)
    {
        if (m_storeController == null)
            return null;
        return m_storeController.products.WithID(productId);
    }

    public void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = m_storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                m_storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("RestorePurchases started ...");
            var apple = m_storeExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result, error) =>
            {
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        else
        {
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    /////////////////////////////////////// IStoreListener ///////////////////////////////////

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAPManager: OnInitialized - PASS");
        m_storeController = controller;
        m_storeExtensionProvider = extensions;
    }


    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + message);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        purchaseResultEvent?.Invoke(args.purchasedProduct.definition.storeSpecificId, true);
        //TrackingManager.IAPRevenue(args);
#if UNITY_IOS
        MainController.ActiveLoading(false, 1);
#endif
        return PurchaseProcessingResult.Complete;
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        purchaseResultEvent?.Invoke(product.definition.storeSpecificId, false);
#if UNITY_IOS
        MainController.ActiveLoading(false, 1);
#endif
    }
}
