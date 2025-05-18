using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectSawChild : MonoBehaviour
{
    [SerializeField] private STObjectSaw m_object;

    private DamageDealerInfo m_damageDealerInfor;

    private void Start() 
    {
        m_damageDealerInfor = new DamageDealerInfo();
        m_damageDealerInfor.damage = m_object.GetDamage(true);
        m_damageDealerInfor.attacker = this.transform;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(m_damageDealerInfor == null)
            return;
        if (collision.tag == GameTag.PLAYER)
        {   
            m_damageDealerInfor.damage = m_object.GetDamage(true);
            STGameController.HitPlayer(m_damageDealerInfor);
        }
        else
        {
            STObjectMonster monster = collision.GetComponent<STObjectMonster>();
            if(monster)
            {
                m_damageDealerInfor.damage = m_object.GetDamage(false);
                monster.gameObject.SendMessage("OnHit", m_damageDealerInfor, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
