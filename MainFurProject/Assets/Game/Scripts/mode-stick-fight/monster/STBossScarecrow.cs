using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using DG.Tweening;
using Spine.Unity.Examples;

public class STBossScarecrow : STObjectBoss
{
    [SerializeField] private float m_moveSpeed;
    [SerializeField] private int m_scytheDamage = 20;
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_maxMonsterSpawn = 3;
    [SerializeField] private float m_idleTime = 2f;

    [SerializeField] private GameObject m_childPortal;
    [SerializeField] private GameObject m_portal;
    [SerializeField] private GameObject m_virtualCamera;
    [SerializeField] private Collider2D m_detectArea;
    [SerializeField] private Collider2D m_attackArea;
    [SerializeField] private BoxCollider2D m_summonArea;
    [SerializeField] private ContactFilter2D m_playerLayer;
    [SerializeField] private GameObject m_boyPrefab;
    [SerializeField] private SkeletonGhost m_skeletonGhost;
    [SerializeField] private int m_maxGhostDash = 5;
    [SerializeField] private int m_maxGhostAttack = 15;
    [SerializeField] private AudioClip m_audioCut;
    [SerializeField] private AudioClip m_audioSummon;

    private Vector2 m_baseScale;
    private Vector2 m_portalScale;
    private Vector2 m_spineScale;
    private int m_direction;
    private List<GameObject> m_portals;
    private List<GameObject> m_currentMonsters;
    private int m_currentSkillIndex;
    private int m_firstSkillCounter;
    private GameObject m_body;
    private BoxCollider2D m_collider;
    private float m_baseSpeed;
    private bool m_start;
    private BodyState m_currentState = BodyState.Idle;

    private const string ANIM_IDLE = "idle";
    private const string ANIM_WALK = "walk";
    private const string ANIM_ATTACK = "attack";
    private const string ANIM_DEAD = "dead";
    private const string ANIM_DASH = "dash";
    private const string ANIM_SKILL_1 = "skill_1";
    private enum BodyState{Idle, Move, Attack}

