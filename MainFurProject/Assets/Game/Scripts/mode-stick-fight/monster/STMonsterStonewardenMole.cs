using DG.Tweening;
using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI.Extensions.Tweens;

public class STMonsterStonewardenMole : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private float m_walkSpeed = 3f;
    [SerializeField] private float m_runSpeed = 3f;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private LayerMask m_wallLayer;
    [SerializeField] private Vector2 m_activeRangeSize = Vector2.one;
    [SerializeField] private Vector2 m_activeRangeOffset = Vector2.zero;
    [SerializeField] private float m_shakeCameraDistance = 10;
    [SerializeField] private float m_detectDelay;
    [SerializeField] private float m_trackTime;
    [SerializeField] private float m_attackDelay;
    [SerializeField] private float m_maxTrackSpeed;
    [SerializeField] private float m_trackAccel;
    [SerializeField] private float m_attackJumpForce;

    [Header("Reference")]
    [SerializeField] private GameObject m_boxHitter;
    [SerializeField] private GameObject m_objectEarth;
    [SerializeField] private SkeletonAnimation m_earthSpine;
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private BoxCollider2D m_activeRangeCollider;
    [SerializeField] private GameObject m_body;
    [SerializeField] private AudioClip m_audioBurrow;
    [SerializeField] private AudioClip m_audioAttack;
    [SerializeField] private AudioClip m_audioDead;

    private int m_direction;
    private bool m_isGrounded;
    private Vector2 m_baseScale;
    private Bounds m_bounds;
    private GameObject m_soundMove;
    private BoxCollider2D m_collider;
    private bool m_moving;
    private bool m_canDetect;
    private float m_currentSpeed;
    private GameObject m_effect;
    private bool m_isUnderground;

    private const string ANIM_IDLE = "idle";
    private const string ANIM_MOVE_UNDER = "move";
    private const string ANIM_BURROW = "skill_chuidat";
    private const string ANIM_ATTACK = "skill_2_2";

    // Setups
    public override void Awake()
    {
        base.Awake();
        m_collider = GetComponent<BoxCollider2D>();
        m_baseScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        m_direction = transform.localScale.x > 0 ? 1 : -1;
        m_bounds = m_collider.bounds;
        m_activeRangeCollider.size = m_activeRangeSize;
        m_activeRangeCollider.offset = m_activeRangeOffset;
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        m_canDetect = true;

        m_activeRangeCollider.transform.parent = transform.parent;

        spine.AnimationState.Event += OnAnimEvent;
        spine.AnimationState.Complete += OnAnimComplete;
        m_earthSpine.AnimationState.Complete += OnEarthAnimComplete;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        StopSoundMove();

        spine.AnimationState.Event -= OnAnimEvent;
        spine.AnimationState.Complete -= OnAnimComplete;
        m_earthSpine.AnimationState.Complete -= OnEarthAnimComplete;
    }

    private void Start()
    {
        if (!myRigidbody.simulated)
            return;
        Idle();
    }

    private void OnDisable()
    {
        if(m_activeRangeCollider != null)
            m_activeRangeCollider.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        m_activeRangeCollider.gameObject.SetActive(true);
    }

    // State transitions
    private void Idle()
    {
        m_moving = false;
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
    }

    public void Move()
    {
        if (isDead)
            return;
        canBeKnockback = false;
        UpdateDirection(transform.position.x > player.transform.position.x ? -1 : 1);
        spine.AnimationState.SetAnimation(0, ANIM_BURROW, false);
        //myRigidbody.velocity = Vector2.zero;
        m_currentSpeed = 0;
    }

    public override void Attack()
    {
        if (isDead)
            return;
        base.Attack();
        UpdateDirection(transform.position.x > player.transform.position.x ? -1 : 1);
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK, false);
        SoundManager.PlaySound(m_audioAttack, false);
        myRigidbody.velocity = Vector2.zero;
        m_moving = false;

        myRigidbody.AddForce(Vector2.up * m_attackJumpForce, ForceMode2D.Impulse);

        SpawnEarthEffect(false);
    }

    public override void AttackComplete()
    {
        if (isDead)
            return;
        base.AttackComplete();

        Idle();
    }
    
    public override void Dead()
    {
        base.Dead();

        if(m_effect != null) Destroy(m_effect);

        Destroy(m_activeRangeCollider.gameObject);
        m_body.SetActive(false);
        SoundManager.PlaySound(m_audioDead, false);
        StopSoundMove();
    }


    // Events
    public override void PlayerInRange(Collider2D other)
    {
        if (isDead || !startBehaviour)
            return;
        if (!m_canDetect)
            return;

        m_canDetect = false;
        if (bodyState != State.Attacking)
            Move();
    }

    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        Destroy(gameObject);
    }

    public override void OnResumeAfterHit()
    {

    }

    public override void StartBehaviour()
    {
        base.StartBehaviour();
        m_activeRangeCollider.gameObject.SetActive(true);
    }

    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
        m_activeRangeCollider.gameObject.SetActive(false);
    }

    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        if(m_isUnderground) return;

        base.OnHit(attackerInfor);
    }


    // Animation transitions
    void OnAnimComplete(TrackEntry trackEntry)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case ANIM_BURROW:
                m_moving = true;
                spine.AnimationState.SetAnimation(0, ANIM_MOVE_UNDER, true);
                bodyState = State.Attacking;
                canBeKnockback = true;
                StartCoroutine(ITrackTimer());

                //StopSoundMove();
                //PlaySoundMove(m_audioRun);
                break;
            case ANIM_ATTACK:
                AttackComplete();
                StartCoroutine(IDelayDetect());
                break;
        }
    }

    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        switch(e.Data.Name)
        {
            case "ready":
                if(trackEntry.Animation.Name == ANIM_BURROW)
                {
                    SoundManager.PlaySound(m_audioBurrow, false);
                    SpawnEarthEffect(true);
                    m_body.SetActive(false);
                    m_boxHitter.SetActive(false);
                    m_isUnderground = true;
                }
                if(trackEntry.Animation.Name == ANIM_ATTACK)
                {
                    m_body.SetActive(true);
                    m_boxHitter.SetActive(true);
                    m_isUnderground = false;
                }
                break;
        }
    }

    void OnEarthAnimComplete(TrackEntry trackEntry)
    {
        Destroy(m_effect);
    }


    // Update
    private void FixedUpdate()
    {
        if (isDead || !startBehaviour)
            return;
        if (!m_moving)
            return;
        CheckGrounded();
        if (!m_isGrounded)
            return;

        // attack
        if (bodyState == State.Attacking)
        {
            m_direction = transform.position.x < player.transform.position.x ? 1 : -1;

            float distanceToPlayer = Mathf.Abs(transform.position.x - player.transform.position.x);
            m_currentSpeed += m_direction * distanceToPlayer * m_trackAccel * Time.fixedDeltaTime;

            m_currentSpeed = Mathf.Abs(m_currentSpeed) < m_maxTrackSpeed ? m_currentSpeed : Mathf.Sign(m_currentSpeed) * m_maxTrackSpeed;
            m_currentSpeed *= distanceToPlayer < 0.5f ? 0.85f : 1f;

            Vector2 moveVector = new Vector2(m_currentSpeed, 0);
            myRigidbody.velocity = moveVector;
            
            // hit wall
            if (CheckWall())
            {
                myRigidbody.velocity = Vector2.zero;
                m_moving = false;
                //SoundManager.PlaySound(m_audioHitWall, false);
                //if (Vector3.Distance(transform.position, player.transform.position) <= m_shakeCameraDistance)
                //    GameController.ShakeCamera();
            }
            return;
        }

        // wander
        myRigidbody.velocity = (m_direction > 0 ? Vector2.right : Vector2.left) * m_walkSpeed;
        if (CheckWall() || CheckAbyss())
            UpdateDirection(-m_direction);
    }


    // Helper Functions
    void UpdateDirection(int direction)
    {
        m_direction = direction;
        transform.localScale = new Vector3(m_baseScale.x * direction, m_baseScale.y, 1);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
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
        RaycastHit2D raycast = Physics2D.Raycast(m_collider.bounds.center + new Vector3(rightCheck ? m_bounds.extents.x : -m_bounds.extents.x, 0), Vector2.down, m_bounds.extents.y + 0.2f, m_groundLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center + new Vector3(rightCheck ? m_bounds.extents.x : -m_bounds.extents.x, 0), Vector2.down * (m_bounds.extents.y + 0.2f), Color.black);
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

    void PlaySoundMove(AudioClip audioClip)
    {
        if (m_soundMove != null || isDead)
            return;
        m_soundMove = SoundManager.PlaySound3D(audioClip, 10, true, transform.position);
    }
    
    void StopSoundMove()
    {
        if (m_soundMove != null)
            Destroy(m_soundMove);
    }

    private IEnumerator IDelayDetect()
    {
        while(!m_isGrounded)
        {
            CheckGrounded();
            yield return null;
        }

        yield return new WaitForSeconds(m_detectDelay);

        m_canDetect = true;
    }

    private IEnumerator ITrackTimer()
    {
        yield return new WaitForSeconds(m_trackTime);

        m_moving = false;

        StartCoroutine(IDelayAttack());
    }

    private IEnumerator IDelayAttack()
    {
        yield return new WaitForSeconds(m_attackDelay);

        Attack();
    }

    private void SpawnEarthEffect(bool onBurrow)
    {
        if (isDead) return;
        if(m_effect != null) Destroy(m_effect);

        RaycastHit2D centerCast = Physics2D.Raycast(m_collider.bounds.center, Vector2.down, 10f, m_groundLayer);

        if(centerCast.collider != null)
        {
            m_effect = Instantiate(m_objectEarth, centerCast.point, Quaternion.identity);
            m_effect.SetActive(true);

            string animToPlay = onBurrow ? ANIM_BURROW : ANIM_ATTACK;

            m_effect.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, animToPlay, false);
            m_effect.GetComponent<SkeletonAnimation>().AnimationState.Complete += OnEarthAnimComplete;
        }
    }

    private void OnDrawGizmosSelected()
    {
        m_activeRangeCollider.size = m_activeRangeSize;
        m_activeRangeCollider.offset = m_activeRangeOffset;
    }
}
