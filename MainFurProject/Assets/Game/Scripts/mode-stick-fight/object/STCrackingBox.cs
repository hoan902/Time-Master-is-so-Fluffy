using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class STCrackingBox : STObjectInteractive
{
    [SerializeField] private CrackingChestReward[] m_rewards;
    [SerializeField] private Vector2 m_force;
    [SerializeField] private float m_brokenDelay = 0.02f;

    [Header("References")]
    [SerializeField] GameObject m_effectBroken;
    [SerializeField] private GameObject m_coinObj;
    [SerializeField] private GameObject m_heartObj;
    [SerializeField] private GameObject m_healPotionMini;
    [SerializeField] private GameObject m_healPotionLarge;

    [Header("Audio")]
    [SerializeField] private AudioClip[] m_audiosBreak;

    private BoxCollider2D m_collider;
    private DamageDealerInfo m_damageDealer;
    private Transform m_parent;

    public override void Awake()
    {
        base.Awake();
        m_parent = transform.parent;
        m_collider = GetComponent<BoxCollider2D>();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override void OnDeadFinish()
    {
        // base.OnDeadFinish();
        Destroy(gameObject);
    }
    public override void Dead()
    {
        base.Dead();
        StartCoroutine(DelayDropItem());
        InitEffectBroken();
    }
    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        m_damageDealer = attackerInfor;
        base.OnHit(attackerInfor);
    }

    private void Start() 
    {
        spine.AnimationState.SetAnimation(0, "idle", true); 
    }

    IEnumerator DelayDropItem()
    {
        yield return new WaitForSeconds(m_brokenDelay);
        int rand = Random.Range(0, m_audiosBreak.Length);
        SoundManager.PlaySound(m_audiosBreak[rand], false);
        m_collider.enabled = false;
        DropItem();
    }

    void DropItem()
    {
        if(m_rewards.Length == 0)
            return;
        foreach(CrackingChestReward reward in m_rewards)
        {
            if(reward.value <= 0)
                continue;
            int rand = Random.Range(0, 100);
            if(rand > reward.spawnRate)
                continue;
            GameObject objToSpawn;
            switch(reward.rewardType)
            {
                case CrackingChestRewardType.Coin:
                    objToSpawn = m_coinObj;
                    break;
                case CrackingChestRewardType.Heart:
                    objToSpawn = m_heartObj;
                    break;
                case CrackingChestRewardType.HealPotionMini:
                    objToSpawn = m_healPotionMini;
                    break;
                case CrackingChestRewardType.HealPotionLarge:
                    objToSpawn = m_healPotionLarge;
                    break;
                default:
                    objToSpawn = m_coinObj;
                    break;
            }
            for(int i = 0; i < reward.value; i++)
            {
                GameObject go = Instantiate(objToSpawn, transform.position + new Vector3(0, 1, 0), Quaternion.identity, m_parent);
                go.SetActive(true);
                Vector2 targetForce = new Vector2(Random.Range(-m_force.x, m_force.x), m_force.y);
                go.GetComponent<Rigidbody2D>().AddForce(targetForce, ForceMode2D.Impulse);
            }
        }
    }

    void InitEffectBroken()
    {
        GameObject go = Instantiate(m_effectBroken);
        go.transform.SetParent(transform.parent, false);
        go.transform.position = m_effectBroken.transform.position;
        go.transform.localScale = Vector3.one;
        if(m_damageDealer.attacker.position.x > transform.position.x)
            go.transform.eulerAngles = new Vector3(0, 0, 90);
        go.SetActive(true);
    } 
}
