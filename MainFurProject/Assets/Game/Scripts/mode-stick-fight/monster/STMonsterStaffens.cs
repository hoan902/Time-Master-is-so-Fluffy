using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using UnityEditor;

public class STMonsterStaffens : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_bulletDamage = 10;
    [SerializeField] private float m_moveSpeed = 3f;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private LayerMask m_wallLayer;
    [SerializeField] private float m_attackRange = 10f;
    [SerializeField] private float m_attackDuration = 3f;
    [SerializeField] private float m_maxBulletHeight = 5f;
    [SerializeField] private Vector2 m_attackAreaSize = new Vector2(15, 20);
    [SerializeField] private Vector2 m_followAreaSize = new Vector2(20, 20);

    [Header("Reference")]
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private STObjectDealDamage m_bulletDamageDealer;
    [SerializeField] private GameObject m_body;
    [SerializeField] private GameObject m_bullet;
    [SerializeField] private Transform m_shotPoint;
    [SerializeField] private BoxCollider2D m_activeRange;
    [SerializeField] private BoxCollider2D m_followRange;
    [SerializeField] private AudioClip m_audioShoot;
    [SerializeField] private AudioClip m_audioDead;
    [SerializeField] private AudioClip m_audioWalk;

    private bool m_canAttack;
    private bool m_lookAtPlayer;
    private bool m_isGrounded;
    private BoxCollider2D m_collider;
    private Vector2 m_startPos;
    private int m_direction = 1;
    private Vector2 m_baseScale;
    private Bounds m_bounds;
    private bool m_hitting;
    private Vector2? m_targetPos;
    private bool m_shooted;
    private GameObject m_soundWalk;
    private float m_attackTimer;

    private const string ANIM_ATTACK = "attack";
    private const string ANIM_ATTACK_2 = "attack2";
    private const string ANIM_IDLE = "idle";
    private const string ANIM_WALK = "walk";
    private const string ANIM_WALK_BACK = "walk_back";

    public override void Awake()
    {
        base.Awake();
        m_collider = GetComponent<BoxCollider2D>();
        m_startPos = transform.position;
        m_baseScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        m_direction = m_baseScale.x > 0 ? 1 : -1;
        m_bounds = m_collider.bounds;
        m_activeRange.size = m_attackAreaSize;
        m_followRange.size = m_followAreaSize;
        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        m_bulletDamageDealer.UpdateDamage(m_bulletDamage);

        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        StopSoundWalk();
        spine.AnimationState.Complete -= OnAnimComplete;
        spine.AnimationState.Event -= OnAnimEvent;
    }
    public override void Attack()
    {
        base.Attack();
        StopMove();
        UpdateDirection(player.transform.position.x > transform.position.x ? 1 : -1);
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK_2, false);
        m_attackTimer = 0;
    }
    public override void AttackComplete()
    {
        base.AttackComplete();
        m_hitting = false;
        if(!m_lookAtPlayer)
            Patrol();
        else
            ReadyAttack();
        m_targetPos = null;
    }
    public override void PlayerInRange(Collider2D other)
    {
        m_canAttack = true;
    }
    public override void PlayerOutRange(Collider2D other)
    {
        m_canAttack = false;
    }
    public override void OnResumeAfterHit()
    {
        m_hitting = false;
        if(bodyState == State.Attacking)
            return;
        if(!m_lookAtPlayer)
            Patrol();
        else
            ReadyAttack();
    }
    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        Destroy(gameObject);
    }
    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        base.OnHit(attackerInfor);
        m_hitting = true;
    }
    public override void StartBehaviour()
    {
        base.StartBehaviour();
        m_activeRange.gameObject.SetActive(true);
        m_followRange.gameObject.SetActive(true);
        if(!m_lookAtPlayer)
            Patrol();
        else
            ReadyAttack();
    }
    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
        m_activeRange.gameObject.SetActive(false);
        m_followRange.gameObject.SetActive(false);
    }
    public override void Dead()
    {
        base.Dead();
        StopAllCoroutines();
        m_body.SetActive(false);
        SoundManager.PlaySound(m_audioDead, false);
        StopSoundWalk();
    }

    private void Start() 
    {
        m_activeRange.gameObject.SetActive(true);
        m_followRange.gameObject.SetActive(true);
        if(Mathf.Approximately(m_moveSpeed, 0))
            return;
        if(!myRigidbody.simulated)
            return;
        Patrol();   
    }
    private void FixedUpdate() 
    {
#if UNITY_EDITOR
        Debug.DrawRay(m_activeRange.bounds.center + new Vector3(m_activeRange.bounds.extents.x, m_activeRange.bounds.extents.y), Vector2.down * m_activeRange.bounds.extents.y, Color.yellow);
        Debug.DrawRay(m_followRange.bounds.center + new Vector3(m_followRange.bounds.extents.x, m_followRange.bounds.extents.y), Vector2.down * m_followRange.bounds.extents.y, Color.blue);
#endif
        if(isDead || !startBehaviour)
            return;
        if(m_soundWalk != null)
            m_soundWalk.transform.position = transform.position;
        if(bodyState == State.Attacking)
            return;
        
        m_attackTimer += Time.deltaTime;
        if(m_attackTimer >= m_attackDuration && m_canAttack)
        {
            StopMove();
            Attack();
            return;
        }
        
        CheckGrounded();

        if(!m_isGrounded)
            return;

        if(m_lookAtPlayer)
        {
            MoveLookAtPlayer();
        }
        else
        {
            MoveDirectly();
        }
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        if(isDead)
            return;
        switch(trackEntry.Animation.Name)
        {
            case ANIM_WALK:
            case ANIM_WALK_BACK:
            case ANIM_IDLE:
                if(m_lookAtPlayer)
                    StartCoroutine(ReadyAttackMoveStepComplete());
                break;
            case ANIM_ATTACK_2:
            case ANIM_ATTACK:
                AttackComplete();
                break;
        }
    }
    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if(isDead)
            return;
        switch(trackEntry.Animation.Name)
        {
            case ANIM_ATTACK_2:
            case ANIM_ATTACK:
                Shoot();
                break;
        }
    }

    void CheckGrounded()
    {
        RaycastHit2D leftCast = Physics2D.Raycast(m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, 0), Vector2.down, m_collider.bounds.extents.y + 0.1f, m_groundLayer);
        RaycastHit2D rightCast = Physics2D.Raycast(m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, 0), Vector2.down, m_collider.bounds.extents.y + 0.1f, m_groundLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, 0), Vector3.down * (m_collider.bounds.extents.y + 0.1f), Color.yellow);
        Debug.DrawRay(m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, 0), Vector3.down * (m_collider.bounds.extents.y + 0.1f), Color.yellow);