    public override void Awake()
    {
        base.Awake();

        m_baseSpeed = m_moveSpeed;
        m_body = transform.Find("body").gameObject;
        m_portals = new List<GameObject>();
        m_currentMonsters = new List<GameObject>();
        m_collider = GetComponent<BoxCollider2D>();
        SetupAllDamager();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();

        spine.AnimationState.Complete -= OnAnimComplete;
        spine.AnimationState.Event -= OnAnimEvent;
    }
    public override void OnResumeAfterHit()
    {
        
    }
    public override void Dead()
    {
        base.Dead();
        StopAllCoroutines();
        m_skeletonGhost.ghostingEnabled = false;

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
        m_baseScale = Vector3.one;
        m_portalScale = m_portal.transform.localScale;
        m_spineScale = spine.transform.localScale;
        m_direction = m_baseScale.x > 0 ? 1 : -1;
        spine.transform.localScale = Vector3.zero;
        m_portal.transform.localScale = Vector3.zero;
        m_summonArea.transform.SetParent(transform.parent);
        m_skeletonGhost.ghostingEnabled = false;

        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;
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
        if(m_currentState != BodyState.Move)
            return;
        bool needToFlip = (myRigidbody.position.x < (player.transform.position.x - 1)) || (myRigidbody.position.x > (player.transform.position.x + 1));
        if (needToFlip)
            LookAtPlayer();
        Vector2 moveDirection = m_direction > 0 ? Vector2.right : Vector2.left;
        myRigidbody.velocity = moveDirection * m_moveSpeed;
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        if(isDead)
            return;
        switch(trackEntry.Animation.Name)
        {
            case ANIM_ATTACK:
                m_detectArea.gameObject.SetActive(false);
                m_skeletonGhost.ghostingEnabled = false;
                if(m_firstSkillCounter == 2)
                    StartCoroutine(ScheduleSecondSkill());
                else
                    Move(ANIM_DASH);
                break;
            case ANIM_SKILL_1:
                StartCoroutine(ScheduleFirstSkill());
                break;
        }
    }
    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if(isDead)
            return;
        switch(trackEntry.Animation.Name)
        {
            case ANIM_ATTACK:
                m_skeletonGhost.ghostingEnabled = true;
                SoundManager.PlaySound(m_audioCut, false);
                HitPlayer();
                break;
            case ANIM_SKILL_1:
                SpawnMonster();
                break;
        }
    }

    void FirstSkill()
    {
        StopMove();
        m_firstSkillCounter++;
        m_currentState = BodyState.Attack;
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK, false);
        m_skeletonGhost.spawnInterval = 0.001f;
        // m_skeletonGhost.ghostingEnabled = false;
    }
    IEnumerator ScheduleSecondSkill()
    {
        StopMove();
        m_currentSkillIndex = 2;
        LookAtPlayer();
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        m_currentState = BodyState.Attack;
        yield return new WaitForSeconds(m_idleTime);
        SecondSkill();
    }
    void SecondSkill()
    {
        SoundManager.PlaySound(m_audioSummon, false);
        spine.AnimationState.SetAnimation(0, ANIM_SKILL_1, false);
    }
    void StartThirdSkill()
    {

    }
    void ThirdSkill()
    {

    }
    void StartFourthSkill()
    {

    }
    void FourthSkill()
    {

    }
    void SetupAllDamager()
    {
        m_body.GetComponent<STObjectDealDamage>().UpdateDamage(m_bodyDamage);
    }
    void LookAtPlayer()
    {
        m_direction = transform.position.x > player.transform.position.x ? -1 : 1;
        transform.localScale = new Vector3(m_baseScale.x * m_direction, m_baseScale.y, 1);
    }
    void StopMove()
    {
        myRigidbody.velocity = Vector2.zero;
        m_currentState = BodyState.Idle;
    }
    void Move(string animMove)
    {
        if(m_currentState == BodyState.Move)
            return;
        m_currentState = BodyState.Move;
        spine.AnimationState.SetAnimation(0, animMove, true);
        m_detectArea.gameObject.SetActive(true);
        m_skeletonGhost.ghostingEnabled = true;
        m_skeletonGhost.spawnInterval = 0.1f;
    }
    void SpawnMonster()
    {
        List<int> choosenPos = new List<int>();
        for(int i = 0; i < m_maxMonsterSpawn; i++)
        {
            Vector2 spawnPos = RandomPointInBounds(m_summonArea.bounds);
            GameObject monster = Instantiate(m_boyPrefab, spawnPos, Quaternion.identity, transform.parent);
            monster.SetActive(false);
            STObjectMonster monsterComp = monster.GetComponent<STObjectMonster>();
            monsterComp.PauseBehaviour();
            m_currentMonsters.Add(monster);
            STMonsterScarecrow monsterScarecrow = monster.GetComponent<STMonsterScarecrow>();
            if(monsterScarecrow)
            {
                monsterScarecrow.Boss = this;
            }

            GameObject portal = Instantiate(m_childPortal, spawnPos + Vector2.up, Quaternion.identity, transform.parent);
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
    Vector2 RandomPointInBounds(Bounds bounds) 
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            transform.position.y
        );
    }

    public void ReachPlayer()
    {
        if(m_currentState == BodyState.Attack || isDead)
            return;
        FirstSkill();
    }
    public void HitPlayer()
    {
        List<Collider2D> results = new List<Collider2D>();
        m_attackArea.OverlapCollider(m_playerLayer, results);
        for(int i = 0; i < results.Count; i++)
        {
            if(results[i].tag == GameTag.PLAYER)
            {
                DamageDealerInfo damageDealerInfor = new DamageDealerInfo();
                damageDealerInfor.damage = m_scytheDamage;
                damageDealerInfor.attacker = transform;
                STGameController.HitPlayer(damageDealerInfor);
            }
        }
    }

    IEnumerator ScheduleFirstSkill(bool firstTime = true)
    {
        m_currentSkillIndex = 1;
        m_firstSkillCounter = 0;
        yield return null;
        Move(ANIM_DASH);
    }
    IEnumerator IDelayDestroyPortal()
    {
        yield return new WaitForSeconds(2.2f);
        foreach(GameObject portal in m_portals)
        {
            Destroy(portal);
        }
    }
}
