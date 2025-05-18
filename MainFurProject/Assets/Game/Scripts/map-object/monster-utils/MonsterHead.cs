using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHead : MonoBehaviour
{
    [SerializeField]private GameObject m_monster;

    public void OnTriggerEnter2D(Collider2D collider)
    {
        m_monster.SendMessage("OnHurt");
    }
}
