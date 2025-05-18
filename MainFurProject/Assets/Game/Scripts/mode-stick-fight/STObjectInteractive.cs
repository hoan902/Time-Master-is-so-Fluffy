using System;
using Spine;
using Spine.Unity;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class STObjectInteractive : MonoBehaviour
{
    private const float MAX_WIDTH_PROGRESS = 0.78125f;
    public enum State {Normal, Attacking}

    // Config
    public string deadKeyTrigger = "";
    public float maxHP = 100;
    public float knockbackStrength = 500;
    public bool hitStopAttack = false; // can be used later
    public bool canBeKilled = true;
    [HideInInspector]
    public bool canBeKnockback = true;

    // Reference
    [HideInInspector]
    public Rigidbody2D myRigidbody;
    [HideInInspector]
    public STPlayerStats player;
    [HideInInspector]
    public State bodyState;
    public bool onlyKnockbackUp = true;
    public Vector2 knockbackDirectionOffset;
    public Transform center;
    
    [Header("Optional")]
    public GameObject effectBlood;
    public GameObject effDead;
    public SpriteRenderer healthProgress;
    public AudioClip[] audiosHit;
    public SkeletonAnimation spine;
    public string hitAnimation;
    public string deadAnimation;
    
    protected float m_currentHP;

    // Constructor
    public bool isDead {get => m_currentHP <= 0;}
    public float currentHP {get => m_currentHP;}
    public DamageDealerInfo AttackerCache {get => m_attackerCache;}

    private SpriteRenderer m_healthBlack;
    private Tweener m_hpTweener;
    private Coroutine m_hpTimer;
    private Coroutine m_hurtTimer;
    private string m_lastAnim;
    private DamageDealerInfo m_attackerCache;

    public virtual void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<STPlayerStats>();
        m_currentHP = maxHP;
        if(healthProgress != null)
        {
            m_healthBlack = healthProgress.transform.parent.Find("hp-progress-white").GetComponent<SpriteRenderer>();
            UpdateHealthProgress();
        }
    }
    public virtual void OnDestroy()
    {
        if(m_hpTweener != null)
            m_hpTweener.Kill();
    }

    public virtual void OnResumeAfterHit()
    {
        if(spine != null)
            spine.AnimationState.SetAnimation(0, "idle", true);
    }

    public virtual void OnHit(DamageDealerInfo attackerInfor)
    {
        if(isDead)
            return;
        m_attackerCache = attackerInfor;
        if(audiosHit.Length > 0)
        {
            int rand = UnityEngine.Random.Range(0, audiosHit.Length);
            SoundManager.PlaySound3D(audiosHit[rand], 10, false, transform.position);
        }
            
        if(canBeKilled)
        {
            m_currentHP -= attackerInfor.damage;
            // hp
            if(healthProgress != null)
            {
                if(m_hpTimer != null)
                    StopCoroutine(m_hpTimer);
                m_hpTimer = StartCoroutine(IHideHp());
                UpdateHealthProgress();  
            }
        }
            
        // animation hit
        if(bodyState != State.Attacking && !isDead)
        {
            if(hitAnimation == "")
            {
                if(m_hurtTimer != null)
                    StopCoroutine(m_hurtTimer);
                m_hurtTimer = StartCoroutine(DelayHitComplete());
            }
            else
            {
                TrackEntry entry = spine.AnimationState.SetAnimation(0, hitAnimation, false);
                entry.Complete += (e) =>
                {
                    if(!isDead)
                        OnResumeAfterHit();
                };
            }   
        }

        //knockback
        if(myRigidbody != null && attackerInfor.attacker != transform)
        {
            bool attackerOnLeft = attackerInfor.attacker.position.x < center.position.x;
            int angle = attackerInfor.attacker.position.y > center.position.y ? 135 : 45;
            if (onlyKnockbackUp)
                angle = 45;
            Vector3 direction = Quaternion.AngleAxis((attackerOnLeft ? -1 : 1) * angle, Vector3.forward) * Vector3.up;
#if UNITY_EDITOR
            Debug.DrawRay(center.position, direction * 5f, Color.blue, 5f);
#endif
            if (myRigidbody.bodyType == RigidbodyType2D.Dynamic && knockbackStrength > 0 && canBeKnockback)
            {
                myRigidbody.AddForce(direction * knockbackStrength, ForceMode2D.Impulse);
            }   
        }
        //
        if(attackerInfor.damage > 0)
            InitEffectBlood();
    }

    public virtual void Dead()
    {
        if(deadKeyTrigger != "" && !(this is STObjectBoss))
            GameController.DoTrigger(deadKeyTrigger, true, this.gameObject);
        if(deadAnimation == "")
            return;
        TrackEntry deadTrack = spine.AnimationState.SetAnimation(0, deadAnimation, false);
        deadTrack.Complete += (deadTrack) => OnDeadFinish();
    }

    public virtual void OnDeadFinish()
    {
        InitEffectDead();
    }
    
    void UpdateHealthProgress()
    {
        Vector2 size = healthProgress.size;
        float oldVal = size.x;
        if(m_currentHP < 0)
            m_currentHP = 0;
        float newVal  = MAX_WIDTH_PROGRESS * m_currentHP / maxHP;
        healthProgress.size = new Vector2(newVal, size.y);
        if(isDead)
            Dead();
        if(m_hpTweener != null)
            m_hpTweener.Kill();
        m_hpTweener = DOTween.To(()=>oldVal, x => size.x = x, newVal, 0.2f).OnUpdate(()=>{
            m_healthBlack.size = size;
        }).OnComplete(()=>{
            if(isDead)
            {
                if(m_hpTimer != null)
                    StopCoroutine(m_hpTimer);
                healthProgress.transform.parent.gameObject.SetActive(false);
            }
        }).SetDelay(0.3f);

        // red blinking
        if(spine != null)
        {
            HurtFlashEffect eff = spine.GetComponent<HurtFlashEffect>();
            if (eff == null || m_currentHP == maxHP)
                return;
            eff.Fade(0);
        }
    }
    void InitEffectBlood()
    {
        if(effectBlood == null)
            return;
        GameObject go = Instantiate(effectBlood);
        go.transform.parent = transform.parent;
        go.transform.position = center.position;
        Destroy(go, 1f);
    }
    void InitEffectDead()
    {
        if(effDead == null)
            return;
        GameObject go = Instantiate(effDead);
        go.transform.SetParent(transform.parent, false);
        go.transform.position = center.position;
        go.transform.localScale = Vector3.one;
    }
    
    IEnumerator IHideHp()
    {
        healthProgress.transform.parent.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        healthProgress.transform.parent.gameObject.SetActive(false);
    }

    IEnumerator DelayHitComplete()
    {
        yield return new WaitForSeconds(1f);
        OnResumeAfterHit();
    }
}
