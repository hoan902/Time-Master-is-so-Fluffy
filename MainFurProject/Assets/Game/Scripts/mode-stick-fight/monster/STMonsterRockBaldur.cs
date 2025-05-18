using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class STMonsterRockBaldur : STObjectMonster
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
    [SerializeField] private float m_delayDetect;
    [SerializeField] private float m_hitWallKnockbackStrength;
    [SerializeField] private Vector2 m_rangeSize;
    [SerializeField] private Vector2 m_rangeOffset;

    [Header("Reference")]
    [SerializeField] private HurtFlashEffect m_flashEffect;
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private BoxCollider2D m_activeRangeCollider;
    [SerializeField] private GameObject m_body;
    [SerializeField] private AudioClip m_audioRun;
    [SerializeField] private AudioClip m_audioHitWall;
    [SerializeField] private AudioClip m_audioDead;

    private int m_direction;
    private bool m_isGrounded;
    private Vector2 m_baseScale;
    private Bounds m_bounds;
    private GameObject m_soundMove;
    private BoxCollider2D m_collider;
    private bool m_moving;
    private bool m_playerInRange;
    private bool m_hitWall;
    private bool m_hitByWeapon;
    private bool m_canDetect;

    private const string ANIM_START_ROLL = "attack1";
    private const string ANIM_ROLL = "attack2";
    private const string ANIM_END_ROLL = "attack3";
    private const string ANIM_IDLE = "idle";


    // Set up
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

        spine.AnimationState.Complete += OnAnimComplete;
    }

    private void Start()
    {
        if (!myRigidbody.simulated)
            return;
        Idle();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        //StopSoundMove();

        spine.AnimationState.Complete -= OnAnimComplete;
    }


    // Update
    private void FixedUpdate()
    {
        if (isDead || !startBehaviour)
            return;

        // recover animation
        CheckGrounded();
        if (m_hitWall && m_isGrounded)
        {
            m_hitWall = false;
            spine.AnimationState.SetAnimation(0, ANIM_END_ROLL, false);
            return;
        }
        if (m_hitByWeapon && m_isGrounded)
        {
            m_hitByWeapon = false;
            spine.AnimationState.SetAnimation(0, ANIM_END_ROLL, true);
            return;
        }

        if (!m_moving)
            return;
        CheckGrounded();
        if (!m_isGrounded)
            return;

        // roll
        if (bodyState == State.Attacking)
        {
            myRigidbody.velocity = (m_direction > 0 ? Vector2.right : Vector2.left) * m_runSpeed;
            
            if (CheckWall())
            {
                myRigidbody.velocity = Vector2.zero;
                m_moving = false;

                // knockback on hitting wall
                Vector3 direction = Quaternion.AngleAxis((m_direction > 0 ? 1 : -1) * 30, Vector3.forward) * Vector3.up;

                if (myRigidbody.bodyType == RigidbodyType2D.Dynamic && knockbackStrength > 0)
                {
                    myRigidbody.AddForce(direction * m_hitWallKnockbackStrength, ForceMode2D.Impulse);
                }

                m_hitWall = true;

                // set to opposite direction
                UpdateDirection(m_direction > 0 ? -1 : 1);

                StopSoundMove();
                SoundManager.PlaySound(m_audioHitWall, false);


                //SoundManager.PlaySound(m_audioHitWall, false);
                if (Vector3.Distance(transform.position, player.transform.position) <= m_shakeCameraDistance)
                    GameController.ShakeCamera();
            }
            return;
        }
    }

    private void OnDrawGizmosSelected()
    {
        m_activeRangeCollider.offset = m_rangeOffset;
        m_activeRangeCollider.size = m_rangeSize;
    }


    // State transitions
    private void Idle()
    {
        m_moving = false;
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
    }

    public override void Attack()
    {
        if (isDead)
            return;
        base.Attack();
        UpdateDirection(transform.position.x > player.transform.position.x ? -1 : 1);
        spine.AnimationState.SetAnimation(0, ANIM_START_ROLL, false);
        myRigidbody.velocity = Vector2.zero;
        m_moving = false;
        m_canDetect = false;
    }

    public override void AttackComplete()
    {
        if (isDead)
            return;
        base.AttackComplete();
        if (m_playerInRange)
            Attack();
        else
            Idle();
    }

    public override void Dead()
    {
        base.Dead();
        m_body.SetActive(false);
        if(m_audioDead != null)
            SoundManager.PlaySound(m_audioDead, false);
        StopSoundMove();
    }

    public override void StartBehaviour()
    {
        base.StartBehaviour();
        Idle();
        m_activeRangeCollider.gameObject.SetActive(true);
    }

    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
        m_activeRangeCollider.gameObject.SetActive(false);
    }


    // Events
    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        base.OnHit(attackerInfor);
        m_flashEffect.Flash();

        if(hitStopAttack)
        {
            m_moving = false;
            m_hitByWeapon = true;
        }
    }

    public override void PlayerInRange(Collider2D other)
    {
        if (isDead)
            return;
        if (!m_canDetect)
            return;

        m_playerInRange = true;
        if (bodyState != State.Attacking)
            Attack();
    }

    public override void PlayerOutRange(Collider2D other)
    {
        if (isDead)
            return;
        m_playerInRange = false;
    }
    
    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        Destroy(gameObject);
    }

    public override void OnResumeAfterHit()
    {
        // override to prevent animation reset to idle after getting hit
        // and knocked back after landing
    }


    // Animation transitions
    void OnAnimComplete(TrackEntry trackEntry)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case ANIM_START_ROLL:
                m_moving = true;
                spine.AnimationState.SetAnimation(0, ANIM_ROLL, true);
                PlaySoundMove(m_audioRun);
                break;
            case ANIM_END_ROLL:
                AttackComplete();
                bodyState = State.Normal;
                StartCoroutine(IDelaySetCanDetect());
                break;
        }
    }


    // Helper functions
    private IEnumerator IDelaySetCanDetect()
    {
        yield return new WaitForSeconds(m_delayDetect);

        m_canDetect = true;
    }

    void UpdateDirection(int direction)
    {
        m_direction = direction;
        transform.localScale = new Vector3(m_baseScale.x * direction, m_baseScale.y, 1);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
    }

    void CheckGrounded(float extraLength = 0f)
    {
        float rayCastLength = m_collider.bounds.extents.y + 0.1f + extraLength;

        RaycastHit2D leftCast = Physics2D.Raycast(m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, 0), Vector2.down, rayCastLength, m_groundLayer);
        RaycastHit2D rightCast = Physics2D.Raycast(m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, 0), Vector2.down, rayCastLength, m_groundLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, 0), Vector3.down * rayCastLength, Color.yellow);
        Debug.DrawRay(m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, 0), Vector3.down * rayCastLength, Color.yellow);
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
        m_soundMove.transform.parent = transform;
    }

    void StopSoundMove()
    {
        if (m_soundMove != null)
            Destroy(m_soundMove);
    }
}
