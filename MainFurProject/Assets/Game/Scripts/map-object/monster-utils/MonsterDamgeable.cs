using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDamgeable : MonoBehaviour
{
    [Range(0, 3)]
    [SerializeField] private int m_damage;
    private bool m_damageable = true;

    public bool Damageable{get => m_damageable; set => m_damageable = value;}
    public int Damage{get => m_damage; set => m_damage = value;}

    private void Start() 
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != GameTag.PLAYER)
            return;
        if(Damageable)
            GameController.UpdateHealth(-m_damage);
    }
}
