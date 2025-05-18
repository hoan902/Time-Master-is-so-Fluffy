using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemDropper : MonoBehaviour
{
    [SerializeField] ItemToDrop[] itemList;

    public void Drop(Vector3 startPos, GameObject? collector = null)
    {
        if(itemList.Length == 0)
            return;
        foreach(ItemToDrop item in itemList)
        {
            for(int i = 0; i < 1; i++) // fix amount later
            {
                Vector3 spawnPos = new Vector3(startPos.x, startPos.y + 1, startPos.z);
                GameObject go = Instantiate(item.itemPrefab, spawnPos, Quaternion.identity, transform.parent);
                Dropable dropableItem = go.GetComponent<Dropable>();
                dropableItem.SetKey(item.keyTrigger, item.secondKeyTrigger);
                dropableItem.hasAds = item.hasAds;
                if(collector)
                    dropableItem.collector = collector;

                Rigidbody2D itemRig = go.GetComponent<Rigidbody2D>();
                if(itemRig)
                {
                    float rand = UnityEngine.Random.Range(-item.xForce, item.xForce);
                    float randY = UnityEngine.Random.Range(item.yForce - 100, item.yForce);
                    go.GetComponent<Rigidbody2D>().AddForce(new Vector2(rand, randY));
                }
            }
        }
    }
}
[Serializable]
public class ItemToDrop
{
    public GameObject itemPrefab;
    public int amount;
    [Range(100, 500)]
    public float xForce;
    [Range(100, 500)]
    public float yForce;
    public string keyTrigger;
    public string secondKeyTrigger;
    public bool hasAds;
}
