using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine;
using Spine.Unity;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class STBossStick : STObjectBoss
{
    private enum BodyState{Normal, Stun}
    private const string ANIM_IDLE = "idle";
    private const string ANIM_IDLE2 = "idle2";
    private const string ANIM_ATTACK1 = "attack1";
    private const string ANIM_ATTACK2 = "attack2";
    private const string ANIM_ATTACK3 = "attack3";
    private const string ANIM_DEAD = "dead";
    private const string ANIM_RUN = "run";

    [SerializeField] private float m_moveSpeed = 5f;
    [SerializeField] private float m_stunTime = 5;
    [SerializeField] private float m_bulletSpeed = 10f;
    [SerializeField] private int m_damage = 10;
    [SerializeField] private int m_smallBulletDamamge = 10;
    [SerializeField] private int m_bigBulletDamage = 20;
    [SerializeField] private int m_waveBulletDamage = 10;
    [SerializeField] private int m_monsterScoutAmount = 3;
    [SerializeField] private int m_monsterPlinkyAmount = 6;

    [SerializeField] private GameObject m_portal;
    [SerializeField] private GameObject m_virtualCamera;
    [SerializeField] private Transform m_shotPoint;
    [SerializeField] private GameObject m_smallBullet;
    [SerializeField] private GameObject m_bigBullet;
    [SerializeField] private STObjectDealDamage m_waveEffect;
    [SerializeField] private float m_firstSkillDuration = 3f;
    [SerializeField] private GameObject m_childPortal;
    [SerializeField] private List<GameObject> m_monsterList;
    [SerializeField] private GameObject m_magicLine;

    [SerializeField] private AudioClip m_audioShoot;

    public Vector2[] monsterSpawnPostitions;
    public Vector2 leftPos;
    public Vector2 rightPos;
    public Vector2 topPos;
    public Vector2 bottomPos;
    public Vector2 upperPos;
    public Vector2 lowerPos;

    private BodyState m_bodyState;
    private Vector2 m_baseScale;
    private Vector2 m_portalScale;
    private Vector2 m_spineScale;
    private int m_direction;
    private bool m_start;
    private Vector2? m_destination;
    private Vector2 m_moveDirection;
    private List<GameObject> m_portals;
    private List<GameObject> m_currentMonsters;
    private List<GameObject> m_magicLines;
    private int m_currentSkillIndex;
    private Tweener m_fadeTween;
    private int m_firstSkillCounter;
    private int m_thirdSkillCounter;
    private int m_fifthSkillCounter;
    private float m_baseSpeed;
    private GameObject m_body;
    private Coroutine m_delayAfterShootRoutine;
    private CircleCollider2D m_collider;

    private Vector2[] m_fourthSkillDestination;

    public override void Awake()
    {
        base.Awake();

        transform.position = rightPos;
        m_baseSpeed = m_moveSpeed;
        m_body = transform.Find("body").gameObject;
        m_portals = new List<GameObject>();
        m_currentMonsters = new List<GameObject>();
        m_magicLines = new List<GameObject>();
        m_collider = GetComponent<CircleCollider2D>();
        SetupAllDamager();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();

        spine.AnimationState.Complete -= OnAnimComplete;
        spine.AnimationState.Event -= OnAnimEvent;

        GameController.monsterDeadEvent -= OnMonsterDead;
    }
    public override void OnResumeAfterHit()
    {
        
    }
    public override void Dead()
    {
        base.Dead();
        StopMove();
        StopAllCoroutines();
        // PlayerPrefs.SetInt(DataKey.FIRST_BOSS_KILLED, 1);

        foreach(GameObject monster in m_currentMonsters)
        {
            Destroy(monster);
        }
        foreach(GameObject portal in m_portals)
        {
            Destroy(portal);
        }
    }
    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        Destroy(gameObject);
    }
    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        base.OnHit(attackerInfor);
        GameController.UpdateBossHp((int)maxHP, (int)currentHP);
    }

    public override void OnAppear()
    {
        StartCoroutine(IVisible());
    }

    public override void OnReadyPlay()
    {
        base.OnReadyPlay();
    }
    public override void OnReady()
    {
        base.OnReady();
    }
    public override void Init()
    {
        m_baseScale = transform.localScale;
        m_portalScale = m_portal.transform.localScale;
        m_spineScale = spine.transform.localScale;
        m_direction = m_baseScale.x > 0 ? -1 : 1;
        spine.transform.localScale = Vector3.zero;
        m_portal.transform.localScale = Vector3.zero;

        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;

        GameController.monsterDeadEvent += OnMonsterDead;
    }
    public override void StartBoss()
    {
        m_start = true;
        StartCoroutine(ScheduleFirstSkill());
    }

    IEnumerator IVisible()
    {
        m_virtualCamera.SetActive(true);
        yield return new WaitForSeconds(1f);
        m_portal.transform.DOScale(m_portalScale, 0.5f);
        yield return new WaitForSeconds(1f);
        StartCoroutine(IDelayStart());
    }
    IEnumerator IDelayStart()
    {
        spine.transform.localScale = m_spineScale;
        TrackEntry entry = spine.AnimationState.SetAnimation(0, ANIM_IDLE, false);
        yield return new WaitForSeconds(entry.AnimationEnd);
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        m_portal.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
        {
            m_portal.gameObject.SetActive(false);
        });
        m_virtualCamera.SetActive(false);
        yield return new WaitForSeconds(2f);
        GameController.BossReady();
    }

    private void FixedUpdate() 
    {
        if(isDead || !m_start)
            return;    
        if(m_destination == null)
            return;
        myRigidbody.velocity = m_moveDirection * m_moveSpeed;
        if(Vector3.Distance(transform.position, m_destination.Value) <= (0.2f * (m_moveSpeed / m_baseSpeed)))
        {
            ReachDestination();
        }
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        if(isDead)
            return;
        switch(trackEntry.Animation.Name)
        {
            case ANIM_ATTACK1:
                if(m_currentSkillIndex == 1 && m_firstSkillCounter < 2)
                    FirstSkill();
                else if(m_currentSkillIndex == 3 && m_thirdSkillCounter < 2)
                    ThirdSkill();
                else
                    m_delayAfterShootRoutine = StartCoroutine(IDelayMoveAfterShoot());
                break;
            case ANIM_ATTACK3:
                if(m_currentSkillIndex == 2)
                    NextBehaviour(null, null);
                else if(m_currentSkillIndex == 4)
                    NextBehaviour(null, null);
                break;
        }
    }
    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if(isDead)
            return;
        switch(trackEntry.Animation.Name)
        {
            case ANIM_ATTACK1:
                Vector2 directionToPlayer = (player.transform.position - m_shotPoint.position).normalized;
                Shoot(directionToPlayer, m_bulletSpeed, m_currentSkillIndex == 1 ? m_smallBullet : m_bigBullet);
                break;
            case ANIM_ATTACK3:
                if(m_currentSkillIndex == 2)
                {
                    SpawnMonster(0, m_monsterScoutAmount);
                }
                else if(m_currentSkillIndex == 4)
                {
                    SpawnMonster(1, m_monsterPlinkyAmount);
                }
                break;
        }
    }
    void OnMonsterDead(GameObject monster)
    {
        if(isDead)
            return;
        if(!m_currentMonsters.Contains(monster))
            return;
        m_currentMonsters.Remove(monster);
        if(m_currentMonsters.Count == 0 && m_currentSkillIndex == 2)
        {
            Fade(1, 0.5f, 0, null);
            m_collider.enabled = true;
            NextBehaviour(() => MoveTo(bottomPos, 0.7f), StartThirdSkill);
            StartCoroutine(IStun());
        }
    }

    void FirstSkill()
    {
        m_firstSkillCounter++;
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK1, false);
    }
    void StartSecondSkill()
    {
        m_currentSkillIndex = 2;
        MoveTo(topPos);
    }
    void SecondSkill()
    {
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK3, false);
        m_collider.enabled = false;
        Fade(0.5f, 0.5f, 0, null);
    }
    void StartThirdSkill()
    {
        m_currentSkillIndex = 3;
        m_thirdSkillCounter = 0;
        bool chooseLeftPos = Random.Range(0, 100) > 50;
        MoveTo(chooseLeftPos ? leftPos : rightPos);
    }
    void ThirdSkill()
    {
        m_thirdSkillCounter++;
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK1, false);
    }
    void StartFourthSkill()
    {
        m_currentSkillIndex = 4;
        MoveTo(topPos);
    }
    void FourthSkill()
    {
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK3, false);
    }
    void StartFifthSkill()
    {
        m_fifthSkillCounter = 0;
        m_currentSkillIndex = 5;
        FifthSkill();
    }
    void FifthSkill()
    {
        bool startUpper = player.transform.position.y > leftPos.y;
        transform.position = startUpper ? upperPos : lowerPos;
        Vector2 target = new Vector2(startUpper ? lowerPos.x : upperPos.x, transform.position.y);
        UpdateDirection(startUpper ? -1 : 1);
        Fade(1, 1f, 1f, () => MoveTo(target, 2.5f));
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
    }
    IEnumerator IStun()
    {
        m_body.SetActive(false);
        m_bodyState = BodyState.Stun;
        spine.AnimationState.SetAnimation(0, ANIM_IDLE2, true);
        yield return new WaitForSeconds(m_stunTime);
        m_body.SetActive(true);
        m_bodyState = BodyState.Normal;
        NextBehaviour(() => StartCoroutine(ScheduleFirstSkill(false)), StartThirdSkill);
    }
    void StopMove()
    {
        MoveTo(null);
        myRigidbody.velocity = Vector2.zero;
    }
    void Shoot(Vector2 direction, float speed, GameObject bullet)
    {
        GameObject go = Instantiate(bullet, m_shotPoint.position, Quaternion.identity, transform.parent);
        go.SetActive(true);
        
        DamageDealerInfo damageDealerInfo = new DamageDealerInfo();
        damageDealerInfo.damage = bullet == m_smallBullet ? m_smallBulletDamamge : m_bigBulletDamage;
        damageDealerInfo.attacker = go.transform;
        
        go.GetComponent<STEnemyBulletAttackable>().Init(direction, speed, damageDealerInfo);
        SoundManager.PlaySound(m_audioShoot, false);
    }
    void SpawnMonster(int monsterIndex, int amount)
    {
        m_currentMonsters.Clear();
        m_magicLines.Clear();
        amount = Mathf.Min(amount, monsterSpawnPostitions.Length);
        GameObject monsterToSpawn = m_monsterList[monsterIndex];
        List<Vector2> cacheSpawnPositions = monsterSpawnPostitions.ToList();
        List<int> choosenPos = new List<int>();
        for(int i = 0; i < amount; i++)
        {
            Vector2 spawnPos;
            int randPosIndex = 0;
            do
            {
                randPosIndex = Random.Range(0, cacheSpawnPositions.Count);
            }while(choosenPos.Contains(randPosIndex));
            choosenPos.Add(randPosIndex);
            spawnPos = monsterSpawnPostitions[randPosIndex];

            GameObject monster = Instantiate(monsterToSpawn, spawnPos, Quaternion.identity, transform.parent);
            monster.SetActive(false);
            STObjectMonster monsterComp = monster.GetComponent<STObjectMonster>();
            monsterComp.PauseBehaviour();
            m_currentMonsters.Add(monster);

            if(monsterIndex == 0) // only monster scout has magic line
                SpawnMagicLine(center, monster.transform);

            GameObject portal = Instantiate(m_childPortal, spawnPos, Quaternion.identity, transform.parent);
            portal.SetActive(true);
            portal.transform.localScale = Vector3.zero;
            m_portals.Add(portal);

            portal.transform.DOScale(Vector3.one, 1).OnComplete(() => {
                    monster.SetActive(true);
                    Vector3 baseScale = monster.transform.localScale;
                    monster.transform.localScale = Vector3.zero;

                    monster.transform.DOScale(baseScale, 1).OnComplete(() => 
                    {
                        monsterComp.StartBehaviour();
                    });
                });
        }
        StartCoroutine(IDelayDestroyPortal());
    }
    void Fade(float targetAlpha, float fadeTime, float delayTime, System.Action callback)
    {
        m_fadeTween?.Kill();

        Color temp = Color.white;
        temp.a = spine.skeleton.GetColor().a;

        m_fadeTween = DOTween.To(() => temp.a, x => temp.a = x, targetAlpha, fadeTime).SetDelay(delayTime).OnUpdate(() => {
            spine.skeleton.SetColor(temp);
        }).OnComplete(() => {
            if(!isDead)
                callback?.Invoke();
        });
    }
    void SpawnMagicLine(Transform start, Transform end)
    {
        GameObject magicLine = Instantiate(m_magicLine, transform.parent);
        magicLine.SetActive(true);
        magicLine.GetComponent<STObjectMagicLine>().Init(start, end);
        m_magicLines.Add(magicLine);
    }
    void UpdateDirection(int direction)
    {
        m_direction = direction;
        transform.localScale = new Vector3(m_baseScale.x * m_direction, m_baseScale.y, 1);
    }

    IEnumerator ScheduleFirstSkill(bool firstTime = true)
    {
        m_currentSkillIndex = 1;
        m_firstSkillCounter = 0;
        if(!firstTime)
        {
            bool chooseLeftPos = Random.Range(0, 100) > 50;
            MoveTo(chooseLeftPos ? leftPos : rightPos);
            yield break;
        }
        yield return new WaitForSeconds(m_firstSkillDuration);
        FirstSkill();
    }
    IEnumerator IDelayDestroyPortal()
    {
        yield return new WaitForSeconds(2.2f);
        foreach(GameObject portal in m_portals)
        {
            Destroy(portal);
        }
        yield return null;
        if(m_currentSkillIndex == 4)
            Fade(0, 2f, 0, () => NextBehaviour(null, StartFifthSkill));
    }

    void SetupAllDamager()
    {
        m_waveEffect.UpdateDamage(m_waveBulletDamage);
        m_body.GetComponent<STObjectDealDamage>().UpdateDamage(m_damage);
    }
    void MoveTo(Vector2? des, float moveSpeedRatio = 1)
    {
        m_destination = des;
        if(m_destination != null)
        {
            m_moveSpeed = m_baseSpeed * moveSpeedRatio;
            m_moveDirection = (m_destination.Value - (Vector2)transform.position).normalized;
            if(des != bottomPos) // to stun
                spine.AnimationState.SetAnimation(0, ANIM_RUN, true);
            UpdateDirection(transform.position.x > des.Value.x ? 1 : -1);
        }
    }
    void ReachDestination()
    {
        if(isDead)
            return;
        if(m_destination == leftPos || m_destination == rightPos)
        {
            UpdateDirection(m_destination == leftPos ? -1 : 1);
        }
        else if(m_destination == bottomPos)
        {
            StopMove();
            return;
        }

        switch(m_currentSkillIndex)
        {
            case 1:
                FirstSkill();
                break;
            case 2:
                SecondSkill();
                break;
            case 3:
                ThirdSkill();
                break;
            case 4:
                FourthSkill();
                break;
            case 5:
                m_fifthSkillCounter++;
                if(m_fifthSkillCounter == 1)
                    Fade(0, 0.5f, 0, () => NextBehaviour(null, FifthSkill));
                else
                    Fade(0, 0.5f, 0, AppearAfterFifthSkill);
                break;
        }
        StopMove();
    }
    void AppearAfterFifthSkill()
    {
        transform.position = rightPos;
        UpdateDirection(1);
        Fade(1, 0.5f, 0, StartThirdSkill);
    }
    void NextBehaviour(System.Action firstPhaseAction, System.Action secondPhaseAction)
    {
        if(isDead)
            return;
        if(IsSecondPhase())
        {
            secondPhaseAction?.Invoke();
            if(secondPhaseAction == null)
                spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        }   
        else
        {
            firstPhaseAction?.Invoke();
            if(firstPhaseAction == null)
                spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        }
    }
    IEnumerator IDelayMoveAfterShoot()
    {
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        yield return new WaitForSeconds(2f);
        if(m_currentSkillIndex == 1)
            NextBehaviour(StartSecondSkill, StartThirdSkill);
        else if(m_currentSkillIndex == 3)
            NextBehaviour(StartFourthSkill, StartFourthSkill);
    }

    bool IsSecondPhase()
    {
        return m_currentHP <= (maxHP * 0.4f); // hard-code
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if(monsterSpawnPostitions.Length > 0)
        {
            Gizmos.color = Color.blue;
            foreach(Vector2 spawnPos in monsterSpawnPostitions)
            {
                Gizmos.DrawSphere(spawnPos, 0.5f);
            }
        }
        Gizmos.color = Color.red;
        Handles.Label(leftPos + Vector2.up, "LEFT");
        Gizmos.DrawWireSphere(leftPos, 0.5f);
        Handles.Label(rightPos + Vector2.up, "RIGHT");
        Gizmos.DrawWireSphere(rightPos, 0.5f);
        Handles.Label(topPos + Vector2.up, "TOP");
        Gizmos.DrawWireSphere(topPos, 0.5f);
        Handles.Label(bottomPos + Vector2.up, "BOTTOM");
        Gizmos.DrawWireSphere(bottomPos, 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center.position, visibleRange);

        Gizmos.color = Color.yellow;
        Handles.Label(upperPos + Vector2.up, "UPPER");
        Gizmos.DrawWireSphere(upperPos, 0.5f);
        Handles.Label(lowerPos + Vector2.up, "LOWER");
        Gizmos.DrawWireSphere(lowerPos, 0.5f);
    }
#endif
}
