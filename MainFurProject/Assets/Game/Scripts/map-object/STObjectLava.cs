using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectLava : MonoBehaviour
{
    [SerializeField] private float m_delayKillTime = 2f;
    [SerializeField] AudioClip m_audioSank;
    [Min(0.1f)]
    [SerializeField] private float m_velDownY = 1f;

    [Header("Only work for player")]
    [SerializeField] bool m_isDragingByHand = false;

    [Header("Drag down configs")]
    [Min(0)]
    [SerializeField] private int m_numOfDrag = 4;
    [Min(0)]
    [SerializeField] private float m_velocityDownYPerDrag = 5;


    private bool m_ready = true;
    private Transform m_player;
    private float m_playerMass;
    private bool m_killing = false;
    private Coroutine m_sankRoutine;
    private Coroutine m_onDraginRoutine;
    private DamageDealerInfo m_damageDealerInfor;
    private List<STObjectMonster> m_killedMonster;

    private void Start()
    {
        m_killedMonster = new List<STObjectMonster>(); ;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null || rb.bodyType == RigidbodyType2D.Static)
            return;

        SoundManager.PlaySound3D(m_audioSank, 10, false, other.transform.position);
        switch (other.tag)
        {
            case GameTag.PLAYER:
                if (!m_killing)
                {
                    if(other.offset.y < 0.5f || STGameModel.hp <= 0)
                        return;
                    m_killing = true;
                    m_player = other.transform;
                    m_playerMass = rb.mass;
                    rb.gravityScale = 0.5f;
                    rb.velocity = new Vector3(0, -m_velDownY, 0);
                    StartCoroutine(IPlayerDead());
                }
                break;
            case GameTag.OBJECT_BOX:
                Rigidbody2D rigid = other.GetComponent<Rigidbody2D>();
                if (rigid != null)
                    rigid.mass = 40;
                break;
            default:
                STObjectMonster monster = other.GetComponent<STObjectMonster>();
                if (monster && !m_killedMonster.Contains(monster))
                {
                    m_killedMonster.Add(monster);
                    StartCoroutine(IMonsterDead(monster));
                }
                break;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null || rb.bodyType == RigidbodyType2D.Static)
            return;
        if (other.tag == GameTag.PLAYER && !m_killing)
        {
            if(other.offset.y < 0.5f || STGameModel.hp <= 0)
                return;
            StopAllCoroutines();
            rb.mass = m_playerMass;
            m_player = null;
            GameController.ActiveInput(true);
            rb.gravityScale = 5;
        }
        else
        {
            SoundManager.PlaySound3D(m_audioSank, 10, false, other.transform.position);
        }
    }

    DamageDealerInfo InitNewDamageDealerInfor(int damage)
    {
        DamageDealerInfo info = new DamageDealerInfo();
        info.damage = damage;
        info.attacker = transform;
        return info;
    }

    void CustomStopCoroutines()
    {
        if (m_sankRoutine != null)
            StopCoroutine(m_sankRoutine);
        if (m_onDraginRoutine != null)
            StopCoroutine(m_onDraginRoutine);
    }

    IEnumerator IPlayerDead()
    {
        CustomStopCoroutines();
        if (m_isDragingByHand)
            m_onDraginRoutine = StartCoroutine(IPlayerDragDown());
        m_sankRoutine = StartCoroutine(IPlayerSank());
        GameController.ActiveInput(false);
        yield return new WaitForSeconds(m_delayKillTime);
        m_player.GetComponent<Rigidbody2D>().gravityScale = 5;
        STGameController.UpdatePlayerImmunity(false, false);
        STGameController.HitPlayer(InitNewDamageDealerInfor(STGameModel.hp));
        m_killing = false;
        CustomStopCoroutines();
    }
    IEnumerator IPlayerSank()
    {
        yield return new WaitForSeconds(m_delayKillTime);
        Rigidbody2D rigid = m_player.GetComponent<Rigidbody2D>();
        while (m_player != null)
        {
            if(STGameModel.hp > 0)
                STGameController.HitPlayer(InitNewDamageDealerInfor(STGameModel.hp));
            if (rigid != null)
                rigid.mass += 0.5f;
            yield return null;
        }
    }

    IEnumerator IPlayerDragDown()
    {
        Rigidbody2D rigid = m_player.GetComponent<Rigidbody2D>();
        float timeBetweenDrag = m_delayKillTime / m_numOfDrag;
        while (m_player != null)
        {
            yield return new WaitForSeconds(timeBetweenDrag);
            Vector2 downVel = rigid.velocity;
            downVel.y -= m_velocityDownYPerDrag;
            rigid.velocity = downVel;
            yield return new WaitForEndOfFrame();
            rigid.velocity = new Vector3(0, -m_velDownY, 0);
        }
    }
    IEnumerator IMonsterDead(STObjectMonster monster)
    {
        monster.startBehaviour = false;
        monster.GetComponentInChildren<SkeletonAnimation>().AnimationState.TimeScale = 0;
        Rigidbody2D monsterBody = monster.GetComponent<Rigidbody2D>();
        if(monsterBody)
        {

            monsterBody.gravityScale = 0.5f;
            monsterBody.velocity = new Vector3(0, -1, 0);
            StartCoroutine(IMonsterSank(monsterBody));
        }
        yield return new WaitForSeconds(2f);
        monster.knockbackStrength = 0;
        monster.GetComponentInChildren<SkeletonAnimation>().AnimationState.TimeScale = 1;
        yield return null;
        monster.gameObject.SendMessage("OnHit", InitNewDamageDealerInfor((int)monster.currentHP + 1), SendMessageOptions.DontRequireReceiver);
    }
    IEnumerator IMonsterSank(Rigidbody2D monsterBody)
    {
        while (monsterBody != null)
        {
            monsterBody.mass += 0.05f;
            yield return null;
        }
    }
}
