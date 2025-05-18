using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectBoxHitter : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        STCrackingBox boxComp = other.GetComponent<STCrackingBox>();
        if (!boxComp)
            return;
        DamageDealerInfo damageDealerInfo = new DamageDealerInfo();
        damageDealerInfo.damage = (int)boxComp.maxHP;
        damageDealerInfo.attacker = transform;
        
        other.gameObject.SendMessage("OnHit", damageDealerInfo, SendMessageOptions.DontRequireReceiver);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        STCrackingBox boxComp = collision.gameObject.GetComponent<STCrackingBox>();
        if (!boxComp)
            return;
        DamageDealerInfo damageDealerInfo = new DamageDealerInfo();
        damageDealerInfo.damage = (int)boxComp.maxHP;
        damageDealerInfo.attacker = transform;

        collision.gameObject.SendMessage("OnHit", damageDealerInfo, SendMessageOptions.DontRequireReceiver);
    }
}
