using DG.Tweening;
using Spine;
using UnityEngine;

public class STMonsterSporespike : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private float m_moveSpeed = 5f;
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_explodeDamage = 30;
    [SerializeField] private float m_explodeRadius = 3f;
    [SerializeField] private float m_explodeTime = 0.5f;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private LayerMask m_wallLayer;

    [Header("Reference")]
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private CircleCollider2D m_boxHitter;
    [SerializeField] private CircleCollider2D m_explodeCollider;
    [SerializeField] private AudioClip m_audioDead;
    [SerializeField] private AudioClip m_audioExplode;
    [SerializeField] private AudioClip m_audioWalk;

    private bool m_isGrounded;
    private BoxCollider2D m_collider;
    private int m_direction = 1;
    private Vector2 m_baseScale;
    private Bounds m_bounds;
    private GameObject m_soundWalk;
    private float m_currentExplodeRadius;

    private const string ANIM_WALK = "walk";

    public override void Awake()
    {
        base.Awake();
        m_collider = GetComponent<BoxCollider2D>();
        m_baseScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        m_direction = transform.localScale.x > 0 ? 1 : -1;
        m_bounds = m_collider.bounds;
        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
        m_currentExplodeRadius = 1;

        spine.AnimationState.Event += OnAnimEvent;
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();
        StopSoundWalk();

        spine.AnimationState.Event -= OnAnimEvent;
    }

    private void Start()
    {
        if (Mathf.Approximately(m_moveSpeed, 0))
            return;
        if (!myRigidbody.simulated)
            return;
        PlaySoundWalk();
    }

    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        Destroy(gameObject);
    }
    
    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        base.OnHit(attackerInfor);
    }
    
    public override void Dead()
    {
        base.Dead();
        StopAllCoroutines();

        myRigidbody.velocity = Vector2.zero;
        if (m_audioDead != null)
            SoundManager.PlaySound(m_audioDead, false);

        StopSoundWalk();
    }
    
    public override void OnResumeAfterHit()
    {

    }
    
    public override void PauseBehaviour()
    {
        myRigidbody.simulated = false;
        startBehaviour = false;
        spine.AnimationState.SetAnimation(0, "walk", true);
    }

    private void FixedUpdate()
    {
        if (isDead || !startBehaviour)
            return;
        CheckGrounded();
        if (!m_isGrounded)
            return;

        if (CheckWall() || CheckAbyss())
        {
            UpdateDirection(-m_direction);
        }

        Vector2 velocity = myRigidbody.velocity;
        velocity.x = m_direction * m_moveSpeed;
        myRigidbody.velocity = velocity;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;
        Vector2 offset = m_explodeCollider.offset * m_explodeRadius * transform.localScale;
        pos.x += offset.x;
        pos.y += offset.y;

        float radius = m_explodeRadius * m_explodeCollider.radius * Mathf.Abs(transform.localScale.x);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, radius);
    }

    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        switch (e.Data.Name)
        {
            case "explode":
                if (trackEntry.Animation.Name == deadAnimation)
                {
                    Explode();
                }
                break;

            case "end":
                if (trackEntry.Animation.Name == deadAnimation)
                {
                    ExplodeEnd();
                }
                break;
        }
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
        RaycastHit2D raycast = Physics2D.Raycast(m_collider.bounds.center, rightCheck ? Vector2.right : Vector2.left, m_bounds.extents.x + 0.2f, m_wallLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center, (rightCheck ? Vector2.right : Vector2.left) * (m_bounds.extents.x + 0.2f), Color.black);
#endif
        return raycast.collider != null;
    }
    
    void PlaySoundWalk()
    {
        if (m_soundWalk != null || isDead)
            return;
        m_soundWalk = SoundManager.PlaySound3D(m_audioWalk, 10, true, transform.position);
    }
    
    void StopSoundWalk()
    {
        if (m_soundWalk != null)
            Destroy(m_soundWalk);
    }
    
    void UpdateDirection(int dir)
    {
        if (!myRigidbody.simulated)
            return;
        m_direction = dir;
        transform.localScale = new Vector2(m_baseScale.x * m_direction, m_baseScale.y);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
    }

    private void Explode()
    {
        if (m_audioExplode != null)
            SoundManager.PlaySound(m_audioExplode, false);

        m_bodyDamageDealer.UpdateDamage(m_explodeDamage);
        m_explodeCollider.enabled = true;

        Vector3 baseScale = transform.localScale;

        DOTween.To(() => m_currentExplodeRadius, r =>
        {
            m_currentExplodeRadius = r;
            transform.localScale = baseScale * m_currentExplodeRadius;
        }, m_explodeRadius, m_explodeTime).OnComplete(() =>
        {
            m_boxHitter.enabled = true;
        });
    }

    private void ExplodeEnd()
    {
        m_explodeCollider.enabled = false;
    }
}
