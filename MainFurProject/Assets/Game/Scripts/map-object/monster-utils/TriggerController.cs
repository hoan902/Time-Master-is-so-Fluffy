using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerController : MonoBehaviour
{
    public List<string> deadKeys;
    
    public void SetTriggerDead(List<string> deadList)
    {
        if(deadKeys.Count > 0)
            deadKeys.Clear();
        deadKeys = deadList;
    }

    public void TriggerDead()
    {
        foreach(string key in deadKeys)
        {
            GameController.DoTrigger(key, true);
        }
    }

}
