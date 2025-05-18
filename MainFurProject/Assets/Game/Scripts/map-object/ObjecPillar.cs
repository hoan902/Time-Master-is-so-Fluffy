using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjecPillar : MonoBehaviour
{
    [SerializeField] int m_damageToPlayer = 10;
    [SerializeField] int m_damageToMonster = 10;
    private DamageDealerInfo m_damageDealerInfor;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        m_damageDealerInfor = new DamageDealerInfo
        {
            attacker = this.transform
        };
        if (collision.CompareTag(GameTag.PLAYER))
        {
            m_damageDealerInfor.damage = m_damageToPlayer;
            STGameController.HitPlayer(m_damageDealerInfor);
        }
        else
        {
            STObjectMonster monster = collision.GetComponent<STObjectMonster>();
            if (monster)
            {
                m_damageDealerInfor.damage = m_damageToMonster;
                monster.gameObject.SendMessage("OnHit", m_damageDealerInfor, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
