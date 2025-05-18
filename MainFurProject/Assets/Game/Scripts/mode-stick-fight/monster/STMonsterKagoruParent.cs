using System;
using System.Collections;
using DG.Tweening;
using Spine;
using UnityEngine;
using Event = Spine.Event;
using Sequence = DG.Tweening.Sequence;

public class STMonsterKagoruParent : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private float m_moveSpeed = 5f;

    [SerializeField] private float m_jumpDuration;
    [SerializeField] private float m_jumpDelay;
    [SerializeField] private float m_jumpHeight;
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_punchDamage = 10;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private LayerMask m_wallLayer;
    [SerializeField] private Vector2 m_attackAreaSize = new(5, 2);
    [SerializeField] private Vector2 m_followAreaSize = new(15, 2);
    [SerializeField] private float m_attackCooldownTime = 2;
    [SerializeField] private int m_maxChildCount = 3;

    [Header("Reference")]
    [SerializeField] private STMonsterKagoruChild m_childPrefab;

    [SerializeField] private Transform m_childStartPos;
    [SerializeField] private BoxCollider2D m_attackRange;
    [SerializeField] private BoxCollider2D m_followRange;

    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private AudioClip m_audioDead;
    [SerializeField] private AudioClip m_audioWalk;

    private bool m_isGrounded => IsGrounded();
    private BoxCollider2D m_collider;
    private int m_direction = 1;
    private Vector2 m_baseScale;
    private Bounds m_bounds;
    private GameObject m_soundWalk;
    private Transform m_target;
    private bool m_canAttack => m_playerInAttackRange && !m_attackCooldown && m_childCount < m_maxChildCount;
    private bool m_attackLock;
    private bool m_lookAtPlayer;
    private bool m_playerInAttackRange;
    private bool m_attackCooldown;
    private Sequence m_jumpSeq;
    private bool m_enrage;
    private int m_childCount = 0;
    private string m_currentSkin => !m_enrage ? "1" : "2";
    private string m_childSkin = "3";
    private const string ANIM_ANGRY = "angry";
    private const string ANIM_IDLE = "idle";
    private const string ANIM_RUN = "jump";
    private const string ANIM_RUN_FAST = "jump_fast";
    private const string ANIM_SPAWN_CHILD = "tha_con";
    public Action deadEvent;

    public override void Awake()
    {
        base.Awake();
        m_collider = GetComponent<BoxCollider2D>();
        m_baseScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        m_direction = transform.localScale.x > 0 ? 1 : -1;
        m_bounds = m_collider.bounds;

        m_attackRange.size = m_attackAreaSize;
        m_followRange.size = m_followAreaSize;
        m_followRange.gameObject.SetActive(true);
        m_attackRange.gameObject.SetActive(true);
        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);

        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;
        spine.AnimationState.Start += OnAnimStart;
        spine.SetMixSkin(m_currentSkin, m_childSkin);
    }

    public override void StartBehaviour()
    {
        base.StartBehaviour();
        StartCoroutine(IAttackCoolDown());
        m_followRange.gameObject.SetActive(true);
        m_attackRange.gameObject.SetActive(true);
    }

    private void OnAnimStart(TrackEntry trackentry)
    {
        if (isDead)
            return;

        switch (trackentry.Animation.Name)
        {
            case ANIM_RUN:
            case ANIM_RUN_FAST:
                Jump();
                break;
            case ANIM_SPAWN_CHILD:
                break;
        }
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
        // StopSoundWalk();
    }

    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        Destroy(gameObject);
    }

    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        m_jumpSeq.Kill();
        base.OnHit(attackerInfor);
    }

    public override void Dead()
    {
        base.Dead();
        m_bodyDamageDealer.gameObject.SetActive(false);
        StopAllCoroutines();
        deadEvent?.Invoke();
        // SoundManager.PlaySound(m_audioDead, false);
        // StopSoundWalk();
    }

    public override void OnResumeAfterHit()
    {
    }

    public override void PauseBehaviour()
    {
        myRigidbody.simulated = false;
        startBehaviour = false;
    }

    private void OnAnimComplete(TrackEntry trackEntry)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case ANIM_RUN:
            case ANIM_RUN_FAST:
                spine.AnimationState.AddAnimation(0, ANIM_IDLE, true, 0f);
                break;
            case ANIM_ANGRY:
                break;
            case ANIM_SPAWN_CHILD:
                AttackComplete();
                break;
            case ANIM_IDLE:
                break;
        }
    }

    public override void AttackComplete()
    {
        base.AttackComplete();
        m_attackLock = false;
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        // StartCoroutine(IAttackCoolDown());
    }

    private void OnAnimEvent(TrackEntry trackEntry, Event e)
    {
        if (isDead)
            return;
        switch (e.Data.Name)
        {
        }
    }

    private void SpawnChild()
    {
        m_childCount++;
        spine.SetSkin(m_currentSkin);
        var child = Instantiate(m_childPrefab, transform.position, Quaternion.identity);
        child.SetUp(m_childStartPos.position, m_direction, m_childCount);
        spine.AnimationState.SetAnimation(0, ANIM_SPAWN_CHILD, false);
        child.deadEvent += OnChildDied;
    }

    private void OnChildDied()
    {
        // m_childCount--;
        if (!m_enrage && !isDead)
        {
            m_enrage = true;
            if (m_attackLock)
            {
                spine.AnimationState.AddAnimation(0, ANIM_ANGRY, false, 0f);
            }
            else
            {
                spine.AnimationState.SetAnimation(0, ANIM_ANGRY, false);
            }

            m_jumpDuration = (0.733f / 1.5f) * m_jumpDuration;
            m_jumpDelay = (0.733f / 1.5f) * m_jumpDelay;
            spine.SetSkin(m_currentSkin);
        }
    }

    IEnumerator IAttackCoolDown()
    {
        m_attackCooldown = true;
        spine.SetSkin(m_currentSkin);
        yield return new WaitForSeconds(m_attackCooldownTime);
        m_attackCooldown = false;
        if (m_childCount < m_maxChildCount)
            spine.SetMixSkin(m_currentSkin, m_childSkin);
    }

    private void Start()
    {
        if (Mathf.Approximately(m_moveSpeed, 0))
            return;
        if (!myRigidbody.simulated)
            return;
        // PlaySoundWalk();
    }


    private void FixedUpdate()
    {
        if (isDead || !startBehaviour)
            return;
        if (!m_isGrounded || m_attackLock)
            return;


        if (m_canAttack)
        {
            Attack();
            return;
        }

        if (m_lookAtPlayer)
        {
            var direction = player.transform.position.x > transform.position.x ? 1 : -1;
            UpdateDirection(direction);
        }


        if (m_playerInAttackRange && !m_enrage)
        {
            StopMove();
            return;
        }


        Move();
    }

    private void Move()
    {
        if (CheckWall() || CheckAbyss())
        {
            // StopMove();
            UpdateDirection(-m_direction);
        }

        if (m_enrage)
        {
            if (spine.AnimationState.GetCurrent(0).Animation.Name != ANIM_RUN_FAST)
            {
                spine.AnimationState.AddAnimation(0, ANIM_RUN_FAST, false, 0f);
            }
        }
        else
        {
            if (spine.AnimationState.GetCurrent(0).Animation.Name != ANIM_RUN)
            {
                spine.AnimationState.AddAnimation(0, ANIM_RUN, false, 0f);
            }
        }


        // Vector2 velocity = myRigidbody.velocity;
        // velocity.x = m_direction * m_moveSpeed;
        // myRigidbody.velocity = velocity;
    }

    private void Jump()
    {
        // Debug.Log("kagoru jump");
        // m_jumping = true;
        if (!m_isGrounded)
            return;
        m_jumpSeq = myRigidbody
            .DOJump(transform.position + new Vector3(m_direction * m_moveSpeed, 0), m_jumpHeight, 1,
                m_jumpDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => PlaySoundWalk())
            .SetDelay(m_jumpDelay);
    }

    public override void PlayerInRange(Collider2D other)
    {
        m_playerInAttackRange = true;
    }

    public override void PlayerOutRange(Collider2D other)
    {
        m_playerInAttackRange = false;
    }

    private void Attack()
    {
        StopMove();
        m_attackLock = true;
        m_attackCooldown = true;
        UpdateDirection(player.transform.position.x > transform.position.x ? 1 : -1);
        SpawnChild();
        StartCoroutine(IAttackCoolDown());
    }

    private void StopMove()
    {
        Vector2 veloc = myRigidbody.velocity;
        veloc.x = 0;
        myRigidbody.velocity = veloc;
        if (spine.AnimationState.GetCurrent(0).Animation.Name != ANIM_IDLE)
            spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        m_jumpSeq.Kill();
    }

    bool IsGrounded()
    {
        RaycastHit2D leftCast =
            Physics2D.Raycast(m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, 0), Vector2.down,
                m_collider.bounds.extents.y + 0.1f, m_groundLayer);
        RaycastHit2D rightCast =
            Physics2D.Raycast(m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, 0), Vector2.down,
                m_collider.bounds.extents.y + 0.1f, m_groundLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, 0),
            Vector3.down * (m_collider.bounds.extents.y + 0.1f), Color.yellow);
        Debug.DrawRay(m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, 0),
            Vector3.down * (m_collider.bounds.extents.y + 0.1f), Color.yellow);
