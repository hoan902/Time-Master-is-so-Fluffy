using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectMonsterFireball : MonoBehaviour
{
    [SerializeField] int m_damage = 10;
    [SerializeField] float m_jumpDuration = 3f;
    [SerializeField] float m_jumpHeight = 5f;
    [SerializeField] float m_jumpTime = 2f;
    [SerializeField] float m_firstJumpDelay = 0;
    [SerializeField] STObjectDealDamage m_damageDealer;

    IEnumerator Start()
    {
        m_damageDealer.UpdateDamage(m_damage);
        yield return new WaitForSeconds(m_firstJumpDelay);
        transform.DOJump(transform.position, m_jumpHeight, 1, m_jumpTime).SetDelay(m_jumpDuration).SetLoops(-1).SetEase(Ease.Linear);
    }
}
