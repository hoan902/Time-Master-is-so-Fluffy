using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneBoardTrigger : MonoBehaviour
{
    enum ActionType
    {
        Continue,
        ActiveGroup,
        Stop
    }

    public string mesage = "";
    public string textOk = "OK";
    public string textCancel = "CANCEL";
    [SerializeField] private ActionType m_okAction = ActionType.Continue;
    [SerializeField] private ActionType m_cancelAction = ActionType.Stop;
    [SerializeField] private bool m_showAds;
    [SerializeField] private int m_timeWaiting = -1;
    [SerializeField] private bool m_hasToPause = true;
    public List<string> okGroups;
    public List<string> cancelGroup;

    private bool m_trigger;
    private bool m_result;

    void Awake()
    {
        CutSceneController.boardResultEvent += OnBoardResult;
    }

    private IEnumerator Start()
    {
        yield return null;
        CutSceneController.ActiveGroups(false, okGroups);
        CutSceneController.ActiveGroups(false, cancelGroup);
    }

    void OnDestroy()
    {
        CutSceneController.boardResultEvent -= OnBoardResult;
    }

    private void OnBoardResult(bool isOk)
    {
        if (!m_trigger)
            return;
        ActionType ac = isOk ? m_okAction : m_cancelAction;
        switch (ac)
        {
            case ActionType.Continue:
                CutSceneController.PauseCutScene(false);
                break;
            case ActionType.ActiveGroup:
                m_result = isOk;
                if(m_hasToPause)
                {
                    CutSceneController.ActiveGroups(true, isOk ? okGroups : cancelGroup);
                    CutSceneController.PauseCutScene(false);
                }
                else if(isOk)
                {
                    CutSceneController.UpdateCutsceneSpeed(3f);
                }
                break;
            case ActionType.Stop:
                CutSceneController.BeginFinish();
                break;
        }
        m_trigger = false;
    }

    public void OnTrigger()
    {
        m_trigger = true;
        CutSceneController.PauseCutScene(m_hasToPause);
        CutSceneController.ActiveBoard(mesage, textOk, textCancel, m_showAds, m_timeWaiting, m_hasToPause);
    }

    public void OnFastEnd()
    {
        if(m_hasToPause)
            return;
        CutSceneController.UpdateCutsceneSpeed(1f);
        CutSceneController.ActiveGroups(true, m_result ? okGroups : cancelGroup);
    }
}
