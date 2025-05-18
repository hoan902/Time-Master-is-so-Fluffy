using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class STMonsterFlameGoby : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_bulletDamage = 10;
    [SerializeField] private float m_bulletHeight = 10f;
    [SerializeField] private float m_bulletDistance = 3f;
    [SerializeField] private float m_walkSpeed = 3f;
    [SerializeField] private float m_jumpDistance = 3f;
    [SerializeField] private float m_jumpForce = 30f;
    [SerializeField] private float m_attackCooldown = 5f;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private LayerMask m_wallLayer;
    [SerializeField] private Vector2 m_activeRangeSize = Vector2.one;
    [SerializeField] private Vector2 m_activeRangeOffset = Vector2.zero;
    [SerializeField] private float m_shakeCameraDistance = 10;

    [Header("Reference")]
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private STObjectDealDamage m_bulletDamageDealer;
    [SerializeField] private BoxCollider2D m_activeRangeCollider;
    [SerializeField] private GameObject m_body;
    [SerializeField] private GameObject m_bullet;
    [SerializeField] private Transform m_shotPoint;
    [SerializeField] private AudioClip m_audioWalk;
    [SerializeField] private AudioClip m_audioRun;
    [SerializeField] private AudioClip m_audioHitWall;
    [SerializeField] private AudioClip m_audioDead;
    [SerializeField] private AudioClip m_audioLanding;

    [Header("Anim Name")]
    [SpineAnimation]
    [SerializeField] private string m_animReadyAtk;
    [SpineAnimation]
    [SerializeField] private string m_animUp;
    [SpineAnimation]
    [SerializeField] private string m_animDown;
    [SpineAnimation]
    [SerializeField] private string m_animAtkEnd;
    [SpineAnimation]
    [SerializeField] private string m_animHit;
    [SpineAnimation]
    [SerializeField] private string m_animIdle;

    private int m_direction;
    private bool m_isGrounded;
    private Vector2 m_baseScale;
    private Bounds m_bounds;
    private GameObject m_soundMove;
    private BoxCollider2D m_collider;
    private bool m_moving;
    private bool m_playerInRange;
    private bool m_isAtkAvaliable = true;
    private Coroutine m_jumpCoroutine;

    private const string ANIM_ATTACK = "attack_1ready_attack";
    private const string ANIM_IDLE = "idle";
    private const string ANIM_RUN = "attack_2up";
    private const string ANIM_WALK = "attack_3down";

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
        m_bulletDamageDealer.UpdateDamage(m_bulletDamage);

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
        base.Attack();
        UpdateDirection(transform.position.x > player.transform.position.x ? -1 : 1);
        spine.AnimationState.SetAnimation(0, m_animReadyAtk, false);
        myRigidbody.velocity = Vector2.zero;
    }
    public override void AttackComplete()
    {
        base.AttackComplete();
        if (m_playerInRange && m_isAtkAvaliable)
            Attack();
    }
    public override void PlayerInRange(Collider2D other)
    {
        m_playerInRange = true;
        if (bodyState != State.Attacking && m_isAtkAvaliable)
            Attack();
    }
    public override void PlayerOutRange(Collider2D other)
    {
        m_playerInRange = false;
    }
    public override void Dead()
    {
        StopAllCoroutines();
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
    }
    private void FixedUpdate()
    {
        if (isDead || !startBehaviour)
            return;
        CheckGrounded();
        if (!m_isGrounded)
        {
            return;
        }
        if (CheckWall() || CheckAbyss())
            UpdateDirection(-m_direction);
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case string atk when atk == m_animReadyAtk:
                int rand = UnityEngine.Random.Range(0, 3);
                int tempDirection = m_direction;
                if (rand == 2)
                    tempDirection = -m_direction;
                myRigidbody.velocity = (tempDirection > 0 ? new Vector2(m_jumpDistance, m_jumpForce) : new Vector2(-m_jumpDistance, m_jumpForce));
                if (bodyState == State.Attacking)
                {
                    if (m_jumpCoroutine != null)
                        StopCoroutine(m_jumpCoroutine);
                    m_jumpCoroutine = StartCoroutine(IEJumpToPlayer());
                    return;
                }
                StopSoundMove();
                PlaySoundMove(m_audioRun);
                break;
            case string getHit when getHit == m_animHit:
                AttackComplete();
                break;
            case string endAtk when endAtk == m_animAtkEnd:
            case string downAtk when downAtk == m_animDown:
                spine.AnimationState.SetAnimation(0, m_animIdle, true);
                break;
        }
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

    //------------------- IENumerator -------------------------
    IEnumerator IEJumpToPlayer()
    {
        yield return new WaitUntil(() => !m_isGrounded);
        if (myRigidbody.velocity.y > 0)
            spine.AnimationState.SetAnimation(0, m_animUp, false);
        if (CheckWall())
        {
            myRigidbody.velocity = Vector2.zero;
            spine.AnimationState.SetAnimation(0, m_animHit, false);
            SoundManager.PlaySound(m_audioHitWall, false);
            if (Vector3.Distance(transform.position, player.transform.position) <= m_shakeCameraDistance)
                GameController.ShakeCamera();
        }
        yield return new WaitUntil(() => !m_isGrounded && myRigidbody.velocity.y < 0f);
            spine.AnimationState.SetAnimation(0, m_animDown, false);
        if (CheckWall())
        {
            myRigidbody.velocity = Vector2.zero;
            spine.AnimationState.SetAnimation(0, m_animHit, false);
            SoundManager.PlaySound(m_audioHitWall, false);
            if (Vector3.Distance(transform.position, player.transform.position) <= m_shakeCameraDistance)
                GameController.ShakeCamera();
        }
        
        yield return new WaitUntil(() => m_isGrounded);
        spine.AnimationState.SetAnimation(0, m_animAtkEnd, false);
        GameController.ShakeCameraWeak();
        SoundManager.PlaySound(m_audioLanding, false);
        GameObject fireBallLeft = Instantiate(m_bullet, m_shotPoint.position, Quaternion.identity, transform.root);
        GameObject fireBallRight = Instantiate(m_bullet, m_shotPoint.position, Quaternion.identity, transform.root);
        
        //-- Fireball Left
        fireBallLeft.SetActive(true);
        Rigidbody2D fireBallLeftRgbd = fireBallLeft.GetComponent<Rigidbody2D>();
        Vector2 targetPosLeft = new Vector2(transform.position.x - m_bulletDistance, fireBallLeft.transform.position.y);
        Vector2 finalVelocityLeft = Blobcreate.ProjectileToolkit.Projectile.VelocityByHeight(transform.position, targetPosLeft, m_bulletHeight);
        fireBallLeftRgbd.AddForce(finalVelocityLeft * fireBallLeftRgbd.mass, ForceMode2D.Impulse);
        fireBallLeft.GetComponent<STEnemyBullet>().Init(Vector2.zero, 0, isClockWise: true);

        //-- Fireball Right
        fireBallRight.SetActive(true);
        Rigidbody2D fireBallRightRgbd = fireBallRight.GetComponent<Rigidbody2D>();
        Vector2 targetPosRight = new Vector2(transform.position.x + m_bulletDistance, fireBallRight.transform.position.y);
        Vector2 finalVelocityRight = Blobcreate.ProjectileToolkit.Projectile.VelocityByHeight(transform.position, targetPosRight, m_bulletHeight);
        fireBallRightRgbd.AddForce(finalVelocityRight * fireBallRightRgbd.mass, ForceMode2D.Impulse);
        fireBallRight.GetComponent<STEnemyBullet>().Init(Vector2.zero, 0);

        //
        m_isAtkAvaliable = false;
        AttackComplete();
        yield return new WaitForSeconds(m_attackCooldown);
        m_isAtkAvaliable = true;
        AttackComplete();
    }
}
