using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectMonster : STObjectInteractive
{
    public bool startBehaviour = true;

    public GameObject[] deadDropItems;

    public virtual void Attack()
    {
        bodyState = State.Attacking;
    }
    public virtual void AttackComplete()
    {
        bodyState = State.Normal;
    }
    public virtual void PlayerInRange(Collider2D other)
    {

    }
    public virtual void PlayerOutRange(Collider2D other)
    {

    }
    public override void Dead()
    {
        base.Dead();
        GameController.MonsterDead(gameObject);
        DropItem();
    }
    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        base.OnHit(attackerInfor);
        if(isDead && AttackerCache.attacker != null && AttackerCache.attacker.gameObject.tag == GameTag.PLAYER)
            GameController.ShakeCamera();
    }

    public virtual void StartBehaviour()
    {
        myRigidbody.simulated = true;
        startBehaviour = true;
    }
    public virtual void PauseBehaviour()
    {
        myRigidbody.simulated = false;
        startBehaviour = false;
        spine.AnimationState.SetAnimation(0, "idle", true);
    }
    public virtual void DropItem()
    {
        if(deadDropItems.Length == 0)
            return;
        for(int i = 0; i < deadDropItems.Length; i++)
        {
            GameObject go = Instantiate(deadDropItems[i],transform.position, Quaternion.identity, transform.parent);
            go.SetActive(true);
            Vector2 targetForce = new Vector2(Random.Range(-8, 8), 13);
            go.GetComponent<Rigidbody2D>().AddForce(targetForce, ForceMode2D.Impulse);
        }
    }
}
