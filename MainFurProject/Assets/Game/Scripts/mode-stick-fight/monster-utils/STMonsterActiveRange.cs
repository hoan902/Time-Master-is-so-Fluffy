using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STMonsterActiveRange : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.tag != GameTag.PLAYER)
            return;
        if(other.offset.y < 0.5f)
            return;
        transform.parent.gameObject.SendMessage("PlayerInRange", other, SendMessageOptions.DontRequireReceiver);
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if(other.gameObject.tag != GameTag.PLAYER)
            return;
        if(other.offset.y < 0.5f)
            return;
        transform.parent.gameObject.SendMessage("PlayerOutRange", other, SendMessageOptions.DontRequireReceiver);
    }
}
