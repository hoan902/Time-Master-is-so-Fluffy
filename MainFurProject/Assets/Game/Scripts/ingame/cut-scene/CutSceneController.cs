using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneController
{
    public static Action<bool, float, bool> transitionEvent;//<isfull, duration, finish>
    public static Action nextActionEvent;
    public static Action<bool> pauseEvent;
    public static Action<bool, List<string>> activeGroupEvent;//<active, groups name>
    public static Action<string, string, string, bool, int, bool> boardEvent;//<message, text ok, text cancel, show ads, time wait>
    public static Action<bool> boardResultEvent;//<isOk>
    public static Action beginFinishEvent;
    public static Action finishEvent;
    public static Action<float> updateSpeedEvent;
    public static Action<bool> activateFastForwardEvent;
    public static Action skipCutsceneEvent;

    public static void DoCutSceneTransition(bool isFull, float duration, bool finish)
    {
        transitionEvent?.Invoke(isFull, duration, finish);
    }

    public static void NextCutSceneAction()
    {
        nextActionEvent?.Invoke();
    }

    public static void PauseCutScene(bool pause)
    {
        pauseEvent?.Invoke(pause);
    }

    public static void UpdateCutsceneSpeed(float value)
    {
        updateSpeedEvent?.Invoke(value);
    }

    public static void ActiveGroups(bool active, List<string> groups)
    {
        activeGroupEvent?.Invoke(active, groups);
    }

    public static void ActiveBoard(string message, string textOk, string textCancel, bool showAds, int timeWait, bool hasToPause)
    {
        boardEvent?.Invoke(message, textOk, textCancel, showAds, timeWait, hasToPause);
    }

    public static void DoBoardResult(bool isOk)
    {
        boardResultEvent?.Invoke(isOk);
    }

    public static void BeginFinish()
    {
        beginFinishEvent?.Invoke();
    }

    public static void ActivateFastForward(bool toActive)
    {
        activateFastForwardEvent?.Invoke(toActive);
    }
    public static void SkipCutscene()
    {
        skipCutsceneEvent?.Invoke();
    }

    public static void Finish()
    {
        MainModel.inCutscene = false;
        finishEvent?.Invoke();
        /*if(ConfigLoader.instance.mapLevel == 0)
        {
            TrackingManager.IngameAction(ConfigLoader.instance.GetFakeLevelIndex(MainModel.gameInfo.level, MainModel.gameInfo.world) + "_0");
        }*/
    }
}
