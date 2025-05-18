using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI progress;

    QuestStatus status;

    public void SetUp(QuestStatus status)
    {
        this.status = status;
        title.text = status.GetQuest().GetTitle() + "  " + status.GetCompletedCount() + "/" + status.GetQuest().GetObjectiveCount();
        // progress.text = status.GetCompletedCount() + "/" + status.GetQuest().GetObjectiveCount();
    }
    public QuestStatus GetQuestStatus()
    {
        return status;
    }
}
