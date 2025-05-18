using System;
using System.Collections;
using DG.Tweening;
using Spine;
using UnityEngine;
using Event = Spine.Event;
using Sequence = DG.Tweening.Sequence;

public class STMonsterKagoruChild : STObjectMonster
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
    [SerializeField] private float m_attackCooldownTime;
    [SerializeField] private float m_attackForwardDistance = 2f;

    [Header("Reference")]
    [SerializeField] private Collider2D m_hitCollider;

    [SerializeField] private BoxCollider2D m_attackRange;
    [SerializeField] private BoxCollider2D m_followRange;

    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private STObjectDealDamage m_fistDamageDealer;
    [SerializeField] private AudioClip m_audioDead;
    [SerializeField] private AudioClip m_audioWalk;
    [SerializeField] private AudioClip m_audioAttack;

    private bool m_isGrounded => IsGrounded();
    private BoxCollider2D m_collider;
    private int m_direction = 1;
    private Vector2 m_baseScale;
    private Bounds m_bounds;
    private GameObject m_soundWalk;
    private GameObject m_soundAttack;
    private Transform m_target;
    private bool m_canAttack => m_playerInAttackRange && !m_attackCooldown;
    private bool m_attackLock;
    private bool m_lookAtPlayer;
    private bool m_playerInAttackRange;
    private bool m_attackCooldown;
    private Sequence m_jumpSeq;
    private Vector3 m_startPos;
    private const string ANIM_ATTACK = "con_attack";
    private const string ANIM_IDLE = "idle_con";
    private const string ANIM_RUN = "jump_con";
    private const string ANIM_SPAWN = "tha_con";
    private const string ANIM_ATTACK_EVENT = "attack";
    private const string ANIM_SPAWN_EVENT = "spawn";
    private const string ANIM_READY_EVENT = "ready";


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
        m_fistDamageDealer.UpdateDamage(m_punchDamage);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);

        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;
        spine.AnimationState.Start += OnAnimStart;
    }

    public void SetUp(Vector3 startPos, int direction, int layerOrder = 0)
    {
        m_startPos = startPos;
        spine.GetComponent<MeshRenderer>().sortingOrder = layerOrder;
        UpdateDirection(direction);
        PauseBehaviour();
    }

    private void OnAnimStart(TrackEntry trackentry)
    {
        if (isDead)
            return;

        switch (trackentry.Animation.Name)
        {
            case ANIM_RUN:
                Jump();
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
                spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
                break;
            case ANIM_IDLE:
                break;
            case ANIM_ATTACK:
                AttackComplete();
                break;
            case ANIM_SPAWN:
                // StartBehaviour();
                break;
        }
    }

    public override void AttackComplete()
    {
        base.AttackComplete();
        m_attackLock = false;
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        StartCoroutine(IAttackCoolDown());
    }

    private void OnAnimEvent(TrackEntry trackEntry, Event e)
    {
        if (isDead)
            return;
        switch (e.Data.Name)
        {
            case ANIM_ATTACK_EVENT:
                Punch();
                break;
            case ANIM_SPAWN_EVENT:
                // Debug.Log("kagoru child ready");
                // StartBehaviour();
                transform.DOMoveX(m_startPos.x, 0.15f);
                break;
            case ANIM_READY_EVENT:
                StartBehaviour();

                break;
        }
    }

    private void Punch()
    {
        // Debug.Log("kagoru child punch");
        m_hitCollider.gameObject.SetActive(true);
        PlaySoundAttack();
        transform.DOLocalMoveX(transform.localPosition.x + m_direction * m_attackForwardDistance, 0.05f)
            .OnComplete(() => { m_hitCollider.gameObject.SetActive(false); });
    }

    private void PlaySoundAttack()
    {
        if (m_soundAttack != null || isDead)
            return;
        m_soundAttack = SoundManager.PlaySound3D(m_audioAttack, 10, false, transform.position);
        Destroy(m_soundAttack, m_audioAttack.length);
    }

    IEnumerator IAttackCoolDown()
    {
        m_attackCooldown = true;
        yield return new WaitForSeconds(m_attackCooldownTime);
        m_attackCooldown = false;
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

        Move();
    }

    private void Move()
    {
        if (m_lookAtPlayer)
        {
            var direction = player.transform.position.x > transform.position.x ? 1 : -1;
            UpdateDirection(direction);
        }

        if (CheckWall() || CheckAbyss())
        {
            StopMove();
            UpdateDirection(-m_direction);
        }

        if (spine.AnimationState.GetCurrent(0).Animation.Name != ANIM_RUN)
        {
            spine.AnimationState.SetAnimation(0, ANIM_RUN, true);
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
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK, false);
    }

    private void StopMove()
    {
        Vector2 veloc = myRigidbody.velocity;
        veloc.x = 0;
        myRigidbody.velocity = veloc;
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
        RaycastHit2D raycast = Physics2D.Raycast(m_collider.bounds.center, rightCheck ? Vector2.right : Vector2.left,
            m_bounds.extents.x + m_moveSpeed + 0.3f, m_wallLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center,
            (rightCheck ? Vector2.right : Vector2.left) * (m_bounds.extents.x + m_moveSpeed + 0.3f), Color.black);
#endif
        return raycast.collider != null;
    }

    void PlaySoundWalk()
    {
        if (m_soundWalk != null || isDead)
            return;
        m_soundWalk = SoundManager.PlaySound3D(m_audioWalk, 10, true, transform.position);
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