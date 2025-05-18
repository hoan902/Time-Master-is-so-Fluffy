using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Mgif/New Quest", order = 1)]
public class Quest : ScriptableObject
{
    [SerializeField] List<Objective> objectives = new List<Objective>();
    [SerializeField] List<QuestReward> rewards = new List<QuestReward>();

    public string GetTitle()
    {
        string result = name;
        result = name.Replace("_", " ");
        return result;
    }
    public int GetObjectiveCount()
    {
        return objectives.Count;
    }
    public List<Objective> GetObjectives()
    {
        return objectives;
    }
    public List<QuestReward> GetRewards()
    {
        return rewards;
    }
    public bool HasObjective(string objectiveRef)
    {
        foreach(Objective objective in objectives)
        {
            if(objective.reference == objectiveRef)
            {
                return true;
            }
        }
        return false;
    }

    public static Quest GetByName(string questName)
    {
            foreach(Quest quest in Resources.LoadAll<Quest>(""))
            {
            if(quest.name == questName)
            {
                return quest;
            }
            }
        return null;
    }
}

[System.Serializable]
public class Objective
{
    public string reference;
    public string description;
    public bool usesCondition = false;
    public Condition completionCondition;
}
[System.Serializable]
public class QuestReward
{
    public int coin;
    public int heart;
}
