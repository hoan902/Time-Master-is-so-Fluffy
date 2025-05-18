using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private string m_key = "";
    [SerializeField] int m_maxAmount = 3;
    [SerializeField] float m_spawnDuration = 5f;
    [SerializeField] GameObject m_objectToSpawn;
    [SerializeField] Transform m_spawnPoint;

    private int m_currentAmount;
    private float m_timer;
    private List<GameObject> m_spawnedObjects;
    
    public List<GameObject> SpawnedObjectList {get => m_spawnedObjects; set => m_spawnedObjects = value;}

    void Start()
    {
        m_currentAmount = 0;
        m_spawnedObjects = new List<GameObject>();

        GameController.monsterDeadEvent += OnMonsterDead;
        GameController.triggerEvent += OnTrigger;
    }

    private void OnDestroy() 
    {
        GameController.monsterDeadEvent -= OnMonsterDead;
        GameController.triggerEvent -= OnTrigger;
    }  

    void OnTrigger(string key, bool status, GameObject triggerSource)
    {
        if(key != m_key || !status || m_key == "")
            return;
        SpawnObject();
    }

    void OnMonsterDead(GameObject monster)
    {
        if(!m_spawnedObjects.Contains(monster))
            return;
        m_spawnedObjects.Remove(monster);
    }

    void Update()
    {
        if(m_key != "")
            return;
        m_timer += Time.deltaTime;
        if(m_timer >= m_spawnDuration)
        {
            m_timer = 0;
            SpawnObject();
        }
    }

    void SpawnObject()
    {
        if(m_spawnedObjects.Count >= m_maxAmount || m_objectToSpawn == null)
            return;

        GameObject go = Instantiate(m_objectToSpawn, m_spawnPoint.position, Quaternion.identity, transform.parent);
        go.SetActive(true);
        m_spawnedObjects.Add(go);
    }
}