#endif
        return leftCast.collider != null || rightCast.collider != null;
    }

    bool CheckAbyss()
    {
        bool rightCheck = m_direction > 0;
        RaycastHit2D raycast =
            Physics2D.Raycast(
                m_collider.bounds.center +
                new Vector3(rightCheck ? m_bounds.extents.x + m_moveSpeed : -m_bounds.extents.x - m_moveSpeed, 0),
                Vector2.down, m_bounds.extents.y + 0.2f, m_groundLayer);
#if UNITY_EDITOR
        Debug.DrawRay(
            m_collider.bounds.center +
            new Vector3(rightCheck ? m_bounds.extents.x + m_moveSpeed : -m_bounds.extents.x - m_moveSpeed, 0),
            Vector2.down * (m_bounds.extents.y + 0.2f), Color.black);
#endif
        return raycast.collider == null;
    }

    bool CheckWall()
    {
        bool rightCheck = m_direction > 0;
        RaycastHit2D raycast = Physics2D.Raycast(center.position, rightCheck ? Vector2.right : Vector2.left,
            m_bounds.extents.x + 0.3f + m_moveSpeed, m_wallLayer);
#if UNITY_EDITOR
        Debug.DrawRay(center.position,
            (rightCheck ? Vector2.right : Vector2.left) * (m_bounds.extents.x + 0.3f + m_moveSpeed), Color.black);
#endif
        return raycast.collider != null;
    }

    void PlaySoundWalk()
    {
        //m_soundWalk != null ||
        if (isDead)
            return;
        var soundWalk = SoundManager.PlaySound3D(m_audioWalk, 10, false, transform.position);
        Destroy(soundWalk, m_audioWalk.length);
    }

    void StopSoundWalk()
    {
        if (m_soundWalk != null)
            Destroy(m_soundWalk);
    }

    void UpdateDirection(int dir)
    {
        if (!myRigidbody.simulated)
            return;
        m_direction = dir;
        transform.localScale = new Vector2(m_baseScale.x * m_direction, m_baseScale.y);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
    }

    void FollowPlayer()
    {
        m_lookAtPlayer = true;
        if (bodyState == State.Attacking)
            return;
        // ReadyAttack();
    }

    void UnFollowPlayer()
    {
        m_lookAtPlayer = false;
        if (bodyState == State.Attacking)
            return;

        UpdateDirection(m_direction);
    }

    void OnBounce(Vector2 velocity)
    {
        myRigidbody.velocity = velocity;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(m_attackRange.bounds.center, m_attackAreaSize);


        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(m_followRange.bounds.center, m_followAreaSize);
    }
}