using System;

public static class SystemController
{
    public static Action doUpdateEvt;
    public static Action initAppOpenEvent;
    public static Action initAdsEvent;
    public static Action initFirebaseEvent;
    public static Action initFacebookEvent;
    public static Action initAppsflyerEvent;
    public static Action initInappUpdateEvent;
    public static Action initBundleEvent;
    public static Action configLoadedEvt;

    public static void LoadConfigSuccess()
    {
        configLoadedEvt?.Invoke();
    }

    public static void DoUpdate()
    {
        doUpdateEvt?.Invoke();
    }

    public static void CancelUpdate()
    {
        SystemVariable.updateReady = false;
    }

    public static void InitAppOpen()
    {
        initAppOpenEvent?.Invoke();
    }

    public static void InitAds()
    {
        initAdsEvent?.Invoke();
    }

    public static void InitFirebase()
    {
        initFirebaseEvent?.Invoke();
    }

    public static void InitFacebook()
    {
        initFacebookEvent?.Invoke();
    }

    public static void InitAppsflyer()
    {
        initAppsflyerEvent?.Invoke();
    }

    public static void InitInappUpdate()
    {
        initInappUpdateEvent?.Invoke();
    }

    public static void InitBundle()
    {
        initBundleEvent?.Invoke();
    }
}
