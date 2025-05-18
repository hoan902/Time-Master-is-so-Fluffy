using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMagicTrail : MonoBehaviour
{
    [SerializeField] Transform m_start;
    [SerializeField] Transform m_end;

    private float m_baseScaleY;

    private void Awake() 
    {
        m_baseScaleY = transform.localScale.y;    
    }
    private void Start() 
    {
        transform.position = m_start.position;    
    }

    private void Update() 
    {
        if(!m_end || !m_start)
        {
            Destroy(gameObject);
            return;
        }
            
        transform.position = m_start.position;
        float targetScaleX = Vector3.Distance(m_start.position, m_end.position) * 2 - 1;
        transform.localScale =  new Vector3(targetScaleX, m_baseScaleY, 1);

        Vector3 dir = (m_end.position - m_start.position).normalized;
        float angle = Mathf.Atan2(dir.x, dir.y);
        Quaternion targetRotation = Quaternion.Euler(0, 0, -angle * Mathf.Rad2Deg + 90);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 700 * Time.deltaTime);
    }
}
