using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STMonsterScarecrow : STObjectMonster
{
    [SerializeField] private AudioClip m_audioDead;
    [SerializeField] private Collider2D m_collider;

    public STObjectBoss Boss;

    public override void Dead()
    {
        base.Dead();
        m_collider.enabled = false;
    }
    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        SoundManager.PlaySound(m_audioDead, false);
        Destroy(gameObject);
    }
    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        if(Boss)
        {
            int bossDamage = (attackerInfor.damage > currentHP) ? (int)currentHP : attackerInfor.damage;
            DamageDealerInfo damageDealerInfo = new DamageDealerInfo();
            damageDealerInfo.damage = bossDamage;
            damageDealerInfo.attacker = transform;
            Boss.gameObject.SendMessage("OnHit", damageDealerInfo, SendMessageOptions.DontRequireReceiver);
        }
        base.OnHit(attackerInfor);
    }
}
