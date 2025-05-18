using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDragonEgg : MonoBehaviour
{
    private bool m_claimed;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(m_claimed)
            return;
        m_claimed = true;
        QuestCompletion questCompletion = GetComponent<QuestCompletion>();
        if(questCompletion)
            questCompletion.CompleteObjective();  

        Destroy(gameObject);  
    }
}
