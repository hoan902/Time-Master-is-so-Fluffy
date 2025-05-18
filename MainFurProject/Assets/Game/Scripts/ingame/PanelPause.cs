using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PanelPause : MonoBehaviour
{
    [SerializeField] private Toggle m_music;
    [SerializeField] private Toggle m_sound;
    [SerializeField] private Toggle m_vibration;
    [SerializeField] private Button m_buttonSkip;

    [SerializeField] private GameObject[] m_layoutNoRatingObjects;
    [SerializeField] private GameObject[] m_layoutRatingObjects;

    void Start()
    {
        m_music.isOn = SoundManager.IsMusicOn();
        m_sound.isOn = SoundManager.IsSoundOn();

        // foreach(GameObject child in m_layoutNoRatingObjects)
        // {
        //     child.SetActive(!RemoteConfig.showRatingInPause);
        // }
        // foreach(GameObject child in m_layoutRatingObjects)
        // {
        //     child.SetActive(RemoteConfig.showRatingInPause);
        // }
    }

    void OnEnable()
    {
        Time.timeScale = 0;
        MainModel.paused = true;
        GameInfo info = MainModel.gameInfo;
        m_buttonSkip.interactable = ConfigLoader.instance.worldLevel == info.world &&
                                    ConfigLoader.instance.mapLevel == info.level && info.playMode == PlayMode.Normal;
    }

    void OnDisable()
    {
        Time.timeScale = 1;
        MainModel.paused = false;
    }

    public void RestartOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.ClosePopup(PopupType.Pause);
        GameController.RestartGame(true);
    }

    public void HomeOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.ClosePopup(PopupType.Pause);
        GameController.QuitLevel();
    }

    public void CloseOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.ClosePopup(PopupType.Pause);
    }

    public void SkipLevelOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        if (Application.isEditor)
        {
            GameController.SkipLevel();
            MainController.ClosePopup(PopupType.Pause);
        }
        else
            GameController.SkipLevel();

        MainController.ClosePopup(PopupType.Pause);
    }

    public void MusicOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        SoundManager.MuteMusic(!m_music.isOn);
    }

    public void SoundOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        SoundManager.MuteSound(!m_sound.isOn);
    }

    public void VibrationOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
    }

    public void RatingOnClick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        if (Application.platform == RuntimePlatform.Android)
            Application.OpenURL("market://details?id=" + Application.identifier);
    }

    public void FacebookOnClick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        Application.OpenURL(GameConstant.URL_FACEBOOK_PAGE);
    }
}