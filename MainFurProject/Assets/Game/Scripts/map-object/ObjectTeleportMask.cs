using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTeleportMask : MonoBehaviour
{
    [SerializeField] Vector3 m_destination;
    [SerializeField] bool m_hasDestroy = true;
    
    private BoxCollider2D m_collider;
    private Coroutine m_reactiveRoutine;

    private void Start() 
    {
        m_collider = GetComponent<BoxCollider2D>();   

        GameController.maskTeleClosedEvent += OnMaskClosed; 
    }
    private void OnDestroy() 
    {
        GameController.maskTeleClosedEvent -= OnMaskClosed; 
    }
    void OnMaskClosed(Vector3 dummyPos)
    {
        if(!gameObject.activeSelf)
            return;
        if(m_reactiveRoutine != null)
            StopCoroutine(m_reactiveRoutine);
        m_reactiveRoutine = StartCoroutine(IReactive());
    }
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.tag != GameTag.PLAYER)
            return;
        m_collider.enabled = false;
        GameController.Teleport(m_destination);
        if(m_hasDestroy)
            Destroy(gameObject);
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(m_destination, 0.5f);    
    }

    IEnumerator IReactive()
    {
        yield return new WaitForSeconds(2f);
        m_collider.enabled = true;
    }

    public void UpdateDestination(Vector3 des)
    {
        m_destination = des;
    }
    public Vector3 GetDestination()
    {
        return m_destination;
    }
}
