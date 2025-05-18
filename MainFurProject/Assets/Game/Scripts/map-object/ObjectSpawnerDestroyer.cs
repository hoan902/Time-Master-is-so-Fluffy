using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawnerDestroyer : MonoBehaviour
{
    private ObjectSpawner m_spawner;

    private void Awake() 
    {
        m_spawner = GetComponentInParent<ObjectSpawner>();    
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        GameObject otherObj = other.gameObject;
        if(otherObj.GetComponent<ObjectBase>())
            return;
        if(!m_spawner.SpawnedObjectList.Contains(otherObj))
            return;
        m_spawner.SpawnedObjectList.Remove(otherObj);
        Destroy(otherObj);
    }
}
