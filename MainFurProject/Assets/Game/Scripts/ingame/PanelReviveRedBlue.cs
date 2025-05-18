using System.Collections;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelReviveRedBlue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_textTime;

    void Start()
    {
        StartCoroutine(CountDown(MapConstant.TIME_RIVIVE));
    }

    public void CloseOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        StopAllCoroutines();
        gameObject.transform.localScale = Vector3.zero;
        MainController.ClosePopup(PopupType.ReviveRedBlue);
        GameController.FailLevel();
        GameController.Quit(QuitGameReason.Fail, true);
    }

    public void RiviveOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        StopAllCoroutines();
        if (Application.isEditor)
        {
            GameController.Revive();
            MainController.ClosePopup(PopupType.ReviveRedBlue);
        }
        else
        {
            GameController.Revive();
            MainController.ClosePopup(PopupType.ReviveRedBlue);
        }
    }

    IEnumerator CountDown(int time)
    {
        while (time > 0)
        {
            m_textTime.text = "(" + time + ")";
            yield return new WaitForSeconds(1);
            time--;
        }

        MainController.ClosePopup(PopupType.ReviveRedBlue);
        GameController.FailLevel();
        GameController.Quit(QuitGameReason.Fail, true);
    }
}