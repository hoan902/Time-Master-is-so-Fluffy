using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STMonsterBody : MonoBehaviour
{
    public void OnHit(DamageDealerInfo attackerInfor)
    {
        transform.parent.gameObject.SendMessage("OnHit", attackerInfor, SendMessageOptions.DontRequireReceiver);
    }
}
