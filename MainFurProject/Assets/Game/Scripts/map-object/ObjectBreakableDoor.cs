using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBreakableDoor : STObjectInteractive
{
    [SerializeField] GameObject m_effectBroken;
    [SerializeField] private Transform m_effectSpawnPos;

    private DamageDealerInfo m_damageDealer;
    private BoxCollider2D m_collider;

    public override void Awake()
    {
        base.Awake();
        m_collider = GetComponent<BoxCollider2D>();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override void OnDeadFinish()
    {
        Destroy(gameObject);
    }
    public override void Dead()
    {
        base.Dead();
        InitEffectBroken();
        gameObject.SetActive(false);
    }
    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        m_damageDealer = attackerInfor;
        base.OnHit(attackerInfor);
    }

    private void InitEffectBroken()
    {
        GameObject effGO = Instantiate(m_effectBroken, m_effectSpawnPos.position, Quaternion.identity, transform.parent);
        float xScale = m_damageDealer.attacker.position.x < transform.position.x ? 1 : -1;
        effGO.transform.localScale = new Vector3(xScale, 1, 1);
        effGO.SetActive(true);
    }
}
