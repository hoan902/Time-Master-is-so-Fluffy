using System.Collections;
using DG.Tweening;
using Spine;
using Unity.Mathematics;
using UnityEngine;
using Event = Spine.Event;

public class STMonsterRockWorm : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private int m_bodyDamage = 10;

    [SerializeField] private float m_moveSpeed = 3f;
    [SerializeField] private LayerMask m_playerLayer;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private LayerMask m_wallLayer;
    [SerializeField] private float m_attackRange = 10f;
    [SerializeField] private float m_attackCoolDown = 3f;
    [SerializeField] private float m_actionDelay = 0.3f;

    [Header("Bullet Config")]
    [SerializeField] private float m_minBulletHeight = 5f;

    [SerializeField] private int m_bulletDamage = 10;
    [SerializeField] private Ease m_bulletMoveEase;
    [SerializeField] private float m_bulletSpeed = 5f;

    [Header("Range Config")]
    [SerializeField] private bool m_staticAttackLocation;

    [SerializeField] private Vector2 m_maxAttackAreaSize = new Vector2(15, 20);
    [SerializeField] private Vector2 m_minAttackAreaSize = new Vector2(5, 20);
    [SerializeField] private Vector2 m_optimalAttackPadding = new Vector2(1, 0);
    [SerializeField] private Vector2 m_attackAreaOffset;
    [SerializeField] private Vector2 m_followAreaSize = new Vector2(20, 20);

    [Header("Reference")]
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;

    [SerializeField] private STObjectDealDamage m_bulletDamageDealer;
    [SerializeField] private GameObject m_body;
    [SerializeField] private GameObject m_bullet;
    [SerializeField] private Transform m_shotPoint;
    [SerializeField] private BoxCollider2D m_maxAttackRange;
    [SerializeField] private BoxCollider2D m_minAttackRange;
    [SerializeField] private BoxCollider2D m_followRange;
    [SerializeField] private AudioClip m_audioShoot;
    [SerializeField] private AudioClip m_audioDead;
    [SerializeField] private AudioClip m_audioWalk;
    private bool m_canAttack = true;
    private bool m_lookAtPlayer;
    private BoxCollider2D m_collider;
    private Vector2 m_startPos;
    private int m_direction = 1;
    private Vector2 m_baseScale;
    private Bounds m_bounds;
    private bool m_hitting;
    private Vector2? m_targetPos;
    private Transform m_target;
    private bool m_shooted;
    private GameObject m_soundWalk;
    private float m_attackTimer;
    private bool m_attackLock;
    private bool m_delay;
    private bool m_canMoveAway = true;
    private const string ANIM_ATTACK = "attack";
    private const string ANIM_IDLE = "idle";
    private const string ANIM_RUN = "run";

    public override void Awake()
    {
        base.Awake();
        Init();
    }

    private void Init()
    {
        m_collider = GetComponent<BoxCollider2D>();
        m_startPos = transform.position;
        m_baseScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        m_direction = m_baseScale.x > 0 ? 1 : -1;
        m_bounds = m_collider.bounds;
        m_maxAttackRange.size = m_maxAttackAreaSize;
        m_minAttackRange.size = m_minAttackAreaSize;
        m_maxAttackRange.offset = m_attackAreaOffset;
        m_minAttackRange.offset = m_attackAreaOffset;
        m_followRange.size = m_followAreaSize;

        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        m_bulletDamageDealer.UpdateDamage(m_bulletDamage);

        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;

        m_maxAttackRange.gameObject.SetActive(true);
        m_minAttackRange.gameObject.SetActive(true);
        m_followRange.gameObject.SetActive(true);
        if (m_staticAttackLocation)
        {
            m_maxAttackRange.transform.SetParent(transform.parent);
            m_minAttackRange.transform.SetParent(transform.parent);
        }
    }

    private void FixedUpdate()
    {
#if UNITY_EDITOR
        bool isLeft = transform.localScale.x < 0;
        Debug.DrawRay(
            m_maxAttackRange.bounds.center +
            new Vector3((isLeft ? -1 : 1) * m_maxAttackRange.bounds.extents.x, m_maxAttackRange.bounds.extents.y),
            Vector2.down * m_maxAttackRange.bounds.extents.y, Color.red);
        Debug.DrawRay(
            m_minAttackRange.bounds.center +
            new Vector3((isLeft ? -1 : 1) * m_minAttackRange.bounds.extents.x, m_minAttackRange.bounds.extents.y),
            Vector2.down * m_minAttackRange.bounds.extents.y, Color.yellow);
        Debug.DrawRay(center.position + ((Vector3)m_attackAreaOffset +
                                         new Vector3(
                                             (isLeft ? -1 : 1) * (m_minAttackAreaSize.x + m_optimalAttackPadding.x) / 2,
                                             0.5f)),
            (isLeft ? Vector3.left : Vector3.right) *
            (m_maxAttackAreaSize.x - m_minAttackAreaSize.x - m_optimalAttackPadding.x * 2) / 2, Color.cyan);
        Debug.DrawRay(
            m_followRange.bounds.center +
            new Vector3((isLeft ? -1 : 1) * m_followRange.bounds.extents.x, m_followRange.bounds.extents.y),
            Vector2.down * m_followRange.bounds.extents.y, Color.blue);
#endif

        if (isDead || !startBehaviour || m_delay)
            return;
        if (m_canAttack && m_target != null)
        {
            Attack();
        }

        Move();
    }

    public override void Dead()
    {
        base.Dead();
        StopAllCoroutines();
        m_body.SetActive(false);
        // SoundManager.PlaySound(m_audioDead, false);
        // StopSoundWalk();
    }

    // TODO: REFACTOR THIS
    IEnumerator IDelayAction()
    {
        m_delay = true;
        yield return new WaitForSeconds(m_actionDelay);
        m_delay = false;
    }

    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        Destroy(m_maxAttackRange.gameObject);
        Destroy(m_minAttackRange.gameObject);
        Destroy(gameObject);
    }

    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        base.OnHit(attackerInfor);
        m_hitting = true;
    }

    private void Move()
    {
        if (!IsGrounded())
            return;
        if (!m_attackLock)
        {
            // if (IsInAttackRange())
            // {
            //     StopMove();
            //     return;
            // }
            // if (CheckWall() || CheckAbyss())
            // {
            //     var direction = player.transform.position.x > transform.position.x ? 1 : -1;
            //     UpdateDirection(direction);
            //     if (spine.state.GetCurrent(0).Animation.Name != ANIM_IDLE)
            //         spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
            //     StopMove();
            //     return;
            // }

            if (m_lookAtPlayer)
            {
                if (m_canAttack)
                {
                    MoveAttack();
                }
                else
                {
                    if (m_canMoveAway)
                        MoveAway();
                }
            }
            else
            {
                // patrol
                if (CheckWall() || CheckAbyss())
                {
                    UpdateDirection(-m_direction);
                }

                if (spine.state.GetCurrent(0).Animation.Name != ANIM_RUN)
                {
                    spine.AnimationState.SetAnimation(0, ANIM_RUN, true);
                }

                myRigidbody.velocity = new Vector2(m_direction * m_moveSpeed, myRigidbody.velocity.y);
            }
        }
    }

    // TODO: REFACTOR THESE MOVE FUNCTION
    private void MoveAttack()
    {
        var minDist = m_minAttackRange.bounds.extents.x + m_optimalAttackPadding.x / 2;
        var maxDist = m_maxAttackRange.bounds.extents.x - m_optimalAttackPadding.x / 2;
        var playerDistance = Mathf.Abs(player.transform.position.x - transform.position.x);
        var direction = player.transform.position.x > transform.position.x ? 1 : -1;
        if (playerDistance < minDist)
        {
            direction = -direction;
        }

        UpdateDirection(direction);
        if (spine.state.GetCurrent(0).Animation.Name != ANIM_RUN)
        {
            spine.AnimationState.SetAnimation(0, ANIM_RUN, true);
        }

        myRigidbody.velocity = new Vector2(m_direction * m_moveSpeed, myRigidbody.velocity.y);
    }

    private void MoveAway()
    {
        var maxDist = m_maxAttackRange.bounds.extents.x - m_optimalAttackPadding.x / 2;
        var playerDistance = Mathf.Abs(player.transform.position.x - transform.position.x);
        if (playerDistance < maxDist && !(CheckWall() || CheckAbyss()))
        {
            var direction = player.transform.position.x > transform.position.x ? -1 : 1;
            UpdateDirection(direction);
            if (spine.state.GetCurrent(0).Animation.Name != ANIM_RUN)
            {
                spine.AnimationState.SetAnimation(0, ANIM_RUN, true);
            }

            myRigidbody.velocity = new Vector2(m_direction * m_moveSpeed, myRigidbody.velocity.y);
        }
        else
        {
            m_canMoveAway = false;
            var direction = player.transform.position.x > transform.position.x ? 1 : -1;
            UpdateDirection(direction);
            if (spine.state.GetCurrent(0).Animation.Name != ANIM_IDLE)
                spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
            StopMove();
        }
    }

    private bool IsInAttackRange()
    {
        var minDist = m_minAttackRange.bounds.extents.x + m_optimalAttackPadding.x / 2;
        var maxDist = m_maxAttackRange.bounds.extents.x - m_optimalAttackPadding.x / 2;
        var playerDistance = Mathf.Abs(player.transform.position.x - transform.position.x);
        return playerDistance <= maxDist && playerDistance >= minDist && m_target != null;
    }


    void FollowPlayer()
    {
        m_lookAtPlayer = true;
        if (bodyState == State.Attacking || m_hitting)
            return;
        // ReadyAttack();
    }

    void UnFollowPlayer()
    {
        m_lookAtPlayer = false;
        if (bodyState == State.Attacking || m_hitting)
            return;
        // Patrol();
        UpdateDirection(m_direction);
    }

    private void OnAnimEvent(TrackEntry trackEntry, Event e)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case ANIM_ATTACK:
                Shoot();
                break;
        }
    }

    public override void PlayerInRange(Collider2D other)
    {
        // m_canAttack = true;
        m_target = other.transform;
    }

    public override void PlayerOutRange(Collider2D other)
    {
        m_target = null;
        // m_canAttack = false;
    }

    void UpdateDirection(int dir, bool turnAround = true)
    {
        if (!myRigidbody.simulated)
            return;
        m_direction = dir;
        if (turnAround)
        {
            transform.localScale = new Vector2(m_baseScale.x * m_direction, m_baseScale.y);
        }
    }

    public override void StartBehaviour()
    {
        base.StartBehaviour();
        StartCoroutine(AttackCooldown());
        m_maxAttackRange.gameObject.SetActive(true);
        m_minAttackRange.gameObject.SetActive(true);
        m_followRange.gameObject.SetActive(true);
        if (m_staticAttackLocation)
        {
            m_maxAttackRange.transform.SetParent(transform.parent);
            m_minAttackRange.transform.SetParent(transform.parent);
        }
    }

    private void Shoot()
    {
        // TODO: GET MAX HEIGHT TO CALCULATE Y MOVE TIME FOR SHORT DISTANCE SHOT

        // Add a small offset in raycast because target position y axis can be on the same y axis as ground so the raycast won't be accurate
        var raycast = Physics2D.Raycast(m_targetPos.Value + new Vector2(0, 0.1f), Vector2.down, 1000f, m_groundLayer);

        var shootLocation = raycast.point;
        var offSetY = shootLocation.y - m_shotPoint.position.y;
        var jumpPower = Mathf.Abs(offSetY) > m_minBulletHeight
            ? Mathf.Abs(offSetY) / m_minBulletHeight + m_minBulletHeight
            : m_minBulletHeight;
        var timeMoveX = Mathf.Abs(shootLocation.x - m_shotPoint.position.x) / m_bulletSpeed;
        var timeMoveY = (jumpPower + Mathf.Clamp(offSetY, 0, offSetY)) / m_bulletSpeed;
        var timeMove = timeMoveX + timeMoveY;
        SoundManager.PlaySound(m_audioShoot, false);
        var bullet = Instantiate(m_bullet, m_shotPoint.position, quaternion.identity);
        bullet.SetActive(true);
        bullet.GetComponent<STEnemyBullet>().Init(Vector2.zero, 0);
        bullet.transform
            .DOJump(shootLocation, jumpPower, 1, timeMove)
            .SetEase(m_bulletMoveEase)
            .OnComplete(() => bullet.GetComponent<STEnemyBullet>().ExplosionEffect());
    }


    IEnumerator AttackCooldown()
    {
        m_canAttack = false;
        yield return new WaitForSeconds(m_attackCoolDown);
        m_canAttack = true;
    }

    private void OnAnimComplete(TrackEntry trackEntry)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case ANIM_RUN:
            case ANIM_IDLE:
                break;
            case ANIM_ATTACK:
                AttackComplete();
                break;
        }
    }

    public override void AttackComplete()
    {
        base.AttackComplete();
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        m_attackLock = false;
        m_canMoveAway = true;
        StartCoroutine(IDelayAction());
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    void StopMove()
    {
        Vector2 veloc = myRigidbody.velocity;
        veloc.x = 0;
        myRigidbody.velocity = veloc;
        // StopSoundWalk();
    }

    public override void Attack()
    {
        base.Attack();
        StopMove();
        m_attackLock = true;
        m_targetPos = m_target.position;
        UpdateDirection(m_target?.position.x > transform.position.x ? 1 : -1);
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK, false);
        StartCoroutine(AttackCooldown());
    }

    private bool IsGrounded()
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
                m_collider.bounds.center + new Vector3(rightCheck ? m_bounds.extents.x : -m_bounds.extents.x, 0),
                Vector2.down, m_bounds.extents.y + 1f, m_groundLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center + new Vector3(rightCheck ? m_bounds.extents.x : -m_bounds.extents.x, 0),
            Vector2.down * (m_bounds.extents.y + 1f), Color.black);
