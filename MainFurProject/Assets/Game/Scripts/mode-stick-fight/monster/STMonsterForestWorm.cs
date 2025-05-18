using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STMonsterForestWorm : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private LayerMask m_wallLayer;
    [SerializeField] private float m_shakeCameraDistance = 10;
    [SerializeField] private float m_riseDelay;
    [SerializeField] private float m_burrowDelay;
    [SerializeField] private float m_bulletSpeed;
    [SerializeField] private int m_bulletDamage = 20;
    [SerializeField] private float m_rangeRadius;
    [SerializeField] private float m_riseZoneSize;
    [SerializeField] private float m_riseZoneOffset;

    [Header("Reference")]
    [SerializeField] private Transform m_shootPos;
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private CircleCollider2D m_rangeCollider;
    [SerializeField] private BoxCollider2D m_riseZoneCollider;
    [SerializeField] private GameObject m_body;
    [SerializeField] private GameObject m_bullet;
    [SerializeField] private AudioClip m_audioAttack;
    [SerializeField] private AudioClip m_audioDead;
    [SerializeField] private AudioClip m_audioRise;
    [SerializeField] private AudioClip m_audioBurrow;

    private int m_direction;
    private Vector2 m_baseScale;
    private Bounds m_bounds;
    private BoxCollider2D m_collider;
    private bool m_playerInRange;
    private bool m_idle;

    private const string ANIM_IDLE = "idle";
    private const string ANIM_BURROW = "down";
    private const string ANIM_RISE = "up";
    private const string ANIM_ATTACK = "attack";


    // Setup
    public override void Awake()
    {
        base.Awake();

        m_rangeCollider.radius = m_rangeRadius;

        Vector2 offset = new Vector2(m_riseZoneOffset, 0);
        Vector2 size = new Vector2(m_riseZoneSize * 2, 0.8f);
        m_riseZoneCollider.offset = offset;
        m_riseZoneCollider.size = size;

        m_collider = GetComponent<BoxCollider2D>();
        m_baseScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        m_direction = transform.localScale.x > 0 ? 1 : -1;
        m_bounds = m_collider.bounds;
        m_riseZoneCollider.transform.parent = transform.parent;

        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);

        SetInteractable(true);

        healthProgress.transform.parent.transform.up = Vector3.up;

        spine.AnimationState.Event += OnAnimEvent;
        spine.AnimationState.Complete += OnAnimComplete;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        spine.AnimationState.Event -= OnAnimEvent;
        spine.AnimationState.Complete -= OnAnimComplete;
    }

    private void Start()
    {
        if (!myRigidbody.simulated)
            return;
        Idle();
    }

    private void OnDisable()
    {
        if(m_riseZoneCollider != null)  
            m_riseZoneCollider.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        m_riseZoneCollider.gameObject.SetActive(true);
    }

    // State transitions
    public void Rise()
    {
        if (isDead)
            return;

        if (m_audioRise != null)
            SoundManager.PlaySound(m_audioRise, false);

        transform.position = GetRandomPointOnGround();
        SetDirectionToPlayer();
        spine.AnimationState.SetAnimation(0, ANIM_RISE, false);
    }

    public void Burrow()
    {
        if (isDead)
            return;

        bodyState = State.Normal;
        m_idle = false;

        if (m_audioBurrow != null)
            SoundManager.PlaySound(m_audioBurrow, false);

        spine.AnimationState.SetAnimation(0, ANIM_BURROW, false);
    }

    public override void Attack()
    {
        if (isDead)
            return;

        base.Attack();
        bodyState = State.Attacking;
        m_idle = false;
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK, true);
    }

    public override void Dead()
    {
        base.Dead();

        myRigidbody.bodyType = RigidbodyType2D.Dynamic;
        m_body.SetActive(false);

        if (m_audioDead != null)
            SoundManager.PlaySound(m_audioDead, false);
    }

    void Idle()
    {
        bodyState = State.Normal;
        m_idle = true;

        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
    }

    private IEnumerator IScheduleBurrow()
    {
        yield return new WaitForSeconds(m_burrowDelay);

        Burrow();
    }

    private IEnumerator IScheduleRise()
    {
        yield return new WaitForSeconds(m_riseDelay);

        Rise();
    }


    // Events
    public override void PlayerInRange(Collider2D other)
    {
        if (isDead || m_playerInRange)
            return;
        if (bodyState == State.Attacking)
            return;

        m_playerInRange = true;

        SetDirectionToPlayer();

        if(m_idle)
            Attack();
    }

    public override void PlayerOutRange(Collider2D other)
    {
        if (isDead || !m_playerInRange)
            return;

        m_playerInRange = false;
    }

    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        Destroy(m_rangeCollider.gameObject);
        Destroy(m_riseZoneCollider.gameObject);
        Destroy(gameObject);
    }

    public override void OnResumeAfterHit()
    {

    }
    
    public override void StartBehaviour()
    {
        base.StartBehaviour();
        Idle();
        m_riseZoneCollider.gameObject.SetActive(true);
    }

    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
        m_riseZoneCollider.gameObject.SetActive(false);
    }


    // Update
    private void FixedUpdate()
    {
        //if (isDead || !startBehaviour)
        //    return;
        //if (!m_moving)
        //    return;
        //CheckGrounded();
        //if (!m_isGrounded)
        //    return;

        //// attack
        //if (bodyState == State.Attacking)
        //{
        //    m_currentSpeed += m_acceleration * Time.fixedDeltaTime;
        //    m_currentSpeed = m_currentSpeed > m_maxMoveSpeed ? m_maxMoveSpeed : m_currentSpeed;

        //    myRigidbody.velocity = (m_direction > 0 ? Vector2.right : Vector2.left) * m_currentSpeed;

        //    if (CheckWall() || CheckAbyss())
        //    {
        //        Shrink();
        //    }
        //}
    }

    private void OnDrawGizmosSelected()
    {
        m_rangeCollider.radius = m_rangeRadius;

        Vector2 offset = new Vector2(m_riseZoneOffset, 0);
        Vector2 size = new Vector2(m_riseZoneSize * 2, 0.8f);
        m_riseZoneCollider.offset = offset;
        m_riseZoneCollider.size = size;
    }
    

    // Animation transitions
    void OnAnimComplete(TrackEntry trackEntry)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case ANIM_RISE:
                if (m_playerInRange)
                    Attack();
                else
                    Idle();
                break;

            case ANIM_BURROW:
                StartCoroutine(IScheduleRise());
                break;

            case ANIM_ATTACK:
                Idle();
                StartCoroutine(IScheduleBurrow());
                break;
        }
    }

    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        switch (e.Data.Name)
        {
            case "ready":
                if (trackEntry.Animation.Name == ANIM_RISE)
                {
                    SetInteractable(true);
                }
                if (trackEntry.Animation.Name == ANIM_BURROW)
                {
                    SetInteractable(false);
                }
                break;

            case "attack":
                if(trackEntry.Animation.Name == ANIM_ATTACK)
                {
                    Shoot();
                }
                break;
        }
    }


    // Helper functions
    void UpdateDirection(int direction)
    {
        m_direction = direction;
        transform.localScale = new Vector3(m_baseScale.x * direction, m_baseScale.y, 1);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
    }

    void SetDirectionToPlayer()
    {
        int direction = transform.position.x > player.transform.position.x ? -1 : 1;

        UpdateDirection(direction);
    }

    private void SetInteractable(bool interactable)
    {
        m_body.SetActive(interactable);
        m_collider.enabled = interactable;
        canBeKilled = interactable;
        canBeKnockback = interactable;
    }

    void Shoot()
    {
        Vector2 vectorToTarget = ((player.transform.position + Vector3.up) - transform.position).normalized;

        float angleOfBullet = Vector2.SignedAngle(Vector2.right, vectorToTarget);
        GameObject bullet = Instantiate(m_bullet, m_shootPos.position, Quaternion.Euler(0, 0, angleOfBullet), transform.parent);
        bullet.SetActive(true);

        STEnemyBullet bulletComp = bullet.GetComponent<STEnemyBullet>();
        bulletComp.Init(vectorToTarget, m_bulletSpeed);
        bullet.GetComponent<STObjectDealDamage>().UpdateDamage(m_bulletDamage);

        if(m_audioAttack != null)
            SoundManager.PlaySound(m_audioAttack, false);
    }

    private Vector3 GetRandomPointOnGround()
    {
        float rand = Random.Range(-1f, 1f);

        Vector3 newPos = rand * m_riseZoneCollider.transform.right * m_riseZoneSize;

        newPos += m_riseZoneCollider.bounds.center;

        return newPos;
    }
}
