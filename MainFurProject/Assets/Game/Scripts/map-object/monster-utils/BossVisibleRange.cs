using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossVisibleRange : MonoBehaviour
{
    private bool m_stop;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_stop)
            return;
        if (collision.tag != GameTag.PLAYER)
            return;
        m_stop = true;
        GameController.BossAppear();
    }
}
