using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class QuestList : MonoBehaviour, IPredicateEvaluator
{
    List<QuestStatus> statuses = new List<QuestStatus>();
    public event Action onUpdate;

    // private void Update()
    // {
    //     CompleteObjectivesPredicates();
    // }

    public void AddQuest(Quest quest)
    {
        if (HasQuest(quest)) return;
        QuestStatus newStatus = new QuestStatus(quest);
        statuses.Add(newStatus);
        if(onUpdate != null)
        {
            onUpdate();
        }
    }    

    public void CompleteObjective(Quest quest, string objective)
    {
        QuestStatus status = GetQuestStatus(quest);
        status.CompleteObjective(objective);
        if(status.IsComplete())
        {
            GiveReward(quest);
        }
        if (onUpdate != null)
        {
            onUpdate();
        }
    }

    public bool HasQuest(Quest quest)
    {
        return GetQuestStatus(quest) != null;
    }

    public List<QuestStatus> GetStatuses()
    {
        return statuses;
    }

    public bool IsQuestCompleted(Quest quest)
    {
        if(!HasQuest(quest))
            return false;
        return GetQuestStatus(quest).IsComplete();
    }

    private QuestStatus GetQuestStatus(Quest quest)
    {
        foreach (QuestStatus status in statuses)
        {
            if (status.GetQuest() == quest)
            {
                return status;
            }
        }
        return null;
    }
    private void GiveReward(Quest quest)
    {
        foreach(QuestReward reward in quest.GetRewards())
        {
            if(reward.coin > 0)
            {
                // do something
            }
            if(reward.heart > 0)
            {
                // do something
            }
        }
    }

    private void CompleteObjectivesPredicates()
    {
        foreach (QuestStatus status in statuses)
        {
            if (status.IsComplete()) continue;
            Quest quest = status.GetQuest();
            foreach (var objective in quest.GetObjectives())
            {
                if (status.IsObjectiveComplete(objective.reference)) continue;
                if (!objective.usesCondition) continue;
                if (objective.completionCondition.Check(GetComponents<IPredicateEvaluator>()))
                {
                    CompleteObjective(quest, objective.reference);
                }
            }
        }
    }

    public bool? Evaluate(string predicate, string[] parameters)
    {
        if (predicate != "HasQuest") return null;
        switch(predicate)
        {
            case "HasQuest":
                return HasQuest(Quest.GetByName(parameters[0]));
            case "CompletedQuest":
                return GetQuestStatus(Quest.GetByName(parameters[0])).IsComplete(); 
        }
        return null;
    }
}
