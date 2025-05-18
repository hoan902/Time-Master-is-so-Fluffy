using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    private const float MAX_WIDTH_PROGRESS = 0.78125f;

    public int health = 1;
    public int coin = 10;
    // [HideInInspector]
    public Transform center;
    // [HideInInspector]
    public SpriteRenderer healthProgress;
    // [HideInInspector]
    public SkeletonAnimation spine;
    // [HideInInspector]
    public CoinEffect coinEffect;
    // [HideInInspector]
    public GameObject m_effectBlood;
    // [HideInInspector]
    public AudioClip m_audioHit;//weapon_impact_success
    // [HideInInspector]
    [SerializeField] protected LayerMask m_wallLayer;

    [HideInInspector] public bool lockHurt;
    [HideInInspector] public bool ignoreHit;
    private SpriteRenderer m_healthBlack;
    protected int m_currentHp;
    private Tweener m_hpTweener;
    private Coroutine m_hpTimer;
    private Coroutine m_hurtTimer;
    private string m_lastAnim;
    protected bool m_hurting;
    private Renderer m_renderer;
    

    public int currentHp
    {
        get
        {
            return m_currentHp;
        }
    }

    public bool isDead{
        get{
            return m_currentHp <= 0;
        }
    }

    public virtual void Awake()
    {
        m_currentHp = health;
        if(healthProgress != null)
        {
            m_healthBlack = healthProgress.transform.parent.Find("hp-progress-white").GetComponent<SpriteRenderer>();
            UpdateHealthProgress();
        }
        //
        m_renderer = spine.GetComponent<Renderer>();
    }

    public virtual void OnDestroy()
    {
        if(m_hpTweener != null)
            m_hpTweener.Kill();
    }

    public virtual void FixedUpdate()
    {
        center.localScale = new Vector3(Mathf.Sign(transform.localScale.x), 1, 1);
        center.eulerAngles = Vector3.zero;
        //
        if (isDead || m_wallLayer.value == 0 || Mathf.Approximately(m_renderer.bounds.extents.x, 0))
            return;
        RaycastHit2D hit = Physics2D.Raycast(m_renderer.bounds.center, Vector2.right, m_renderer.bounds.extents.x + 0.1f, m_wallLayer);
        if(hit.collider != null)
        {
            hit = Physics2D.Raycast(m_renderer.bounds.center, Vector2.left, m_renderer.bounds.extents.x + 0.1f, m_wallLayer);
            if (hit.collider != null)
                SendMessage("OnHit", hit.collider);
        }
    }

    public virtual void Hurt(int hpToReduce = 1)
    {
        if(isDead)
            return;
        SoundManager.PlaySound3D(m_audioHit, 5, false, transform.position);
        m_currentHp -= hpToReduce;
        if(isDead)
        {
            GameController.MonsterDead(this.gameObject);
        }
        //hurt anim
        if(!ignoreHit)
        {
            if(m_hurtTimer != null)
                StopCoroutine(m_hurtTimer);
            else
                m_lastAnim = spine.AnimationName;
            m_hurtTimer = StartCoroutine(IHurt());
        }
        //hp
        if(healthProgress != null)
        {
            if(m_hpTimer != null)
                StopCoroutine(m_hpTimer);
            m_hpTimer = StartCoroutine(IHideHp());
            UpdateHealthProgress();  
        }      
        InitEffectBlood();        
    }

    void UpdateHealthProgress()
    {
        Vector2 size = healthProgress.size;
        float oldVal = size.x;
        float newVal  = MAX_WIDTH_PROGRESS*m_currentHp/health;
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
        HurtFlashEffect eff = spine.GetComponent<HurtFlashEffect>();
        if(eff == null)
            return;
        eff.Flash();
    }

    public virtual void Dead()
    {

    }

    public virtual void DOSlowSpeed()
    {

    }

    public virtual void DoResumeSpeed()
    {

    }

    public virtual bool Transform()
    {
        return false;
    }

    public virtual void StartBehaviour()
    {

    }

    void InitEffectBlood()
    {
        GameObject go = Instantiate(m_effectBlood);
        go.transform.parent = transform.parent;
        go.transform.position = center.position;
        Destroy(go, 1f);
    }

    IEnumerator IHideHp()
    {
        healthProgress.transform.parent.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        healthProgress.transform.parent.gameObject.SetActive(false);
    }

    IEnumerator IHurt()
    {
        if(lockHurt)
            yield break;
        if(!m_hurting)
            DOSlowSpeed();
        m_hurting = true;
        TrackEntry entry  = spine.AnimationState.SetAnimation(0, "hit", false);
        yield return new WaitForSeconds(entry.AnimationEnd);
        if(isDead)
            yield break;
        if(m_hurting)
            DoResumeSpeed();
        m_hurting = false;
        if(lockHurt)   
            yield break;  
        spine.AnimationState.SetAnimation(0, m_lastAnim, true);
    }
}
