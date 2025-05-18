using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STMonsterScorpion : STObjectMonster
{
    enum Stance
    {
        Defense,
        Attack
    }

    [Header("Config")]
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private float m_moveSpeed = 3;

    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private LayerMask m_wallLayer;

    // collider customization
    [SerializeField] private Vector2 m_followRangeSize = Vector2.one;
    [SerializeField] private Vector2 m_followRangeOffset = Vector2.zero;

    [SerializeField] private Vector2 m_attackRangeSize = Vector2.one;
    [SerializeField] private Vector2 m_attackRangeOffset = Vector2.zero;

    [SerializeField] private Vector2 m_defenseRangeSize = Vector2.one;
    [SerializeField] private Vector2 m_defenseRangeOffset = Vector2.zero;

    // behavior logic
    [SerializeField] private float m_stanceDuration = 3f;

    [SerializeField] private int m_attackDamage = 15;
    [SerializeField] private float m_attackForwardMomentum = 30f;
    [SerializeField] private float m_attackColliderDuration = 0.1f;
    [SerializeField] private float m_attackCooldown = 1f;

    [SerializeField] private float m_dodgeCooldown = 0.5f;
    [SerializeField] private float m_dodgeForce = 30f;

    [Header("Reference")]
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private STObjectDealDamage m_attackDamageDealer;

    [SerializeField] private GameObject m_body;
    [SerializeField] private GameObject m_attack;

    [SerializeField] private BoxCollider2D m_followRangeCollider;
    [SerializeField] private BoxCollider2D m_attackRangeCollider;
    [SerializeField] private BoxCollider2D m_defenseRangeCollider;

    [SerializeField] private AudioClip m_audioAttack;
    [SerializeField] private AudioClip m_audioDead;
    [SerializeField] private AudioClip m_audioDodge;
    [SerializeField] private AudioClip m_audioWalk;

    private int m_direction;
    private bool m_isGrounded;
    private Vector2 m_baseScale;
    private Bounds m_bounds;
    private GameObject m_soundMove;
    private BoxCollider2D m_collider;
    private bool m_moving;
    private bool m_playerDetect;
    private bool m_attackIsCooldown;
    private bool m_dodgeIsCooldown;
    private float m_stanceTimer;
    private bool m_inAction;
    private bool m_didStanceAction;
    private Stance m_currentStance;

    private const string ANIM_ATTACK = "attack";
    private const string ANIM_IDLE = "idle";
    private const string ANIM_DODGE = "dodge";
    private const string ANIM_DODGE_END = "dodge_2";
    private const string ANIM_WALK = "walk";


    // Setup
    public override void Awake()
    {
        base.Awake();
        m_collider = GetComponent<BoxCollider2D>();
        m_baseScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        m_direction = transform.localScale.x > 0 ? 1 : -1;
        m_bounds = m_collider.bounds;
        m_followRangeCollider.size = m_followRangeSize;
        m_followRangeCollider.offset = m_followRangeOffset;
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);

        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        m_attack.GetComponent<STObjectDealDamage>().UpdateDamage(m_attackDamage);

        m_currentStance = Stance.Attack;
        UpdateStance(m_currentStance);

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
        Idle();
    }


    // State transitions
    public override void Attack()
    {
        if (isDead || m_didStanceAction)
            return;

        base.Attack();

        bodyState = State.Attacking;
        m_inAction = true;
        m_didStanceAction = true;
        myRigidbody.velocity = Vector2.zero;
        m_moving = false;
        m_attackIsCooldown = true;

        UpdateDirection(transform.position.x > player.transform.position.x ? -1 : 1);

        spine.AnimationState.SetAnimation(0, ANIM_ATTACK, false);
    }

    public void Dodge()
    {
        if (isDead || m_didStanceAction)
            return;

        m_inAction = true;
        m_didStanceAction = true;
        m_moving = false;

        Vector2 dodgeForce = new Vector2(-m_direction, 0f) * m_dodgeForce;

        spine.AnimationState.SetAnimation(0, ANIM_DODGE, false);

        myRigidbody.AddForce(dodgeForce, ForceMode2D.Impulse);

        if(m_audioDodge != null)
            SoundManager.PlaySound(m_audioDodge, false);
    }

    public void Idle()
    {
        if (isDead)
            return;

        m_moving = false;
        m_inAction = false;

        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
    }

    public void Follow()
    {
        if (isDead)
            return;

        m_moving = true;

        spine.AnimationState.SetAnimation(0, ANIM_WALK, true);
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
        if (isDead)
            return;

        // Stance timer 
        if (!m_inAction)
        {
            m_stanceTimer += Time.fixedDeltaTime;

            if(m_stanceTimer >= m_stanceDuration)
            {
                m_stanceTimer = 0;
                UpdateStance(m_currentStance == Stance.Defense ? Stance.Attack : Stance.Defense);
            }
        }

        CheckGrounded();
        if (!m_isGrounded)
            return;

        // Defense stance
        if(m_currentStance == Stance.Defense && m_playerDetect)
        {
            bool needToFlip = (myRigidbody.position.x < (player.transform.position.x - 1)) || (myRigidbody.position.x > (player.transform.position.x + 1));
            if (needToFlip)
                UpdateDirection(player.transform.position.x > transform.position.x ? 1 : -1);
        }

        // Attack stance
        if(m_currentStance == Stance.Attack && m_playerDetect && m_moving)
        {
            bool needToFlip = (myRigidbody.position.x < (player.transform.position.x - 1)) || (myRigidbody.position.x > (player.transform.position.x + 1));
            if (needToFlip)
                UpdateDirection(player.transform.position.x > transform.position.x ? 1 : -1);

            myRigidbody.velocity = (m_direction > 0 ? Vector2.right : Vector2.left) * m_moveSpeed;
        }

        if(CheckAbyss())
        {
            myRigidbody.velocity = Vector2.zero;
        }
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
                Idle();
                break;

            case ANIM_DODGE:
                spine.AnimationState.SetAnimation(0, ANIM_DODGE_END, false);
                break;

            case ANIM_DODGE_END:
                StartCoroutine(IDodgeCooldown());
                Idle();
                break;
        }
    }

    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (isDead)
            return;

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

        if (!m_inAction)
        {
            m_playerDetect = true;

            if (m_currentStance == Stance.Attack && !m_moving)
                Follow();
        }
    }

    public void PlayerUndetect(Collider2D other)
    {
        if (isDead)
            return;

        m_playerDetect = false;

        if(!m_inAction)
            Idle();
    }

    public override void PlayerInRange(Collider2D other)
    {
        if (isDead || m_inAction || !startBehaviour)
            return;

        if (!m_attackIsCooldown) Attack();
    }

    public void PlayerInDefenseRange(Collider2D other)
    {
        if (isDead || m_inAction)
            return;

        if(!m_dodgeIsCooldown)
            Dodge();
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
        int direction = myRigidbody.velocity.x > 0 ? 1 : -1;

        RaycastHit2D raycast = Physics2D.Raycast(m_collider.bounds.center + new Vector3(direction * m_bounds.extents.x, 0), Vector2.down, m_bounds.extents.y + 0.2f, m_groundLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center + new Vector3(direction * m_bounds.extents.x, 0), Vector2.down * (m_bounds.extents.y + 0.2f), Color.black);
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
    }

    private IEnumerator IDodgeCooldown()
    {
        yield return new WaitForSeconds(m_dodgeCooldown);

        m_dodgeIsCooldown = false;
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

    private void UpdateStance(Stance newStance)
    {
        if (isDead)
            return;

        m_didStanceAction = false;
        m_currentStance = newStance;

        if (m_currentStance == Stance.Attack)
        {
            m_defenseRangeCollider.gameObject.SetActive(false);
            m_attackRangeCollider.gameObject.SetActive(true);

            if (m_playerDetect)
                Follow();
        }
        else
        {
            m_defenseRangeCollider.gameObject.SetActive(true);
            m_attackRangeCollider.gameObject.SetActive(false);
            Idle();
        }
    }

    private void OnDrawGizmosSelected()
    {
        m_followRangeCollider.size = m_followRangeSize;
        m_followRangeCollider.offset = m_followRangeOffset;

        m_attackRangeCollider.size = m_attackRangeSize;
        m_attackRangeCollider.offset = m_attackRangeOffset;

        m_defenseRangeCollider.size = m_defenseRangeSize;
        m_defenseRangeCollider.offset = m_defenseRangeOffset;
    }
}
