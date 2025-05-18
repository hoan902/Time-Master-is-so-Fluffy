using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectTriggerByChild : MonoBehaviour
{
    [SerializeField] private string m_key = "";
    [SerializeField] private List<GameObject> m_childs;

    private void Awake() 
    {
        GameController.objectDestroyedEvent += OnObjectDestroyed;
    }
    private void OnDestroy() 
    {
        GameController.objectDestroyedEvent -= OnObjectDestroyed;
    }

    void OnObjectDestroyed(GameObject obj)
    {
        if(!m_childs.Contains(obj))
            return;
        
        m_childs.Remove(obj);

        if(m_childs.Count == 0 && m_key != "")
        {
            GameController.DoTrigger(m_key, true);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + Vector3.up, m_key);
    }
#endif
}
