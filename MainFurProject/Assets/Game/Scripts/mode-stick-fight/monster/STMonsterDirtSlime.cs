using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class STMonsterDirtSlime : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private LayerMask m_wallLayer;
    [SerializeField] private float m_riseDelay;
    [SerializeField] private float m_acceleration;
    [SerializeField] private float m_maxMoveSpeed;
    [SerializeField] private bool m_followPlayerOnActive;
    [SerializeField] private Vector2 m_rangeSize;
    [SerializeField] private Vector2 m_rangeOffset;

    [Header("Reference")]
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private BoxCollider2D m_rangeCollider;
    [SerializeField] private GameObject m_body;
    [SerializeField] private AudioClip m_audioRun;
    [SerializeField] private AudioClip m_audioDead;
    [SerializeField] private AudioClip m_audioRise;
    [SerializeField] private AudioClip m_audioShrink;

    private int m_direction;
    private bool m_isGrounded;
    private Vector2 m_baseScale;
    private Bounds m_bounds;
    private GameObject m_soundMove;
    private BoxCollider2D m_collider;
    private bool m_moving;
    private bool m_active;
    private float m_currentSpeed;

    private const string ANIM_IDLE = "idle_vung_sinh";
    private const string ANIM_MOVE = "move";
    private const string ANIM_RISE = "troi_day";
    private const string ANIM_SHRINK = "chui_xuong";


    // Setup
    public override void Awake()
    {
        base.Awake();
        m_collider = GetComponent<BoxCollider2D>();
        m_baseScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        m_direction = transform.localScale.x > 0 ? 1 : -1;
        m_bounds = m_collider.bounds;
        m_rangeCollider.transform.parent = transform.parent;
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        m_active = false;

        SetInteractable(false);

        spine.AnimationState.Event += OnAnimEvent;
        spine.AnimationState.Complete += OnAnimComplete;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        StopSoundMove();

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
        if(m_rangeCollider != null)
            m_rangeCollider.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        m_rangeCollider.gameObject.SetActive(true);
    }


    // State transitions
    public void Rise()
    {
        if (isDead)
            return;

        bodyState = State.Attacking;
        myRigidbody.velocity = Vector2.zero;
        m_moving = false;

        if(m_audioRise != null)
            SoundManager.PlaySound(m_audioRise, false);

        spine.AnimationState.SetAnimation(0, ANIM_RISE, false);
    }

    public void Shrink()
    {
        if (isDead)
            return;

        StopSoundMove();
        bodyState = State.Normal;
        myRigidbody.velocity = Vector2.zero;

        m_moving = false;

        if (m_audioShrink != null)
            SoundManager.PlaySound(m_audioShrink, false);

        spine.AnimationState.SetAnimation(0, ANIM_SHRINK, false);
    }

    public override void Attack()
    {
        if (isDead)
            return;

        base.Attack();
        bodyState = State.Attacking;
        myRigidbody.velocity = Vector2.zero;
        m_moving = true;
        spine.AnimationState.SetAnimation(0, ANIM_MOVE, true);
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
        m_body.SetActive(false);
        Destroy(m_rangeCollider.gameObject);

        if (m_audioDead != null)
            SoundManager.PlaySound(m_audioDead, false);
        
        StopSoundMove();
    }

    void Idle()
    {
        myRigidbody.velocity = Vector2.zero;
        m_currentSpeed = 0;

        bodyState = State.Normal;
        m_moving = false;

        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);

        if (m_active)
            StartCoroutine(IDelayRise());
    }


    // Events
    public override void PlayerInRange(Collider2D other)
    {
        if (isDead || !startBehaviour)
            return;
        if(m_active) 
            return;

        int startDirection = !m_followPlayerOnActive ? m_direction : transform.position.x > player.transform.position.x ? -1 : 1;

        UpdateDirection(startDirection);

        m_active = true;
        Rise();
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
        //Idle();
        m_rangeCollider.gameObject.SetActive(true);
    }

    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
        Idle();
        m_rangeCollider.gameObject.SetActive(false);
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
            m_currentSpeed += m_acceleration * Time.fixedDeltaTime;
            m_currentSpeed = m_currentSpeed > m_maxMoveSpeed ? m_maxMoveSpeed : m_currentSpeed;

            myRigidbody.velocity = (m_direction > 0 ? Vector2.right : Vector2.left) * m_currentSpeed;

            if (CheckWall() || CheckAbyss())
            {
                Shrink();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        m_rangeCollider.offset = m_rangeOffset;
        m_rangeCollider.size = m_rangeSize;
    }

    // Animation transitions
    void OnAnimComplete(TrackEntry trackEntry)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case ANIM_RISE:
                Attack();
                PlaySoundMove(m_audioRun);
                break;

            case ANIM_SHRINK:
                Idle();
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
                if (trackEntry.Animation.Name == ANIM_SHRINK)
                {
                    SetInteractable(false);
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
        m_soundMove.transform.parent = transform;
    }
    
    void StopSoundMove()
    {
        if (m_soundMove != null)
            Destroy(m_soundMove);
    }

    private void SetInteractable(bool interactable)
    {
        m_body.SetActive(interactable);
        m_collider.enabled = interactable;
        myRigidbody.bodyType = interactable? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
        canBeKilled = interactable;
        canBeKnockback = interactable;
    }

    private IEnumerator IDelayRise()
    {
        yield return new WaitForSeconds(m_riseDelay);

        UpdateDirection(-m_direction);
        Rise();
    }
}
