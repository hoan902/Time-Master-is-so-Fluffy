
using UnityEngine;

public class BattleAxeTrailChild : MonoBehaviour
{
    private DamageDealerInfo m_damageDealerInfo;
    private bool m_ready;
    
    public void Init(int damage)
    {
        m_damageDealerInfo = new DamageDealerInfo();
        m_damageDealerInfo.damage = damage;
        m_damageDealerInfo.attacker = transform;

        m_ready = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!m_ready)
            return;
        STObjectInteractive objectInteractive = other.GetComponent<STObjectInteractive>();
        if(!objectInteractive)
            return;
        objectInteractive.gameObject.SendMessage("OnHit", m_damageDealerInfo, SendMessageOptions.DontRequireReceiver);
    }
}
