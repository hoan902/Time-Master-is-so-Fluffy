using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupWatchAds : MonoBehaviour
{
    public void CloseOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.ClosePopup(PopupType.WatchAds);
    }

    public void WatchAdsOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        if (Application.isEditor)
        {
            MainController.UpdateCoin(GameConstant.BONUS_COIN_ADS);
            MainController.ClosePopup(PopupType.WatchAds);
        }
        else
            MainController.UpdateCoin(GameConstant.BONUS_COIN_ADS);

        MainController.ClosePopup(PopupType.WatchAds);
    }
}