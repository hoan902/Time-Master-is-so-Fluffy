using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterVision : MonoBehaviour
{
    void Update()
    {
        transform.eulerAngles = Vector3.zero;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != GameTag.PLAYER)
            return;
        Rigidbody2D rigid = collision.GetComponent<Rigidbody2D>();
        if (rigid == null)
            return;
        MonsterVisionData data = new MonsterVisionData();
        data.isTrigger = true;
        data.direction = (int)Mathf.Sign(collision.transform.position.x - transform.position.x);
        transform.parent.SendMessage("OnVision", data);        
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag != GameTag.PLAYER)
            return;
        Rigidbody2D rigid = collision.GetComponent<Rigidbody2D>();
        if (rigid == null)
            return;
        MonsterVisionData data = new MonsterVisionData();
        data.isTrigger = true;
        data.direction = (int)Mathf.Sign(collision.transform.position.x - transform.position.x);
        transform.parent.SendMessage("OnVision", data);
    }
}

public class MonsterVisionData
{
    public bool isTrigger;
    public int direction;
}
