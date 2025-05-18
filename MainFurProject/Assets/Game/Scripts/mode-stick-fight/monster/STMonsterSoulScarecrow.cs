using Spine;
using Spine.Unity.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STMonsterSoulScarecrow : STObjectMonster
{
    private const string ANIM_IDLE = "idle";
    private const string ANIM_WALK = "walk";
    private const string ANIM_ATTACK = "attack";
    private const string ANIM_DEAD = "dead";
    private const string ANIM_DASH = "dash";

    private enum BodyState { Idle, Move, Attack }

    [SerializeField] private float m_moveSpeed;
    [SerializeField] private int m_scytheDamage = 20;
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private float m_idleTime = 2f;

    [SerializeField] private Collider2D m_detectArea;
    [SerializeField] private Collider2D m_attackArea;
    [SerializeField] private ContactFilter2D m_playerLayer;
    [SerializeField] private SkeletonGhost m_skeletonGhost;
    [SerializeField] private int m_maxGhostDash = 5;
    [SerializeField] private int m_maxGhostAttack = 15;
    [SerializeField] private GameObject m_activeObject;
    [SerializeField] private AudioClip m_audioCut;

    private BodyState m_currentState = BodyState.Idle;
    private Vector2 m_baseScale;
    private Vector2 m_spineScale;
    private int m_direction;
    private int m_currentSkillIndex;
    private int m_firstSkillCounter;
    private float m_baseSpeed;
    private GameObject m_body;
    private BoxCollider2D m_collider;

    public override void Awake()
    {
        base.Awake();

        m_baseSpeed = m_moveSpeed;
        m_body = transform.Find("body").gameObject;
        m_collider = GetComponent<BoxCollider2D>();
        SetupAllDamager();
    }

    private void Start()
    {
        m_baseScale = Vector3.one;
        m_spineScale = spine.transform.localScale;
        m_direction = m_baseScale.x > 0 ? 1 : -1;
        m_skeletonGhost.ghostingEnabled = false;

        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        spine.AnimationState.Complete -= OnAnimComplete;
        spine.AnimationState.Event -= OnAnimEvent;
    }

    private void Update()
    {
        if (isDead)
            return;
        if (m_currentState != BodyState.Move)
            return;
        bool needToFlip = (myRigidbody.position.x < (player.transform.position.x - 1)) || (myRigidbody.position.x > (player.transform.position.x + 1));
        if (needToFlip)
            LookAtPlayer();
        Vector2 moveDirection = m_direction > 0 ? Vector2.right : Vector2.left;
        myRigidbody.velocity = moveDirection * m_moveSpeed;
    }

    public override void OnResumeAfterHit()
    {

    }

    public override void Dead()
    {
        StopAllCoroutines();
        base.Dead();
        StopMove();
        m_body.SetActive(false);
    }

    public override void OnDeadFinish()
    {
        StopMove();
        base.OnDeadFinish();
        Destroy(gameObject);
    }

    public override void StartBehaviour()
    {
        base.StartBehaviour();
        LookAtPlayer();
        m_currentState = BodyState.Idle;
        StartCoroutine(ScheduleFirstSkill());
        if(m_activeObject != null)
            Destroy(m_activeObject);
    }

    // ---------------------------- public method -------------------------- //
    public void ReachPlayer()
    {
        if (m_currentState == BodyState.Attack || isDead)
            return;
        FirstSkill();
    }

    // ---------------------------- private method -------------------------- //
    private void SetupAllDamager()
    {
        m_body.GetComponent<STObjectDealDamage>().UpdateDamage(m_bodyDamage);
    }

    private void LookAtPlayer()
    {
        m_direction = transform.position.x > player.transform.position.x ? -1 : 1;
        transform.localScale = new Vector3(m_baseScale.x * m_direction, m_baseScale.y, 1);
    }

    private void StopMove()
    {
        myRigidbody.velocity = Vector2.zero;
        m_currentState = BodyState.Idle;
    }

    private void Move(string animMove)
    {
        if (m_currentState == BodyState.Move || isDead)
            return;
        m_currentState = BodyState.Move;
        spine.AnimationState.SetAnimation(0, animMove, true);
        m_detectArea.gameObject.SetActive(true);
        m_skeletonGhost.ghostingEnabled = true;
        m_skeletonGhost.spawnInterval = 0.1f;
    }

    private void FirstSkill()
    {
        StopMove();
        m_firstSkillCounter++;
        m_currentState = BodyState.Attack;
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK, false);
        m_skeletonGhost.spawnInterval = 0.001f;
    }

    private void HitPlayer()
    {
        List<Collider2D> results = new List<Collider2D>();
        m_attackArea.OverlapCollider(m_playerLayer, results);
        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].tag == GameTag.PLAYER)
            {
                DamageDealerInfo damageDealerInfor = new DamageDealerInfo();
                damageDealerInfor.damage = m_scytheDamage;
                damageDealerInfor.attacker = transform;
                STGameController.HitPlayer(damageDealerInfor);
            }
        }
    }

    // ---------------------------- timer -------------------------- //
    private IEnumerator ScheduleFirstSkill()
    {
        m_currentSkillIndex = 1;
        m_firstSkillCounter = 0;
        yield return null;
        Move(ANIM_DASH);
    }

    private IEnumerator IRefreshSkill()
    {
        StopMove();
        m_currentSkillIndex = 2;
        LookAtPlayer();
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        m_currentState = BodyState.Attack;
        yield return new WaitForSeconds(m_idleTime);
        StartCoroutine(ScheduleFirstSkill());
    }

    // ---------------------------- event -------------------------- //
    private void OnAnimComplete(TrackEntry trackEntry)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case ANIM_ATTACK:
                m_detectArea.gameObject.SetActive(false);
                m_skeletonGhost.ghostingEnabled = false;
                if (m_firstSkillCounter == 2)
                    StartCoroutine(IRefreshSkill());
                else
                    Move(ANIM_DASH);
                break;
        }
    }

    private void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case ANIM_ATTACK:
                m_skeletonGhost.ghostingEnabled = true;
                SoundManager.PlaySound(m_audioCut, false);
                HitPlayer();
                break;
        }
    }

}
