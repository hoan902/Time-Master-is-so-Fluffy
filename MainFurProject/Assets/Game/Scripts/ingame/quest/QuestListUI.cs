using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestListUI : MonoBehaviour
{
    [SerializeField] private QuestItemUI m_questPrefab;
    QuestList m_questList;

    private void OnDestroy() 
    {
        if(m_questList != null)
            m_questList.onUpdate -= Redraw;    
    }

    public void Init(QuestList questList)
    {
        m_questList = questList;
        m_questList = FindObjectOfType<QuestList>();
        m_questList.onUpdate += Redraw;
        Redraw();
    }

    private void Redraw()
    {
        foreach (Transform item in transform)
        {
            Destroy(item.gameObject);
        }
        foreach (QuestStatus status in m_questList.GetStatuses())
        {
            QuestItemUI uiInstance = Instantiate<QuestItemUI>(m_questPrefab, transform);
            uiInstance.SetUp(status);
        }
    }
}
