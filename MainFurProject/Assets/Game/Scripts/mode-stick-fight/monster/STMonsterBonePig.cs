using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class STMonsterBonePig : STObjectMonster
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

    [Header("Reference")]
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private BoxCollider2D m_activeRangeCollider;
    [SerializeField] private GameObject m_body;
    [SerializeField] private AudioClip m_audioWalk;
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

    private const string ANIM_ATTACK = "attack";
    private const string ANIM_IDLE = "idle";
    private const string ANIM_RUN = "run";
    private const string ANIM_WALK = "walk";
    private const string ANIM_HIT = "hit";

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
        
        spine.AnimationState.Complete += OnAnimComplete;
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
        StopSoundMove();

        spine.AnimationState.Complete -= OnAnimComplete;
    }
    public override void Attack()
    {
        if(isDead)
            return;
        base.Attack();
        UpdateDirection(transform.position.x > player.transform.position.x ? -1 : 1);
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK, false);
        myRigidbody.velocity = Vector2.zero;
        m_moving = false;
    }
    public override void AttackComplete()
    {
        if(isDead)
            return;
        base.AttackComplete();
        if (m_playerInRange)
            Attack();
        else
            Patrol();
    }
    public override void PlayerInRange(Collider2D other)
    {
        if(isDead)
            return;
        m_playerInRange = true;
        if(bodyState != State.Attacking)
            Attack();
    }
    public override void PlayerOutRange(Collider2D other)
    {
        if(isDead)
            return;
        m_playerInRange = false;
    }
    public override void Dead()
    {
        base.Dead();
        m_body.SetActive(false);
        SoundManager.PlaySound(m_audioDead, false);
        StopSoundMove();
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
        Patrol();
        m_activeRangeCollider.gameObject.SetActive(true);
    }
    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
        m_activeRangeCollider.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (!myRigidbody.simulated)
            return;
        Patrol();
    }

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
        if(bodyState == State.Attacking)
        {
            myRigidbody.velocity = (m_direction > 0 ? Vector2.right : Vector2.left) * m_runSpeed;

            if (CheckWall())
            {
                myRigidbody.velocity = Vector2.zero;
                m_moving = false;
                spine.AnimationState.SetAnimation(0, ANIM_HIT, false);
                SoundManager.PlaySound(m_audioHitWall, false);
                if(Vector3.Distance(transform.position, player.transform.position) <= m_shakeCameraDistance)
                    GameController.ShakeCamera();
            }
            return;
        }

        // wander
        myRigidbody.velocity = (m_direction > 0 ? Vector2.right : Vector2.left) * m_walkSpeed;
        if (CheckWall() || CheckAbyss())
            UpdateDirection(-m_direction);
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        if(isDead)
            return;
        switch(trackEntry.Animation.Name)
        {
            case ANIM_ATTACK:
                m_moving = true;
                spine.AnimationState.SetAnimation(0, ANIM_RUN, true);
                StopSoundMove();
                PlaySoundMove(m_audioRun);
                break;
            case ANIM_HIT:
                AttackComplete();
                break;
        }
    }

    void Patrol()
    {
        m_moving = true;
        spine.AnimationState.SetAnimation(0, ANIM_WALK, true);
        StopSoundMove();
        PlaySoundMove(m_audioWalk);
    }

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

    private void OnDrawGizmosSelected()
    {
        m_activeRangeCollider.size = m_activeRangeSize;
        m_activeRangeCollider.offset = m_activeRangeOffset;
    }
}
