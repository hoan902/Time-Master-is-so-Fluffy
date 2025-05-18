using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestStatus
{
    Quest quest;
    List<string> completedObjectives = new List<string>();

    public QuestStatus(Quest quest)
    {
        this.quest = quest;
    }

    public Quest GetQuest()
    {
        return quest;
    }

    public bool IsComplete()
    {
        foreach(var objective in quest.GetObjectives())
        {
            if(!completedObjectives.Contains(objective.reference))
            {
                return false;
            }
        }
        return true;
    }

    public int GetCompletedCount()
    {
        return completedObjectives.Count;
    }
    public bool IsObjectiveComplete(string objective)
    {
        return completedObjectives.Contains(objective);
    }

    public void CompleteObjective(string objective)
    {
        if(quest.HasObjective(objective))
        {
            completedObjectives.Add(objective);
        }
    }
}
