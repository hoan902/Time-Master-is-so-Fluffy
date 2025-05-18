using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class STMonsterScout : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_bulletDamage = 10;
    [SerializeField] private float m_shotDuration = 3f;
    [SerializeField] private float m_bulletSpeed = 10f;
    [SerializeField] private float m_lerpTime = 0.3f;
    [SerializeField] private float m_attackRange = 10;
    [SerializeField] private Vector3 m_attackAreaOffset;

    [Header("Reference")]
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private STObjectDealDamage m_bulletDamageDealer;
    [SerializeField] private CircleCollider2D m_attackRangeCollider;
    [SerializeField] private GameObject m_bullet;
    [SerializeField] private AudioClip m_audioAttack;
    [SerializeField] private AudioClip m_audioDead;

    private float m_shotTimer;
    private bool m_canAttack;
    private Vector2 m_baseSpineScale;
    private Vector2 m_lookDirection;
    private bool m_playerInRange;

    private const string ANIM_ATTACK = "attack";

    public override void Awake() 
    {
        base.Awake();

        m_baseSpineScale = spine.transform.localScale;
        m_attackRangeCollider.radius = m_attackRange;
        m_attackRangeCollider.offset = m_attackAreaOffset;
        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        m_bulletDamageDealer.UpdateDamage(m_bulletDamage);

        spine.AnimationState.Event += OnAnimEvent;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        spine.AnimationState.Event -= OnAnimEvent;
    }
    public override void Attack()
    {
        base.Attack();
        BeginShoot();
    }
    public override void AttackComplete()
    {
        base.AttackComplete();
        spine.AnimationState.SetAnimation(0, "idle", true);
        m_canAttack = m_playerInRange;
    }
    public override void PlayerInRange(Collider2D other)
    {
        m_canAttack = true;
        m_playerInRange = true;
    }
    public override void PlayerOutRange(Collider2D other)
    {
        m_canAttack = false;
        m_playerInRange = false;
    }
    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        SoundManager.PlaySound(m_audioDead, false);
        Destroy(gameObject);
    }

    private void Update() 
    {
        if(isDead || !m_canAttack || !startBehaviour)
            return;
        LookAtPlayer();
        m_shotTimer += Time.deltaTime;
        if(m_shotTimer >= m_shotDuration)
        {
            m_shotTimer = 0;
            Attack();
        }
    }

    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if(trackEntry.Animation.Name == ANIM_ATTACK)
            Shoot();
    }

    void BeginShoot()
    {
        m_canAttack = false;
        TrackEntry trackEntry = spine.AnimationState.SetAnimation(0, ANIM_ATTACK, false);
        trackEntry.Complete += (trackEntry) => {
            AttackComplete();
        };
    }
    void Shoot()
    {
        float angleOfBullet = Vector2.SignedAngle(Vector2.left, m_lookDirection);
        GameObject bullet = Instantiate(m_bullet, transform.position + (Vector3)m_lookDirection, Quaternion.Euler(0, 0, angleOfBullet), transform.parent);
        bullet.SetActive(true);
        STEnemyBullet bulletComp = bullet.GetComponent<STEnemyBullet>();
        bulletComp.Init(m_lookDirection, m_bulletSpeed);
        SoundManager.PlaySound(m_audioAttack, false);
    }
    void LookAtPlayer()
    {
        Vector3 realTarget = player.transform.position + Vector3.up;
        m_lookDirection = (realTarget - transform.position).normalized;
        float angle = Vector3.SignedAngle(Vector3.left, m_lookDirection, Vector3.forward);

        spine.transform.rotation = Quaternion.Lerp(spine.transform.rotation, Quaternion.Euler(0, 0, angle), m_lerpTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_attackRangeCollider.bounds.center + m_attackAreaOffset, m_attackRange);

        m_attackRangeCollider.radius = m_attackRange;
    }
}
