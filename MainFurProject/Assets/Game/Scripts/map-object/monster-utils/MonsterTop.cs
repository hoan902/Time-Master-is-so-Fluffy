using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTop : MonoBehaviour
{   
    void Update()
    {
        transform.eulerAngles = Vector3.zero;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag != GameTag.PLAYER)
            return;
        transform.parent.SendMessage("OnHit");
        Vector2 direction = transform.position - collision.transform.position;
        collision.rigidbody.AddForce(-direction.normalized * 3000);
    }
}