#endif
        return raycast.collider == null;
    }

    bool CheckWall()
    {
        bool rightCheck = m_direction > 0;
        RaycastHit2D raycast = Physics2D.Raycast(m_collider.bounds.center, rightCheck ? Vector2.right : Vector2.left,
            m_bounds.extents.x + 0.2f, m_wallLayer);
        // RaycastHit2D leftRayCast = Physics2D.Raycast(m_collider.bounds.center, Vector2.left, m_bounds.extents.x + 0.5f,
        // m_wallLayer);
        // RaycastHit2D rightRayCast = Physics2D.Raycast(m_collider.bounds.center, Vector2.right,
        // m_bounds.extents.x + 0.5f,
        // m_wallLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center,
            (rightCheck ? Vector2.right : Vector2.left) * (m_bounds.extents.x + 0.5f), Color.black);
        // Debug.DrawRay(m_collider.bounds.center,
        //     Vector2.right * (m_bounds.extents.x + 0.5f), Color.black);
        // Debug.DrawRay(m_collider.bounds.center,
        //     Vector2.left * (m_bounds.extents.x + 0.5f), Color.black);
#endif
        return raycast.collider != null;
        // return leftRayCast.collider != null || rightRayCast.collider != null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        bool isLeft = transform.localScale.x < 0;
        Gizmos.DrawRay(
            center.position + (Vector3)m_attackAreaOffset +
            new Vector3((m_minAttackAreaSize.x + m_optimalAttackPadding.x) / 2, 0.5f),
            (isLeft ? Vector3.left : Vector3.right) *
            (m_maxAttackAreaSize.x - m_minAttackAreaSize.x - m_optimalAttackPadding.x * 2) / 2);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(m_maxAttackRange.bounds.center + (Vector3)m_attackAreaOffset, m_maxAttackAreaSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(m_minAttackRange.bounds.center + (Vector3)m_attackAreaOffset, m_minAttackAreaSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(m_followRange.bounds.center, m_followAreaSize);
    }
}