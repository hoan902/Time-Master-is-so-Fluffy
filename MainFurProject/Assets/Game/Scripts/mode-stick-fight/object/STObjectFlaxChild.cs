using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectFlaxChild : MonoBehaviour
{
    private STObjectFlax m_flax;
    private DamageDealerInfo m_damageDealerInfor;
    private AreaEffector2D m_areaEffector;

    void Start()
    {
        m_areaEffector = GetComponent<AreaEffector2D>();
        m_flax = gameObject.transform.parent.GetComponent<STObjectFlax>();
        m_damageDealerInfor = new DamageDealerInfo();
        m_damageDealerInfor.damage = m_flax.GetDamage(true);
        m_damageDealerInfor.attacker = this.transform;

        STGameController.updatePlayerImmunityEvent += OnPlayerImmunity;
    }

    private void OnDestroy() 
    {
        STGameController.updatePlayerImmunityEvent -= OnPlayerImmunity;
    }

    void OnPlayerImmunity(bool active, bool fromHit)
    {
        if(active)
            m_areaEffector.colliderMask = m_areaEffector.colliderMask & ~(1 << LayerMask.NameToLayer("player"));
        else
            m_areaEffector.colliderMask |= (1 << LayerMask.NameToLayer("player"));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(m_damageDealerInfor == null || m_flax == null)
            return;
        if (collision.tag == GameTag.PLAYER)
        {
            m_damageDealerInfor.damage = m_flax.GetDamage(true);
            STGameController.HitPlayer(m_damageDealerInfor);
        }
        else
        {
            STObjectMonster monster = collision.GetComponent<STObjectMonster>();
            if(monster)
            {
                m_damageDealerInfor.damage = m_flax.GetDamage(false);
                monster.gameObject.SendMessage("OnHit", m_damageDealerInfor, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
