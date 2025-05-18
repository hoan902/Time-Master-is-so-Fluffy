using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class STMonsterPlinky : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_bulletDamage = 10;
    [SerializeField] private float m_bulletSpeed = 10f;
    [SerializeField] private float m_shotDuration = 3f;
    [SerializeField] private int m_bulletPerShoot = 4;
    [SerializeField] private Vector2 m_firstShootDirection = Vector2.up;
    [SerializeField] private float m_attackRange = 10;
    [SerializeField] private Vector3 m_attackAreaOffset;

    [Header("Reference")]
    [SerializeField] private CircleCollider2D m_attackRangeCollider;
    [SerializeField] private GameObject m_bullet;
    [SerializeField] private float m_shotTimer; // serialize for testing
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private STObjectDealDamage m_bulletDamageDealer;
    [SerializeField] private AudioClip m_audioAttack;
    [SerializeField] private AudioClip m_audioDead;

    private bool m_canAttack;
    private bool m_playerInRange;

    private const string ANIM_ATTACK = "attack";
    private const string ANIM_ATTACK_1 = "attack_1";

    public override void Awake()
    {
        base.Awake();

        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        m_bulletDamageDealer.UpdateDamage(m_bulletDamage);

        spine.AnimationState.Event += OnAnimEvent;
        m_attackRangeCollider.radius = m_attackRange;
        m_attackRangeCollider.offset = m_attackAreaOffset;
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

    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if(trackEntry.Animation.Name == ANIM_ATTACK_1)
            Shoot();
    }

    private void Update() 
    {
        if(isDead || !m_canAttack || !startBehaviour)
            return;
        m_shotTimer += Time.deltaTime;
        if(m_shotTimer >= m_shotDuration)
        {
            m_shotTimer = 0;
            Attack();
        }
    }

    void BeginShoot()
    {
        m_canAttack = false;
        TrackEntry trackEntry = spine.AnimationState.SetAnimation(0, ANIM_ATTACK_1, false);
        trackEntry.Complete += (trackEntry) => {
            AttackComplete();
        };
    }
    void Shoot()
    {
        float anglePerShoot = 360 / m_bulletPerShoot;
        for(int i = 0; i < m_bulletPerShoot; i++)
        {
            Vector2 direction = (Quaternion.AngleAxis(anglePerShoot * i, Vector3.forward) * m_firstShootDirection).normalized;
            float angleOfBullet = Vector2.SignedAngle(Vector2.left, direction);
            GameObject bullet = Instantiate(m_bullet, transform.position, Quaternion.Euler(0, 0, angleOfBullet), transform.parent);
            bullet.SetActive(true);
            STEnemyBullet bulletComp = bullet.GetComponent<STEnemyBullet>();
            bulletComp.Init(direction, m_bulletSpeed);
            SoundManager.PlaySound(m_audioAttack, false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_attackRangeCollider.bounds.center + m_attackAreaOffset, m_attackRange);

        m_attackRangeCollider.radius = m_attackRange;
    }
}
