using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectBoss : STObjectInteractive
{
    public GameObject m_coinPhysic;
    public float visibleRange = 10f;
    public string animIdle = "";
    public string animWin = "";
    public STObjectBoss[] sameBossesInLevel;

    public CircleCollider2D visibleCollider;
    
    public int coin = 5;

    public override void Awake()
    {
        base.Awake();

        GameController.readyEvent += OnReady;
        GameController.bossReadyEvent += OnReadyPlay;
        GameController.stopPlayerEvent += OnFinish;
        GameController.loadSavePointEvent += OnResetSavePoint;
        GameController.bossAppearEvent += OnAppear;

        if(m_coinPhysic.GetComponent<STObjectCoinPhysics>())
            m_coinPhysic.GetComponent<STObjectCoinPhysics>().UpdateCoin(coin);
        sameBossesInLevel = FindObjectsOfType<STObjectBoss>();
    }
    public virtual void Start()
    {
        visibleCollider.radius = visibleRange;
        Init();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();

        GameController.readyEvent -= OnReady;
        GameController.bossReadyEvent -= OnReadyPlay;
        GameController.stopPlayerEvent -= OnFinish;
        GameController.loadSavePointEvent -= OnResetSavePoint;
        GameController.bossAppearEvent -= OnAppear;
    }
    public override void OnResumeAfterHit()
    {
        base.OnResumeAfterHit();
    }
    public override void Dead()
    {
        base.Dead();
    }
    public override void OnDeadFinish()
    {
        base.OnDeadFinish();

        m_coinPhysic.transform.SetParent(transform.parent, true);
        m_coinPhysic.transform.eulerAngles = Vector3.zero;
        m_coinPhysic.SetActive(true);

        CoroutineHelper.NewCoroutine(IUpdatePoint());
    }
    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        base.OnHit(attackerInfor);
    }

    public virtual void OnAppear()
    {

    }
    public virtual void OnResetSavePoint(Vector2? position)
    {

    }
    public virtual void OnFinish(bool isFinish)
    {

    }
    public virtual void OnReadyPlay()
    {
        if (player == null)
            return;
        StartBoss();
    }
    public virtual void OnReady()
    {
        StartCoroutine(WaitBegin());
    }
    public virtual void Init()
    {

    }
    public virtual void StartBoss()
    {

    }

    IEnumerator WaitBegin()
    {
        yield return null;
        spine.AnimationState.SetAnimation(0, animIdle, true);
    }
    IEnumerator IUpdatePoint()
    {
        bool dropStar = true;
        if(sameBossesInLevel.Length > 1)
        {
            foreach(STObjectBoss boss in sameBossesInLevel)
            {
                if(boss == this)
                    continue;
                if(!boss.isDead)
                    dropStar = false;
            }
        }
        if(dropStar)
        {
            Vector3 pos = transform.position;
            GameController.UpdatePoint(1, pos, 1);
            yield return new WaitForSeconds(0.25f);
            GameController.UpdatePoint(1, pos, 1);
            yield return new WaitForSeconds(0.25f);
            GameController.UpdatePoint(1, pos, 1);

            if(deadKeyTrigger != "")
                GameController.DoTrigger(deadKeyTrigger, true);
        }        
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(visibleCollider.bounds.center, visibleRange);
    }
}
