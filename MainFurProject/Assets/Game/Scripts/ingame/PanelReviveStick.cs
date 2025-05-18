using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Spine;
using Spine.Unity;

public class PanelReviveStick : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_textTime;
    [SerializeField] private SkeletonGraphic m_characterSpine;

    void Start()
    {
        StartCoroutine(CountDown(MapConstant.TIME_RIVIVE));
        m_characterSpine.SetSkin(MainModel.currentSkin);
    }

    public void CloseOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        StopAllCoroutines();
        gameObject.transform.localScale = Vector3.zero;
        MainController.ClosePopup(PopupType.ReviveStick);
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
            MainController.ClosePopup(PopupType.ReviveStick);
        }
        else
        {
            GameController.Revive();
            MainController.ClosePopup(PopupType.ReviveStick);
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

        MainController.ClosePopup(PopupType.ReviveStick);
        GameController.FailLevel();
        GameController.Quit(QuitGameReason.Fail, true);
    }
}