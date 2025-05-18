using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class ObjectCrackingBox : MonoBehaviour
{
    [SerializeField] private CrackingChestReward[] m_rewards;
    [SerializeField] private Vector2 m_force;
    [SerializeField] private float m_brokenDelay = 0.02f;

    [Header("References")]
    [SerializeField] private SkeletonAnimation m_spine;
    [SerializeField] private GameObject m_coinObj;
    [SerializeField] private GameObject m_heartObj;

    [Header("Audio")]
    [SerializeField] private AudioClip[] m_audiosBreak;

    private bool m_broken = false;
    private BoxCollider2D m_collider;

    private void Start() 
    {
        m_collider = GetComponent<BoxCollider2D>();

        m_spine.AnimationState.SetAnimation(0, "idle", true); 
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if(m_broken)
            return;
        if(other.gameObject.tag != GameTag.PLAYER)
            return;
        StartCoroutine(DelayDropItem());
    }

    IEnumerator DelayDropItem()
    {
        yield return new WaitForSeconds(m_brokenDelay);
        m_broken = true;
        int rand = Random.Range(0, m_audiosBreak.Length);
        SoundManager.PlaySound(m_audiosBreak[rand], false);
        m_collider.enabled = false;
        m_spine.AnimationState.SetAnimation(0, "broken2", false);
        DropItem();
        Destroy(gameObject, 1f);
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
                default:
                    objToSpawn = m_coinObj;
                    break;
            }
            for(int i = 0; i < reward.value; i++)
            {
                GameObject go = Instantiate(objToSpawn, transform.position + new Vector3(0, 1, 0), Quaternion.identity, transform.parent);
                go.SetActive(true);
                Vector2 targetForce = new Vector2(Random.Range(-m_force.x, m_force.x), m_force.y);
                go.GetComponent<Rigidbody2D>().AddForce(targetForce);
            }
        }
    }
}

[System.Serializable]
public class CrackingChestReward
{

    public CrackingChestRewardType rewardType;
    public int value;
    [Range(0, 100)]
    public int spawnRate;
}

