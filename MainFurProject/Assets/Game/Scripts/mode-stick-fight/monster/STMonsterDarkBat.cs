using System.Collections;
using UnityEngine;
using Spine;

public class STMonsterDarkBat : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private Vector2 m_flySpeed = new Vector2(3, 3);
    [SerializeField] private Vector2 m_readyAttackSpeed = new Vector2(6, 6);
    [SerializeField] private float m_attackDuration = 5f;
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_needleDamage = 10;
    [SerializeField] private float m_followRangeLower = 7;
    [SerializeField] private float m_followRangeHigher = 10;
    [SerializeField] private float m_readyAttackRangeLower = 4;
    [SerializeField] private float m_readyAttackRangeHigher = 6;

    [Header("Reference")]
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private STObjectDealDamage m_needleDamageDealer;
    [SerializeField] private AudioClip m_audioFly;
    [SerializeField] private AudioClip m_audioAttack;
    [SerializeField] private AudioClip m_audioDead;

    private GameObject m_soundFly;
    private float m_attackTimer;
    private Vector2 m_baseScale;
    private bool m_playerInRange;
    private bool m_wakeUp;
    private bool m_moving;
    private int m_direction;
    private Vector3 m_destination;
    private Vector2 m_destinationOffset;
    private bool m_hitting;
    private bool m_readyAttack;
    private Vector2 m_currentSpeed;

    private const string ANIM_ATTACK = "attack";
    private const string ANIM_FLY = "fly";
    private const string ANIM_GET_UP = "get up";
    private const string ANIM_IDLE = "idle";
    
    public CircleCollider2D activeRangeCollider;
    public CircleCollider2D deactiveRangeCollider;

    public override void Awake()
    {
        base.Awake();
        m_baseScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        m_direction = transform.localScale.x > 0 ? 1 : -1;
        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        m_needleDamageDealer.UpdateDamage(m_needleDamage);
        m_needleDamageDealer.gameObject.SetActive(false);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
        m_currentSpeed = m_flySpeed;

        spine.AnimationState.Event += OnAnimEvent;
        spine.AnimationState.Complete += OnAnimComplete;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        StopSoundFly();
        spine.AnimationState.Event -= OnAnimEvent;
        spine.AnimationState.Complete -= OnAnimComplete;
    }
    public override void Attack()
    {
        base.Attack();
        m_attackTimer = 0;
        UpdateDirection(myRigidbody.position.x < player.transform.position.x ? 1 : -1);
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK, false);
    }
    public override void AttackComplete()
    {
        base.AttackComplete();
        m_attackTimer = 0;
        m_readyAttack = false;
        if(m_playerInRange)
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
        if (bodyState == State.Attacking)
            return;
        if (m_wakeUp)
            FollowPlayer();
        else
            WakeUp();
    }
    public override void PlayerOutRange(Collider2D other)
    {
        if (other.offset.y < 0.5f)
            return;
        if (isDead || !m_playerInRange)
            return;

        m_playerInRange = false;
        if(m_wakeUp && bodyState != State.Attacking)
            Idle();
    }
    public override void OnResumeAfterHit()
    {
        
    }

    public override void Dead()
    {
        base.Dead();
        m_bodyDamageDealer.gameObject.SetActive(false);
        StopAllCoroutines();
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
        StartCoroutine(IHit());
    }

    private void Start() 
    {
        activeRangeCollider.gameObject.SetActive(true);
        deactiveRangeCollider.gameObject.SetActive(true);
    }
    private void FixedUpdate() 
    {
        if(isDead || !startBehaviour || m_hitting)
            return;
        if (m_playerInRange)
        {
            bool needToFlip = (myRigidbody.position.x < (player.transform.position.x - 1)) || (myRigidbody.position.x > (player.transform.position.x + 1));
            if(needToFlip)
                UpdateDirection(myRigidbody.position.x < player.transform.position.x ? 1 : -1);
        }
        if(!m_moving)
            return;
        if(m_soundFly != null)
            m_soundFly.transform.position = transform.position;

        if (!m_readyAttack)
        {
            m_attackTimer += Time.deltaTime;
            if (m_attackTimer >= m_attackDuration)
            {
                m_readyAttack = true;
                FindNewFollowPosition(new Vector2(m_readyAttackRangeLower, m_readyAttackRangeHigher));
                m_currentSpeed = m_readyAttackSpeed;
            }
        }
        
        if(bodyState != State.Attacking)
            m_destination = (Vector2)player.transform.position + m_destinationOffset;
        
#if UNITY_EDITOR
        Debug.DrawLine(transform.position, m_destination, m_readyAttack ? Color.red : Color.yellow);
#endif
        
        Vector2 direction = (m_destination - transform.position).normalized;
        myRigidbody.velocity = direction * m_currentSpeed;
        
        if (Vector3.Distance(myRigidbody.position, m_destination) <= 0.2f)
        {
            if (m_readyAttack)
            {
                if (bodyState != State.Attacking)
                {
                    FindNewAttackPosition();
                    m_destination = (Vector2)player.transform.position + m_destinationOffset;
                    m_currentSpeed = m_readyAttackSpeed;
                    Attack();
                }
                else
                {
                    myRigidbody.velocity = Vector2.zero;
                }
            }
            else
            {
                FindNewFollowPosition(new Vector2(m_followRangeLower, m_followRangeHigher));
            }
        }
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        if(isDead)
            return;
        switch(trackEntry.Animation.Name)
        {
            case ANIM_ATTACK:
                m_needleDamageDealer.gameObject.SetActive(false);
                AttackComplete();
                break;
            case ANIM_GET_UP:
                m_wakeUp = true;
                PlaySoundFly();
                if(m_playerInRange)
                    FollowPlayer();
                else
                    Idle();
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
                m_needleDamageDealer.gameObject.SetActive(true);
                SoundManager.PlaySound(m_audioAttack, false);
                break;
        }
    }

    void WakeUp()
    {
        spine.AnimationState.SetAnimation(0, ANIM_GET_UP, false);
    }
    void FollowPlayer()
    {
        FindNewFollowPosition(new Vector2(m_followRangeLower, m_followRangeHigher));
        m_currentSpeed = m_flySpeed;
        m_moving = true;
        spine.AnimationState.SetAnimation(0, ANIM_FLY, true);
    }
    void Idle()
    {
        m_moving = false;
        myRigidbody.velocity = Vector2.zero;
        spine.AnimationState.SetAnimation(0, ANIM_FLY, true);
    }

    void FindNewAttackPosition()
    {
        bool left = transform.position.x < player.transform.position.x;
        int temp = left ? -1 : 1;
        m_destinationOffset = new Vector3(temp * 0.5f, 4f, 0);
    }

    void FindNewFollowPosition(Vector2 range)
    {
        do
        {
            m_destinationOffset = Random.insideUnitCircle.normalized * range.y;
        } while (m_destinationOffset.y < range.x);
        
    }
    void UpdateDirection(int direction)
    {
        m_direction = direction;
        transform.localScale = new Vector3(m_baseScale.x * direction, m_baseScale.y, 1);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
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

    IEnumerator IHit()
    {
        m_hitting = true;
        yield return new WaitForSeconds(0.5f);
        m_hitting = false;
    }
    
    // Editor
    public void UpdateActiveAreaSize(float radius)
    {
        activeRangeCollider.radius = radius;
    }

    public void UpdateActiveAreaOffset(Vector2 offset)
    {
        activeRangeCollider.offset = offset;
    }

    public void UpdateDeactiveSize(float radius)
    {
        deactiveRangeCollider.radius = radius;
    }
}
