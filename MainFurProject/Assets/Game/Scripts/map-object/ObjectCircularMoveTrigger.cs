using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCircularMoveTrigger : MonoBehaviour
{
    [SerializeField] private List<ObjectCircularMove> m_childs;
    [SerializeField] private bool m_childUseMyParams = false;
    [SerializeField] private bool m_clockwise;
    [SerializeField] private float m_speed = 3;
    [SerializeField] private float m_centerOffset = 3f;

    private bool m_actived = false;

    public Vector3 startPos;

    private void Start() 
    {
        if(m_childUseMyParams)
        {
            foreach(ObjectCircularMove child in m_childs)
            {
                child.UpdateParams(startPos, m_speed, m_centerOffset, m_clockwise);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.tag != GameTag.PLAYER || m_actived)
            return;
        m_actived = true;
        foreach(ObjectCircularMove child in m_childs)
        {
            child.gameObject.SetActive(true);
            child.StartMove();
        }
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPos, 0.5f);    
    }
}
