
using System;
using System.Collections;
using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] private SlicedFilledImage m_process;
    [SerializeField] private TextAsset m_storeConfig;
    [SerializeField] private Image m_background;
    [SerializeField] private AudioClip m_music;
    [SerializeField] private Transform m_ball;
    [SerializeField] private Sprite m_ramadanSprite;

    private bool m_resourceComplete = false;
    private bool m_simulateComplete = false;
    private Tweener m_tweener;

    void Awake()
    {
        //Unity.Collections.NativeLeakDetection.Mode = Unity.Collections.NativeLeakDetectionMode.EnabledWithStackTrace;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = Mathf.Min(60, Screen.currentResolution.refreshRate);
        QualitySettings.vSyncCount = 0;
        HackBug();
        MainController.LoadConfig(m_storeConfig.text);
        // if(RemoteConfig.ramadan && m_ramadanSprite)
        //     m_background.sprite = m_ramadanSprite;
        //simulate load
        GameAssetManager.updateEvent += OnProcessUpdate;
        GameAssetManager.completeEvent += OnLoadResourceComplete;
        //
        m_process.fillAmount = 0;
        DoProgress(0.85f, 3f, ()=>{
            DoProgress(1f, 15f);
        });
        //
        StartCoroutine(IComplete());
        //
        MainModel.isTablet = (Screen.width / Screen.height) < 1.4f;
    }

    void OnDestroy()
    {
        GameAssetManager.updateEvent -= OnProcessUpdate;
        GameAssetManager.completeEvent -= OnLoadResourceComplete;
    }

    void DoProgress(float target, float time, Action onComplete = null)
    {
        if(m_tweener != null)
            m_tweener.Kill();
        // Vector3 startPos = new Vector3(-m_process.rectTransform.sizeDelta.x/2, m_ball.localPosition.y);  
        m_tweener = DOTween.To(() => m_process.fillAmount, x => m_process.fillAmount = x, target, time).SetEase(Ease.Linear).OnUpdate(()=>{
            // m_ball.localPosition = startPos + new Vector3(m_process.fillAmount * m_process.rectTransform.sizeDelta.x, 0);
            // m_ball.localEulerAngles = new Vector3(0, 0, -m_process.fillAmount*1080);
        }).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    IEnumerator DelayPlayMusic()
    {
        yield return null;
        yield return null;
        SoundManager.PlaySound(m_music, true, true);
    }

    private void OnLoadResourceComplete()
    {
        m_resourceComplete = true;
    }

    private void OnProcessUpdate(float process)
    {

    }

    IEnumerator IComplete()
    {        
        yield return new WaitForEndOfFrame();
        SystemController.InitBundle();
        yield return new WaitForEndOfFrame();
        SystemController.InitFirebase();
        yield return new WaitForEndOfFrame();
        SystemController.InitAppsflyer();
        yield return new WaitForEndOfFrame();
        SystemController.InitAds();
        yield return new WaitForEndOfFrame();
        //SystemController.InitFacebook();
        while (!m_resourceComplete)
        {
            yield return new WaitForEndOfFrame();
        }
        DoProgress(1f, 0.1f, ()=>{
            GameAssetManager.api.ActiveScene();   
        });             
    }

    void HackBug()
    {
        // These classes won't be linked away because of the code,
        // but we also won't have to construct unnecessarily either,
        // hence the if statement with (hopefully) impossible
        // runtime condition.
        //
        // This is to resolve crash at CultureInfo.CurrentCulture
        // when language is set to Thai. See
        // https://github.com/xamarin/Xamarin.Forms/issues/4037
        if (Environment.CurrentDirectory == "_never_POSSIBLE_")
        {
            new System.Globalization.ChineseLunisolarCalendar();
            new System.Globalization.HebrewCalendar();
            new System.Globalization.HijriCalendar();
            new System.Globalization.JapaneseCalendar();
            new System.Globalization.JapaneseLunisolarCalendar();
            new System.Globalization.KoreanCalendar();
            new System.Globalization.KoreanLunisolarCalendar();
            new System.Globalization.PersianCalendar();
            new System.Globalization.TaiwanCalendar();
            new System.Globalization.TaiwanLunisolarCalendar();
            new System.Globalization.ThaiBuddhistCalendar();
            new System.Globalization.UmAlQuraCalendar();
        }
    }
}
