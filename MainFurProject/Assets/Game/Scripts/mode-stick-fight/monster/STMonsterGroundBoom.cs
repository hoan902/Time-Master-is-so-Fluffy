using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class STMonsterGroundBoom : STObjectMonster
{
    private const float S_RAYCAST_DISTANCE = 0.2f;
    private const string ANIM_IDLE = "idle";
    private const string ANIM_DANGER = "danger";

    [Header("Config")]
    [SerializeField] private float m_explosionTime = 5;
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_bombDamage = 40;

    [Header("Reference")]
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private Collider2D m_collider;
    [SerializeField] private GameObject m_effectDead;
    [SerializeField] private GameObject m_body;
    [SerializeField] private AudioClip m_audioTiktok;
    [SerializeField] private AudioClip m_audioTiktokFast;
    [SerializeField] private AudioClip m_audioExplode;
    public GameObject bomb;

    private int m_direction;
    private bool m_active;
    private GameObject m_soundTiktok;
    private GameObject m_soundTiktokFast;
    private GameObject m_soundExplode;
    private Coroutine m_activeRoutine;
    private BoxCollider2D m_visionCollider;
    private DamageDealerInfo m_damageDealerInfor;
    private bool m_hitted;

    public override void Awake()
    {
        base.Awake();

        m_damageDealerInfor = new DamageDealerInfo();
        m_damageDealerInfor.damage = (int)maxHP;
        m_damageDealerInfor.attacker = this.transform;
        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
    }
    IEnumerator Start() 
    {
        while (player == null)
        {
            yield return null;
        }
        Collider2D[] playerColliders = player.GetComponents<Collider2D>();
        foreach(Collider2D collider in playerColliders)
        {
            Physics2D.IgnoreCollision(m_collider, collider);
        }
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        StopAllCoroutines();
        if(m_soundTiktok != null)
            Destroy(m_soundTiktok);
        if(m_soundTiktokFast != null)
            Destroy(m_soundTiktokFast);
        if(m_soundExplode != null)
            Destroy(m_soundExplode);
    }
    public override void Attack()
    {
        base.Attack();
    }
    public override void PlayerInRange(Collider2D other)
    {
        
    }
    public override void PlayerOutRange(Collider2D other)
    {
        
    }
    public override void OnResumeAfterHit()
    {
        
    }
    public override void Dead()
    {
        base.Dead();
    }
    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        bomb.SetActive(true);
        bomb.transform.SetParent(transform.parent, true);
        bomb.GetComponent<STObjectBomb>().Init(m_bombDamage);
        if(m_soundExplode != null)
            Destroy(m_soundExplode);
        Destroy(gameObject);
    }
    public override void StartBehaviour()
    {
        base.StartBehaviour();
        m_body.SetActive(true);
    }
    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
        m_body.SetActive(false);
    }
    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        if(isDead)
            return;
        DamageDealerInfo fakeInfor = new DamageDealerInfo();
        fakeInfor.damage = attackerInfor.damage;
        fakeInfor.attacker = attackerInfor.attacker;
        if(attackerInfor.attacker != this.transform)
            fakeInfor.damage = 0;
        base.OnHit(fakeInfor);
        if(!m_hitted)
        {
            m_hitted = true;
            StartCoroutine(IActive());
        }
    }

    IEnumerator IActive()
    {
        if(m_soundTiktok == null)
            m_soundTiktok = SoundManager.PlaySound(m_audioTiktok, true);
        TrackEntry entry = spine.AnimationState.SetAnimation(0, ANIM_DANGER, true);
        yield return new WaitForSeconds(m_explosionTime);
        if(m_soundTiktok != null)
            Destroy(m_soundTiktok);   
        if(m_soundTiktokFast == null)
            m_soundTiktokFast = SoundManager.PlaySound(m_audioTiktokFast, true);
        if(m_soundTiktokFast != null)
            Destroy(m_soundTiktokFast);
        m_soundExplode = SoundManager.PlaySound(m_audioExplode, false);
        OnHit(m_damageDealerInfor);
    }

    void OnBounce(Vector2 velocity)
    {
        myRigidbody.velocity = velocity;
    }

    private void OnDrawGizmosSelected()
    {
        CircleCollider2D bombCollider = bomb.GetComponent<CircleCollider2D>();
        Gizmos.color = Color.red;    
        Gizmos.DrawWireSphere(bombCollider.bounds.center, bombCollider.radius);
    }
}
    
