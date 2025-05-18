using Spine;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class STMonsterFlameDragon : STObjectMonster
{
    // config 
    [SerializeField] private Vector2 m_flySpeed = new Vector2(3, 3);
    [SerializeField] private float m_activeRange = 10f;
    [SerializeField] private float m_deactiveRange = 15f;
    [SerializeField] private float m_innerFollowRange = 2;
    [SerializeField] private float m_outerFollowRange = 5;
    [SerializeField] private float m_heightFromPlayer = 3;
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_bulletDamage = 10;
    [SerializeField] private float m_shootDuration = 3f;
    [SerializeField] private float m_bulletSpeed = 10;
    [SerializeField] private float m_changeFollowPositionDuration = 1f;

    //reference
    [SerializeField] private GameObject m_bullet;
    [SerializeField] private Transform m_shotPoint;
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private STObjectDealDamage m_bulletDamageDealer;
    [SerializeField] private AudioClip m_audioFly;
    [SerializeField] private AudioClip m_audioShoot;
    [SerializeField] private AudioClip m_audioDead;

    private GameObject m_soundFly;
    private float m_attackTimer;
    private float m_followTimer;
    private bool m_playerInRange;
    private Vector2 m_baseScale;
    private int m_direction;
    private bool m_moving;
    private Vector3 m_destination;
    private Vector2 m_destinationOffset;
    private Vector3 m_velocity;
    private float slerpTime;
    private Coroutine m_moveRoutine;

    private const string ANIM_IDLE = "idle";
    private const string ANIM_ATTACK = "attack";
    
    public CircleCollider2D activeRangeCollider;
    public CircleCollider2D deactiveRangeCollider;

    public override void Awake()
    {
        base.Awake();
        m_baseScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        m_direction = transform.localScale.x > 0 ? 1 : -1;
        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        m_bulletDamageDealer.UpdateDamage(m_bulletDamage);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        StopSoundFly();
        
        spine.AnimationState.Event -= OnAnimEvent;
        spine.AnimationState.Complete -= OnAnimComplete;
    }
    public override void AttackComplete()
    {
        if(isDead)
            return;
        base.AttackComplete();
        m_attackTimer = 0;
        if (m_playerInRange)
            FollowPlayer();
        else
            Idle();
    }
    public override void PlayerInRange(Collider2D other)
    {
        if (other.offset.y < 0.5f)
            return;
        if (isDead || m_playerInRange)
            return;
        m_playerInRange = true;
        if(bodyState == State.Attacking)
            return;
        FollowPlayer();
    }
    public override void PlayerOutRange(Collider2D other)
    {
        if (other.offset.y < 0.5f)
            return;
        if (isDead || !m_playerInRange)
            return;
        m_playerInRange = false;
        if(bodyState == State.Attacking)
            return;
        Idle();
    }
    public override void OnResumeAfterHit()
    {
        
    }
    public override void Dead()
    {
        StopAllCoroutines();
        base.Dead();
        m_bodyDamageDealer.gameObject.SetActive(false);
        myRigidbody.gravityScale = 5;
        SoundManager.PlaySound(m_audioDead, false);
    }
    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        Destroy(gameObject);
    }
    public override void StartBehaviour()
    {
        base.StartBehaviour();
        activeRangeCollider.gameObject.SetActive(true);
        deactiveRangeCollider.gameObject.SetActive(true);
    }
    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
        activeRangeCollider.gameObject.SetActive(false);
        deactiveRangeCollider.gameObject.SetActive(false);
    }
    public override void OnHit(DamageDealerInfo damageDealerInfo)
    {
        myRigidbody.velocity = Vector2.zero;
        base.OnHit(damageDealerInfo);
    }

    private void Start()
    {
        activeRangeCollider.gameObject.SetActive(true);
        deactiveRangeCollider.gameObject.SetActive(true);
        activeRangeCollider.radius = m_activeRange;
        deactiveRangeCollider.radius = m_deactiveRange;
        PlaySoundFly();
        Idle();
        
        spine.AnimationState.Event += OnAnimEvent;
        spine.AnimationState.Complete += OnAnimComplete;
    }
    private void FixedUpdate()
    {
        if(isDead || !startBehaviour)
            return;
        if (m_playerInRange)
        {
            bool needToFlip = (myRigidbody.position.x < (player.transform.position.x - 1)) || (myRigidbody.position.x > (player.transform.position.x + 1));
            if (needToFlip)
                UpdateDirection(myRigidbody.position.x < player.transform.position.x ? 1 : -1);
            if (bodyState != State.Attacking)
            {
                // timer attack
                m_attackTimer += Time.deltaTime;
                if (m_attackTimer >= m_shootDuration)
                    BeginShoot();
                //timer update follow position
                m_followTimer += Time.deltaTime;
                if(m_followTimer >= m_changeFollowPositionDuration)
                {
                    FindNewFollowPosition(true);
                    if(m_moveRoutine != null)
                        StopCoroutine(m_moveRoutine);
                    m_moveRoutine = StartCoroutine(IFly());
                }
                    
            }
        }
//        if(!m_moving)
//            return;
//        if(m_soundFly != null)
//            m_soundFly.transform.position = transform.position;
        
//        m_destination = player.transform.position + (Vector3)m_destinationOffset;
//        m_destination = Vector3.Slerp(transform.position, player.transform.position + (Vector3)m_destinationOffset,  * Time.deltaTime);
//#if UNITY_EDITOR
//        Debug.DrawLine(transform.position, m_destination, Color.yellow);
//#endif
//        if (Vector3.Distance(myRigidbody.position, m_destination) <= 0.2f)
//        {
//            myRigidbody.velocity = Vector2.zero;
//            return;
//        }
//        Vector2 direction = (m_destination - transform.position).normalized;
//        myRigidbody.velocity = direction * m_flySpeed;
    }

    IEnumerator IFly()
    {
        Vector3 startPos = transform.position;
        m_destination = player.transform.position + (Vector3)m_destinationOffset;
        float distance = Vector3.Distance(startPos, m_destination);
        float timeToCompleteDistance = distance / m_flySpeed.x;
        Vector3 centerPivot = (startPos + m_destination) * 0.5f;
        centerPivot -= new Vector3(0, 2);
        Vector3 startRelativeCenter = startPos - centerPivot;
        Vector3 endRelativeCenter = m_destination - centerPivot;

        Vector2 tempPos;

#if UNITY_EDITOR
        Debug.DrawLine(transform.position, m_destination, Color.yellow, 3f);
#endif
        slerpTime = 0f; ;
        while (slerpTime < 1)
        {
            yield return null;
            slerpTime += Time.deltaTime / timeToCompleteDistance;
            tempPos = Vector3.Slerp(startRelativeCenter, endRelativeCenter, slerpTime) + centerPivot;
            Vector2 direction = (tempPos - myRigidbody.position).normalized;
            myRigidbody.velocity = direction * m_flySpeed;
        }
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        if(isDead)
            return;
        switch(trackEntry.Animation.Name)
        {
            case ANIM_ATTACK:
                AttackComplete();
                break;
        }
    }
    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if(isDead)
            return;
        switch(trackEntry.Animation.Name)
        {
            case ANIM_ATTACK:
                Shoot();
                break;
        }
    }

    void BeginShoot()
    {
        m_moving = false;
        myRigidbody.velocity = Vector2.zero;
        if(m_moveRoutine != null)
            StopCoroutine(m_moveRoutine);
        bodyState = State.Attacking;
        m_attackTimer = 0;
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK, false);
    }
    void Shoot()
    {
        Vector2 shootDirection = (player.transform.position - m_shotPoint.position).normalized;
        GameObject bullet = Instantiate(m_bullet, m_shotPoint.position, Quaternion.identity, transform.parent);
        bullet.SetActive(true);
        STEnemyBullet bulletComp = bullet.GetComponent<STEnemyBullet>();
        bulletComp.Init(shootDirection, m_bulletSpeed);
        SoundManager.PlaySound(m_audioShoot, false);
    }
    void UpdateDirection(int direction)
    {
        m_direction = direction;
        transform.localScale = new Vector3(m_baseScale.x * direction, m_baseScale.y, 1);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
    }
    void Idle()
    {
        m_moving = false;
        myRigidbody.velocity = Vector2.zero;
        if (m_moveRoutine != null)
            StopCoroutine(m_moveRoutine);
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
    }
    void FollowPlayer()
    {
        FindNewFollowPosition();
        m_moving = true;
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
    }
    void FindNewFollowPosition(bool resetFollowTimer = false)
    {
        // tempLerp = 0;
        if(resetFollowTimer)
            m_followTimer = 0;
        do
        {
            m_destinationOffset = Random.insideUnitCircle.normalized * m_outerFollowRange;
        } while (m_destinationOffset.y < m_heightFromPlayer);
    }
    void PlaySoundFly()
    {
        if(m_soundFly != null || isDead)
            return;
        m_soundFly = SoundManager.PlaySound3D(m_audioFly, 10, true, transform.position);
    }
    void StopSoundFly()
    {
        if(m_soundFly != null)
            Destroy(m_soundFly);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(activeRangeCollider.bounds.center, m_activeRange);
    
        activeRangeCollider.radius = m_activeRange;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(deactiveRangeCollider.bounds.center, m_deactiveRange);

        deactiveRangeCollider.radius = m_deactiveRange;
    }
}
