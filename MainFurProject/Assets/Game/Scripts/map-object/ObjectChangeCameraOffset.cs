using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectChangeCameraOffset : MonoBehaviour
{
    public Vector2 offset;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag != GameTag.PLAYER) 
            return;
        if(collision.offset.y < 0.5f)
            return;
        GameController.ChangeCameraOffset(this, true);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag != GameTag.PLAYER)
            return;
        if(collision.offset.y < 0.5f)
            return;
        GameController.ChangeCameraOffset(this, false);
    }
}
