using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ObjectThorn : MonoBehaviour
{
    [SerializeField] private int m_damage = 10;
    [Range(0, 100)]
    [SerializeField] private int m_amount;
    [SerializeField] private GameObject m_thorn;

    public List<GameObject> thorns;

    private void Start()
    {
        foreach(GameObject thorn in thorns)
        {
            thorn.GetComponent<STObjectDealDamage>().UpdateDamage(m_damage);
        }
    }

    // called in editor
    public void SpawnThorn()
    {
        RemoveAllThorn();
        thorns = new List<GameObject>();

        for(int i = 0; i < m_amount; i++)
        {
            GameObject thorn = Instantiate(m_thorn, transform);
            thorn.SetActive(true);
            thorns.Add(thorn);
            thorn.transform.localPosition = new Vector2(i, 0);
        }
    }
    public void RemoveAllThorn()
    {
        if (thorns == null)
            return;
        thorns.Clear();
        for(int i = 1; i < transform.childCount; i++)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
