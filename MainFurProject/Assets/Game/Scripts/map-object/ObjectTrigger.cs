using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectTrigger : MonoBehaviour
{
    [SerializeField] private string m_key = "";
    [SerializeField] private bool m_value = true;
    [SerializeField] private bool m_persistent = false;
    private bool m_stop;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_stop || collision.tag != GameTag.PLAYER)
            return;
        m_stop = true;
        GameController.DoTrigger(m_key, m_value);
        if(m_persistent)
        {
            gameObject.SetActive(false);
            m_stop = false;
        }
        else
            Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + Vector3.up, m_key);
    }
#endif
}
