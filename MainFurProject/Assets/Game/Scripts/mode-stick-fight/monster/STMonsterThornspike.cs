using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STMonsterThornspike : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private float m_walkSpeed = 3f;
    [SerializeField] private float m_runSpeed = 3f;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private LayerMask m_wallLayer;
    [SerializeField] private Vector2 m_followRangeSize = Vector2.one;
    [SerializeField] private Vector2 m_followRangeOffset = Vector2.zero; 
    [SerializeField] private Vector2 m_attackRangeSize = Vector2.one;
    [SerializeField] private Vector2 m_attackRangeOffset = Vector2.zero;
    [SerializeField] private int m_attackDamage = 15;
    [SerializeField] private float m_attackForwardMomentum = 10f;
    [SerializeField] private float m_attackColliderDuration = 0.1f;
    [SerializeField] private float m_attackCooldown = 1f;
    //[SerializeField] private float m_shakeCameraDistance = 10;

    [Header("Reference")]
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private STObjectDealDamage m_attackDamageDealer;
    [SerializeField] private BoxCollider2D m_followRangeCollider;
    [SerializeField] private BoxCollider2D m_attackRangeCollider;
    [SerializeField] private GameObject m_body;
    [SerializeField] private GameObject m_attack;
    [SerializeField] private AudioClip m_audioWalk;
    [SerializeField] private AudioClip m_audioAttack;
    [SerializeField] private AudioClip m_audioDead;

    private int m_direction;
    private bool m_isGrounded;
    private Vector2 m_baseScale;
    private Bounds m_bounds;
    private GameObject m_soundMove;
    private BoxCollider2D m_collider;
    private bool m_moving;
    private bool m_follow;
    private bool m_playerDetect;
    private bool m_attackIsCooldown;

    private const string ANIM_ATTACK = "attack";
    private const string ANIM_WALK = "idle";


    // Setup
    public override void Awake()
    {
        base.Awake();
        m_collider = GetComponent<BoxCollider2D>();
        m_baseScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        m_direction = transform.localScale.x > 0 ? 1 : -1;
        m_bounds = m_collider.bounds;
        //m_followRangeCollider.transform.parent = transform.parent;
        m_followRangeCollider.size = m_followRangeSize;
        m_followRangeCollider.offset = m_followRangeOffset;
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);

        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        m_attack.GetComponent<STObjectDealDamage>().UpdateDamage(m_attackDamage);

        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        StopSoundMove();

        spine.AnimationState.Complete -= OnAnimComplete;
        spine.AnimationState.Event -= OnAnimEvent;
    }

    private void Start()
    {
        if (!myRigidbody.simulated)
            return;

        Patrol();
    }

    private void OnDisable()
    {
        if(m_followRangeCollider != null)
            m_followRangeCollider.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        m_followRangeCollider.gameObject.SetActive(true);
    }


    // State transitions
    public override void Attack()
    {
        if (isDead)
            return;

        base.Attack();
        bodyState = State.Attacking;
        UpdateDirection(transform.position.x > player.transform.position.x ? -1 : 1);
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK, false);
        myRigidbody.velocity = Vector2.zero;
        m_moving = false;
        m_attackIsCooldown = true;
    }
    
    public void Follow()
    {
        if (isDead)
            return;

        m_moving = true;
        m_follow = true;
        bodyState = State.Normal;

        spine.AnimationState.SetAnimation(0, ANIM_WALK, true);

        StopSoundMove();
        PlaySoundMove(m_audioWalk);
    }
    
    void Patrol()
    {
        if (isDead)
            return;

        m_follow = false;
        m_moving = true;
        bodyState = State.Normal;

        spine.AnimationState.SetAnimation(0, ANIM_WALK, true);

        StopSoundMove();
        PlaySoundMove(m_audioWalk);
    }

    public override void Dead()
    {
        base.Dead();
        myRigidbody.velocity = Vector2.zero;
        m_body.SetActive(false);
        if(m_audioDead != null)
            SoundManager.PlaySound(m_audioDead, false);
        StopSoundMove();
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

        // wander
        float moveSpeed = m_walkSpeed;

        if(m_follow)
        {
            moveSpeed = m_runSpeed;

            bool needToFlip = (myRigidbody.position.x < (player.transform.position.x - 1)) || (myRigidbody.position.x > (player.transform.position.x + 1));
            if (needToFlip)
                UpdateDirection(player.transform.position.x > myRigidbody.position.x ? 1 : -1);
        }

        myRigidbody.velocity = (m_direction > 0 ? Vector2.right : Vector2.left) * moveSpeed;

        if (CheckWall() || CheckAbyss())
            UpdateDirection(-m_direction);
    }


    // Animation transitions
    void OnAnimComplete(TrackEntry trackEntry)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case ANIM_ATTACK:
                StartCoroutine(IAttackCooldown());

                if (m_playerDetect)
                    Follow();
                else
                    Patrol();

                break;
        }
    }

    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        switch (e.Data.Name)
        {
            case "attack":
                if (trackEntry.Animation.Name == ANIM_ATTACK)
                {
                    StartCoroutine(IAttack());
                }
                break;
        }
    }


    // Events
    public void PlayerDetect(Collider2D other)
    {
        if (isDead || !startBehaviour)
            return;

        m_playerDetect = true;

        if(bodyState != State.Attacking)
            Follow();
    }

    public void PlayerUndetect(Collider2D other)
    {
        if (isDead)
            return;

        m_playerDetect = false;

        if(bodyState != State.Attacking)
            Patrol();
    }

    public override void PlayerInRange(Collider2D other)
    {
        if (isDead || bodyState == State.Attacking || !startBehaviour)
            return;

        if (!m_attackIsCooldown) Attack();
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
        m_followRangeCollider.gameObject.SetActive(true);
    }

    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
        m_followRangeCollider.gameObject.SetActive(false);
    }


    // Helper function
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
        RaycastHit2D raycast = Physics2D.Raycast(m_collider.bounds.center + Vector3.down, rightCheck ? Vector2.right : Vector2.left, m_bounds.extents.x + 0.2f, m_wallLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center + Vector3.down, (rightCheck ? Vector2.right : Vector2.left) * (m_bounds.extents.x + 0.2f), Color.black);
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

    void UpdateDirection(int direction)
    {
        m_direction = direction;
        transform.localScale = new Vector3(m_baseScale.x * direction, m_baseScale.y, 1);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
    }

    private IEnumerator IAttackCooldown()
    {
        yield return new WaitForSeconds(m_attackCooldown);

        m_attackIsCooldown = false;
        //bodyState = State.Normal;
    }

    private IEnumerator IAttack()
    {
        if (m_audioAttack != null)
            SoundManager.PlaySound(m_audioAttack, false);

        m_attack.SetActive(true);
        Vector2 force = new Vector2(m_direction, 0f) * m_attackForwardMomentum;
        myRigidbody.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(m_attackColliderDuration);

        m_attack.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        m_followRangeCollider.size = m_followRangeSize;
        m_followRangeCollider.offset = m_followRangeOffset;

        m_attackRangeCollider.size = m_attackRangeSize;
        m_attackRangeCollider.offset = m_attackRangeOffset;
    }
}