#endif
        m_isGrounded = leftCast.collider != null || rightCast.collider != null;
    }
    bool CheckAbyss()
    {
        bool rightCheck = m_direction > 0;
        RaycastHit2D raycast = Physics2D.Raycast(m_collider.bounds.center + new Vector3(rightCheck ? m_bounds.extents.x : -m_bounds.extents.x, 0), Vector2.down, m_bounds.extents.y + 1f, m_groundLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center + new Vector3(rightCheck ? m_bounds.extents.x : -m_bounds.extents.x, 0), Vector2.down * (m_bounds.extents.y + 1f), Color.black);
#endif
        return raycast.collider == null;
    }
    bool CheckWall()
    {
        bool rightCheck = m_direction > 0;
        RaycastHit2D raycast = Physics2D.Raycast(m_collider.bounds.center, rightCheck ? Vector2.right : Vector2.left, m_bounds.extents.x + 0.2f, m_wallLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center, (rightCheck ? Vector2.right : Vector2.left) * (m_bounds.extents.x + 0.2f), Color.black);
#endif
        return raycast.collider != null;
    }

    void UpdateDirection(int dir, bool turnAround = true)
    {
        if(!myRigidbody.simulated)
            return;
        m_direction = dir;
        if(turnAround)
            transform.localScale = new Vector2(m_baseScale.x * m_direction, m_baseScale.y);
    }
    void Patrol()
    {
        PlaySoundWalk();
        spine.AnimationState.SetAnimation(0, ANIM_WALK, true);
    }
    void ReadyAttack()
    {
        StopMove();
        UpdateDirection(player.transform.position.x > transform.position.x ? 1 : -1);
        spine.AnimationState.SetAnimation(0, ANIM_WALK, true);
    }
    void MoveDirectly()
    {
        if(Mathf.Approximately(m_moveSpeed, 0) || !m_isGrounded)
            return;
        if(CheckWall() || CheckAbyss())
            UpdateDirection(-m_direction);
        myRigidbody.velocity = new Vector2(m_direction * m_moveSpeed, myRigidbody.velocity.y);
    }
    void MoveLookAtPlayer()
    {
        if(!m_isGrounded)
            return;
        if(CheckWall() || CheckAbyss())
        {
            UpdateDirection(-m_direction, false);
            UpdateMoveAnimation();
        }
        if(spine.state.GetCurrent(0).Animation.Name != ANIM_IDLE)
            myRigidbody.velocity = new Vector2(m_direction * m_moveSpeed, myRigidbody.velocity.y);
    }
    void StopMove()
    {
        Vector2 veloc = myRigidbody.velocity;
        veloc.x = 0;
        myRigidbody.velocity = veloc;
        StopSoundWalk();
    }
    IEnumerator ReadyAttackMoveStepComplete()
    {
        while(!m_isGrounded || m_hitting)
        {
            yield return null;
        }
        if(bodyState == State.Attacking)
            yield break;
        int rand = Random.Range(0, 100);
        if(rand < 20)
        {
            StopMove();
            spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
            int directionToPlayer = player.transform.position.x > transform.position.x ? 1 : -1;
            UpdateDirection(directionToPlayer);
            yield break;
        }
        else
        {
            PlaySoundWalk();
            int rand2 = Random.Range(0, 100);
            if(rand2 > 50) // change direction
                UpdateDirection(-m_direction, false);
            UpdateMoveAnimation();
        }
    }
    void UpdateMoveAnimation()
    {
        int directionToPlayer = player.transform.position.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector2(m_baseScale.x * directionToPlayer, m_baseScale.y);
        if(directionToPlayer + m_direction != 0)
            spine.AnimationState.SetAnimation(0, ANIM_WALK, true);
        else
            spine.AnimationState.SetAnimation(0, ANIM_WALK_BACK, true);
    }
    void Shoot()
    {
        if(m_targetPos == null)
        {
            bool isLeft = transform.localScale.x < 0;
            m_targetPos = transform.position + new Vector3(isLeft ? -m_attackRange : m_attackRange, 0);
        }
        GameObject bone = Instantiate(m_bullet, m_shotPoint.position, Quaternion.identity, transform.parent);
        bone.SetActive(true);
        Rigidbody2D boneRig = bone.GetComponent<Rigidbody2D>();
        Vector2 finalVelocity = Blobcreate.ProjectileToolkit.Projectile.VelocityByHeight(bone.transform.position, m_targetPos.Value, m_maxBulletHeight);
        boneRig.AddForce(finalVelocity * boneRig.mass, ForceMode2D.Impulse);
        bone.GetComponent<STEnemyBullet>().Init(Vector2.zero, 0);
    }

    void FollowPlayer()
    {
        m_lookAtPlayer = true;
        if(bodyState == State.Attacking || m_hitting)
            return;
        ReadyAttack();
    }
    void UnFollowPlayer()
    {
        m_lookAtPlayer = false;
        if(bodyState == State.Attacking || m_hitting)
            return;
        Patrol();
        UpdateDirection(m_direction);
    }

    void PlaySoundWalk()
    {
        if(m_soundWalk != null || isDead)
            return;
        m_soundWalk = SoundManager.PlaySound3D(m_audioWalk, 10, true, transform.position);
    }
    void StopSoundWalk()
    {
        if(m_soundWalk != null)
            Destroy(m_soundWalk);
    }

    void OnBounce(Vector2 velocity)
    {
        myRigidbody.velocity = velocity;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        bool isLeft = transform.localScale.x < 0;
        Gizmos.DrawRay(transform.position + new Vector3(0, 1), (isLeft ? Vector3.left : Vector3.right) * m_attackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(m_activeRange.bounds.center, m_attackAreaSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(m_followRange.bounds.center, m_followAreaSize);
    }
}
