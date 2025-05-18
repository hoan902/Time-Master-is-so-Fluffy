using UnityEngine;

public class STObjectDealDamage : MonoBehaviour
{
    [SerializeField] private int m_damage = 10;

    private DamageDealerInfo m_infor;

    private void Awake() 
    {
        if(m_infor != null)
            return;

        m_infor = new DamageDealerInfo();
        m_infor.damage = m_damage;
        m_infor.attacker = this.transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag(GameTag.PLAYER) || m_infor == null)
            return;
        STGameController.HitPlayer(m_infor);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(GameTag.PLAYER) || m_infor == null)
            return;
        STGameController.HitPlayer(m_infor);
    }

    public void UpdateDamage(int damage)
    {
        m_damage = damage;
        if(m_infor != null)
        {
            m_infor.damage = damage;
            return;
        }
        else
        {
            m_infor = new DamageDealerInfo();
            m_infor.damage = damage;
            m_infor.attacker = this.transform;
        }
    }
}
