using UnityEngine;

public class STObjectDealDamageOverTime : MonoBehaviour
{
    [SerializeField] private int m_damage = 10;
    [SerializeField] private float m_duration = 0.5f;

    private DamageDealerInfo m_infor;
    private float m_timer;
    private bool m_hasDamage;

    private void Awake()
    {
        if (m_infor != null)
            return;

        m_hasDamage = true;

        m_infor = new DamageDealerInfo();
        m_infor.damage = m_damage;
        m_infor.attacker = this.transform;
    }

    private void Update()
    {
        if (m_hasDamage)
            return;
        if (Time.time - m_timer >= 0)
            m_hasDamage = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag(GameTag.PLAYER) || m_infor == null || !m_hasDamage)
            return;
        m_hasDamage = false;
        m_timer = Time.time + m_duration;
        STGameController.HitPlayer(m_infor);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(GameTag.PLAYER) || m_infor == null || !m_hasDamage)
            return;
        m_hasDamage = false;
        m_timer = Time.time + m_duration;
        STGameController.HitPlayer(m_infor);
    }

    public void UpdateDamage(int damage, float duration)
    {
        m_damage = damage;
        m_duration = duration;
        if (m_infor != null)
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
