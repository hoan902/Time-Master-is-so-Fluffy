using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STMonsterFlameSkull : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private float m_horizontalSpeed = 5;
    [SerializeField] private float m_verticalSpeed = 5;
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private float m_flyTime = 2f;
    [SerializeField] private float m_hideTime = 2f;
    [SerializeField] private float m_followRange = 10;
    [SerializeField] private float m_unfollowRange = 20;
    [SerializeField] private Vector3 m_followRangeOffset;

    [Header("Reference")]
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private CircleCollider2D m_followArea;
    [SerializeField] private CircleCollider2D m_unFollowArea;
    [SerializeField] private AudioClip m_audioFly;
    [SerializeField] private AudioClip m_audioDead;
    [SerializeField] private AudioClip m_audioAppear;
    [SerializeField] private AudioClip m_audioDisappear;

    private GameObject m_soundFly;
    private int m_direction;
    private bool m_playerInRange;
    private bool m_flying;
    private Vector2 m_baseScale;
    private bool m_hitting;
    private bool m_idle;

    private const string ANIM_IDLE = "idle_visible";
    private const string ANIM_FLY = "fly";
    private const string ANIM_APPEAR = "appear";
    private const string ANIM_DISAPPEAR = "disappear";


    // Setup
    public override void Awake()
    {
        base.Awake();

        m_baseScale = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
        m_followArea.radius = m_followRange;
        m_followArea.offset = m_followRangeOffset;
        m_unFollowArea.radius = m_followRange;
        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);

        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        spine.AnimationState.Complete -= OnAnimComplete;
        spine.AnimationState.Event -= OnAnimEvent;
    }

    private void Start()
    {
        Idle();
    }


    /// <summary>
    /// States:
    /// Invisible idle -> reveal -> fly -> hide -> invisible idle -> repeat
    /// </summary>
    // State transitions
    public override void Dead()
    {
        base.Dead();

        StopAllCoroutines();

        m_bodyDamageDealer.gameObject.SetActive(false);
        myRigidbody.gravityScale = 5f;

        if(m_audioDead != null)
            SoundManager.PlaySound(m_audioDead, false);
        StopSoundFly();
    }

    void FollowPlayer()
    {
        StartCoroutine(IScheduleHide());

        m_flying = true;

        spine.AnimationState.SetAnimation(0, ANIM_FLY, true);

        PlaySoundFly();
    }

    void Idle()
    {
        if(m_playerInRange)
            StartCoroutine(IScheduleAppear());

        m_idle = true;
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);

        StopSoundFly();
    }

    void Appear()
    {
        m_idle = false;
        spine.AnimationState.SetAnimation(0, ANIM_APPEAR, false);

        if (m_audioAppear != null)
            SoundManager.PlaySound(m_audioAppear, false);
    }

    void Disappear()
    {
        m_flying = false;
        myRigidbody.velocity = Vector2.zero;

        spine.AnimationState.SetAnimation(0, ANIM_DISAPPEAR, false);

        if (m_audioDisappear != null)
            SoundManager.PlaySound(m_audioDisappear, false);
    }

    IEnumerator IHit()
    {
        m_hitting = true;
        yield return new WaitForSeconds(0.5f);
        m_hitting = false;
    }

    IEnumerator IScheduleHide()
    {
        yield return new WaitForSeconds(m_flyTime);

        Disappear();
    }

    IEnumerator IScheduleAppear()
    {
        yield return new WaitForSeconds(m_hideTime);

        Appear();
    }


    // Animation transitions
    private void OnAnimComplete(TrackEntry trackEntry)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case ANIM_APPEAR:
                ToggleInteractable(true);
                FollowPlayer();
                break;

            case ANIM_DISAPPEAR:
                ToggleInteractable(false);
                Idle();
                break;
        }
    }

    private void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        //switch (e.Data.Name)
        //{
        //    case "ready":
        //        if (trackEntry.Animation.Name == ANIM_RISE)
        //        {
        //            SetInteractable(true);
        //        }
        //        if (trackEntry.Animation.Name == ANIM_SHRINK)
        //        {
        //            SetInteractable(false);
        //        }
        //        break;
        //}
    }


    // Events
    public override void PlayerInRange(Collider2D other)
    {
        if (other.offset.y < 0.5f)
            return;
        if (isDead || m_playerInRange)
            return;

        m_playerInRange = true;

        Appear();
    }

    public override void PlayerOutRange(Collider2D other)
    {
        if (other.offset.y < 0.5f)
            return;
        if (isDead || !m_playerInRange)
            return;

        m_playerInRange = false;
        StopAllCoroutines();

        if(!m_idle)
            Disappear();
    }

    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        Destroy(gameObject);
    }

    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        if (isDead)
            return;
        DamageDealerInfo fakeInfor = new DamageDealerInfo();
        fakeInfor.damage = attackerInfor.damage;
        fakeInfor.attacker = attackerInfor.attacker;
        myRigidbody.velocity = Vector2.zero;
        base.OnHit(fakeInfor);
        StartCoroutine(IHit());
    }

    public override void OnResumeAfterHit()
    {

    }


    // Update
    private void FixedUpdate()
    {
        if (isDead || !startBehaviour)
            return;
        if (!m_flying || m_hitting)
            return;

        Vector3 direction = ((player.transform.position + Vector3.up) - transform.position).normalized;
        Vector2 velocity = new Vector2(direction.x * m_horizontalSpeed, direction.y * m_verticalSpeed);
        myRigidbody.velocity = velocity;

        bool needToFlip = (myRigidbody.position.x < (player.transform.position.x - 1)) || (myRigidbody.position.x > (player.transform.position.x + 1));
        if (needToFlip)
            UpdateDirection(direction);
    }


    // Helper functions
    void PlaySoundFly()
    {
        if (m_soundFly != null || isDead)
            return;
        m_soundFly = SoundManager.PlaySound3D(m_audioFly, 10, true, transform.position);
    }

    void StopSoundFly()
    {
        if (m_soundFly != null)
            Destroy(m_soundFly);
    }

    void UpdateDirection(Vector3 moveDirection)
    {
        m_direction = moveDirection.x > 0 ? 1 : -1;
        transform.localScale = new Vector3(m_direction * m_baseScale.x, m_baseScale.y, 1);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
    }

    void ToggleInteractable(bool value)
    {
        m_bodyDamageDealer.gameObject.SetActive(value);
        GetComponent<Collider2D>().enabled = value;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_followArea.bounds.center + m_followRangeOffset, m_followRange);

        m_followArea.radius = m_followRange;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(m_unFollowArea.bounds.center, m_unfollowRange);

        m_unFollowArea.radius = m_unfollowRange;
    }
}
