using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectMaceChild : MonoBehaviour
{
    [SerializeField] private STObjectMace m_parent;

    private DamageDealerInfo m_damageDealerInfor;

    private void Start() 
    {
        if(m_parent.AlwaysShake)
            GetComponent<PointEffector2D>().forceMagnitude = 0;
        m_damageDealerInfor = new DamageDealerInfo();
        m_damageDealerInfor.damage = m_parent.GetDamage(true);
        m_damageDealerInfor.attacker = this.transform;
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if(m_damageDealerInfor == null || m_parent == null)
            return;
        if (collider.tag == GameTag.PLAYER)
        {
            m_damageDealerInfor.damage = m_parent.GetDamage(true);
            STGameController.HitPlayer(m_damageDealerInfor);
        }
        else
        {
            STObjectMonster monster = collider.GetComponent<STObjectMonster>();
            if(monster)
            {
                m_damageDealerInfor.damage = m_parent.GetDamage(false);
                monster.gameObject.SendMessage("OnHit", m_damageDealerInfor, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
