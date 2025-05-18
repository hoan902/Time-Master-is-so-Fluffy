using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class STObjectTriggerMonster : MonoBehaviour
{
    [SerializeField] string m_key;
    [SerializeField] string m_nextWaveKey = "";
    [SerializeField] float m_readySpawnTime = 2f;
    [SerializeField] List<GameObject> m_monsters;
    [SerializeField] GameObject m_portalPrefab;

    private bool m_actived = false;
    private List<GameObject> m_portals;
    private List<GameObject> m_realMonsters;

    private void Start() 
    {
        if(m_monsters.Count == 0)
            return;
        m_realMonsters = new List<GameObject>();
        m_portals = new List<GameObject>();
        foreach(GameObject monster in m_monsters)
        {
            monster.SetActive(false);
            if(monster.GetComponent<STObjectMonster>())
                m_realMonsters.Add(monster);
        }    

        GameController.triggerEvent += OnTrigger;
        GameController.monsterDeadEvent += OnMonsterDead;
    }
    private void OnDestroy() 
    {
        GameController.triggerEvent -= OnTrigger;
        GameController.monsterDeadEvent -= OnMonsterDead;
    }

    void OnMonsterDead(GameObject monster)
    {
        if(!m_realMonsters.Contains(monster))
            return;
        m_monsters.Remove(monster);
        m_realMonsters.Remove(monster);

        if(m_realMonsters.Count == 0)
        {
            GameController.DoTrigger(m_nextWaveKey, true);
        }
            
    }
    void OnTrigger(string key, bool state, GameObject triggerSource)
    {
        if(key != m_key || state == false || m_actived)
            return;
        m_actived = true;
        foreach(GameObject monster in m_monsters)
        {
            STObjectMonster monsterComp = monster.GetComponent<STObjectMonster>();
            if(monsterComp)
            {
                monsterComp.PauseBehaviour();
                
                GameObject portal = Instantiate(m_portalPrefab, monsterComp.center.position, Quaternion.identity, transform.parent);
                portal.transform.localScale = Vector3.zero;
                m_portals.Add(portal);

                portal.transform.DOScale(Vector3.one, m_readySpawnTime / 2).OnComplete(() => {
                    monster.SetActive(true);
                    Vector3 baseScale = monster.transform.localScale;
                    monster.transform.localScale = Vector3.zero;

                    monster.transform.DOScale(baseScale, m_readySpawnTime / 2).OnComplete(() => 
                    {
                        monsterComp.StartBehaviour();
                    });
                });
            }
            else
            {
                monster.SetActive(true);
            }
        }

        StartCoroutine(IDelayDestroyPortal());
    }

    IEnumerator IDelayDestroyPortal()
    {
        yield return new WaitForSeconds(m_readySpawnTime + 0.2f);
        foreach(GameObject portal in m_portals)
        {
            Destroy(portal);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(m_key);
        stringBuilder.Append("\n");
        stringBuilder.Append(m_nextWaveKey);

        Handles.Label(transform.position + Vector3.up, stringBuilder.ToString());
    }
#endif
}
