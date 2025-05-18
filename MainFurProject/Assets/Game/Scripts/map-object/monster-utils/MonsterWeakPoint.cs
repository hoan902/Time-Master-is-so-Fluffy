using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterWeakPoint : MonoBehaviour
{
    public void OnHit()
    {
        ObjectBase monster = GetComponentInParent<ObjectBase>();
        if(monster == null || monster.isDead)
            return;
        transform.parent.gameObject.SendMessage("OnKilled", SendMessageOptions.DontRequireReceiver);
    }
}
