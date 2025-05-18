using System.Collections;
using UnityEngine;

public class STMonsterSoulCreep : STObjectMonster
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
    [SerializeField] private float m_shieldKnockbackStrenght = 50;

    [Header("Reference")]
    [SerializeField] private GameObject m_soulChild;
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private CircleCollider2D m_followArea;
    [SerializeField] private CircleCollider2D m_unFollowArea;
    [SerializeField] private GameObject m_fxHitWall;
    [SerializeField] private AudioClip m_audioFly;
    [SerializeField] private AudioClip m_audioDead;
    [SerializeField] private AudioClip m_audioHitShiled;

    private GameObject m_soundFly;
    private float m_flyTimer;
    private int m_direction;
    private bool m_playerInRange;
    private bool m_flying;
    private bool m_transform;
    private Vector2 m_baseScale;
    private Vector3 velocity;
    private bool m_shield;
    private bool m_hitting;
    private float m_baseKnockbackStrenght;

    private const string ANIM_IDLE = "idle";
    private const string ANIM_FLY = "fly";
    private const string ANIM_HIDE = "hide";

    public override void Awake()
    {
        base.Awake();

        m_baseScale = transform.localScale;
        m_followArea.radius = m_followRange;
        m_followArea.offset = m_followRangeOffset;
        m_unFollowArea.radius = m_followRange;
        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        m_baseKnockbackStrenght = knockbackStrength;
        m_transform = false;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override void PlayerInRange(Collider2D other)
    {
        if (other.offset.y < 0.5f)
            return;
        if (isDead || m_playerInRange)
            return;
        m_playerInRange = true;
        FollowPlayer();
    }
    public override void PlayerOutRange(Collider2D other)
    {
        if (other.offset.y < 0.5f)
            return;
        if (isDead || !m_playerInRange)
            return;
        m_playerInRange = false;
        StopAllCoroutines();
        Idle();
    }
    public override void Dead()
    {
        base.Dead();
        m_bodyDamageDealer.gameObject.SetActive(false);
        StopAllCoroutines();
        myRigidbody.gravityScale = 5f;
        SoundManager.PlaySound(m_audioDead, false);
        StopSoundFly();
        if (!m_transform)
            Destroy(gameObject);
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
        SoundManager.PlaySound(m_audioHitShiled, false);
        knockbackStrength = m_shield ? m_shieldKnockbackStrenght : m_baseKnockbackStrenght;
        if (m_shield)
        {
            fakeInfor.damage = 0;
            OnHitShield();
        }
        myRigidbody.velocity = Vector2.zero;
        base.OnHit(fakeInfor);
        StartCoroutine(IHit());
    }
    public override void OnResumeAfterHit()
    {

    }

    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
    }

    public override void StartBehaviour()
    {
        m_followArea.gameObject.SetActive(false);
        spine.gameObject.SetActive(false);
        m_soulChild.SetActive(true);
        base.StartBehaviour();
    }

    private void FixedUpdate()
    {
        if (isDead || !startBehaviour)
            return;
        if (!m_flying || m_hitting)
            return;
        m_flyTimer += Time.deltaTime;
        if (m_flyTimer >= m_flyTime)
        {
            StartCoroutine(IHide());
            return;
        }

        Vector3 direction = ((player.transform.position + Vector3.up) - transform.position).normalized;
        Vector2 velocity = new Vector2(direction.x * m_horizontalSpeed, direction.y * m_verticalSpeed);
        myRigidbody.velocity = velocity;

        bool needToFlip = (myRigidbody.position.x < (player.transform.position.x - 1)) || (myRigidbody.position.x > (player.transform.position.x + 1));
        if (needToFlip)
            UpdateDirection(direction);
    }

    void FollowPlayer()
    {
        m_flying = true;
        m_shield = false;
        spine.AnimationState.SetAnimation(0, ANIM_FLY, true);
        PlaySoundFly();
    }
    IEnumerator IHide()
    {
        m_flying = false;
        m_shield = true;
        StopSoundFly();
        m_flyTimer = 0;
        myRigidbody.velocity = Vector2.zero;
        spine.AnimationState.SetAnimation(0, ANIM_HIDE, true);
        yield return new WaitForSeconds(m_hideTime);
        if (m_playerInRange)
        {
            spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
            yield return new WaitForSeconds(0.5f);
            FollowPlayer();
        }
        else
            spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
    }
    void Idle()
    {
        m_flying = false;
        m_shield = false;
        StopSoundFly();
        m_flyTimer = 0;
        myRigidbody.velocity = Vector2.zero;
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
    }
    void OnHitShield()
    {
        GameObject go = Instantiate(m_fxHitWall, transform.parent, false);
        go.transform.position = transform.position;
        Destroy(go, 3f);
    }
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
    IEnumerator IHit()
    {
        m_hitting = true;
        yield return new WaitForSeconds(0.5f);
        m_hitting = false;
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

    // Message
    private void OnGrowUp()
    {
        m_transform = true;
        m_followArea.gameObject.SetActive(true);
        spine.gameObject.SetActive(true);
        m_soulChild.SetActive(false);
    }
}
