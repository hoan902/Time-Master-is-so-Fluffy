using DG.Tweening;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STBossGoldhorn : STObjectBoss
{
    private enum BodyState
    {
        Idle,
        Attack,
        Move,
        Jump
    }
    private const float M_RANGE_ATTACK = 3f;
    private const float M_EFFECT_DISTANCE = 1.5f;

    private const string ANIM_APPEAR = "xuat_hien";
    private const string ANIM_IDLE = "idle";
    private const string ANIM_ATTACK1 = "skill1";
    private const string ANIM_ATTACK2 = "skill2";
    private const string ANIM_ATTACK3_READY = "skill3_1_ready_at";
    private const string ANIM_ATTACK3_JUMP_UP = "skill3_2_up";
    private const string ANIM_ATTACK3_JUMP_DOWN = "skill3_3_down";
    private const string ANIM_ATTACK3_END = "skill3_4_at_end";
    private const string ANIM_ATTACK4_READY = "skill4";
    private const string ANIM_ATTACK4_RUN = "skill4_2";

    private const string ANIM_HIT = "hit";
    private const string ANIM_STUN = "stun";
    private const string ANIM_DEAD = "dead";
    private const string ANIM_RUN = "run";

    private const string EVENT_ATTACK = "attack";
    private const string EVENT_STEP = "step";
    private const string EVENT_SHAKE = "shake";
    private const string EVENT_LAND = "land";

    [Range(0f, 1f)][SerializeField] private float m_nextPhaseRatio = 1f;
    [SerializeField] private float m_runSpeed = 8f;
    [SerializeField] private float m_goresSpeed = 10f;
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_trailDamage = 10;
    [SerializeField] private float m_timeJump = 6f;
    [SerializeField] private float m_rangeJump = 3f;
    [SerializeField] private float m_jumpPower = 10f;

    [SerializeField] private GameObject m_virtualCamera;
    [SerializeField] private GameObject m_effectTrail;
    [SerializeField] private Collider2D m_attackArea;
    [SerializeField] private ContactFilter2D m_targetContactFilter;
    [SerializeField] private LayerMask m_layerObstacle;

    [Header("Sounds")]
    [SerializeField] private AudioClip m_audioAppear;
    [SerializeField] private AudioClip m_audioFootsteps;
    [SerializeField] private AudioClip m_audioGeneralImpact;
    [SerializeField] private AudioClip m_audioSkillAttackVoice1;
    [SerializeField] private AudioClip m_audioSkillAttackVoice2;
    [SerializeField] private AudioClip m_audioSkillChargeImpact;
    [SerializeField] private AudioClip m_audioSkillChargeVoice;
    [SerializeField] private AudioClip m_audioSkillSlamImpact;
    [SerializeField] private AudioClip m_audioSkillSlamVoice;
    [SerializeField] private AudioClip m_hitAudio;
    [SerializeField] private AudioClip m_audioDie;

    private bool isNextPhase { get => (m_currentHP / maxHP) <= m_nextPhaseRatio; }

    private Action onCompleted = null;

    private BodyState m_bodyState;
    private Vector2 m_baseScale;
    private Vector2 m_spineScale;
    private int m_direction;
    private int m_attackIndex;
    private float m_baseSpeed;
    private float m_lastY;
    private bool m_start;
    private Vector2? m_destination;
    private Vector2 m_moveDirection;
    private Vector2 m_dirRight;
    private Vector2 m_dirLeft;
    private Coroutine m_pushRotinue;
    private GameObject m_body;
    private Collider2D m_collider;

    public override void Awake()
    {
        base.Awake();

        m_baseSpeed = m_runSpeed;
        m_body = transform.Find("body").gameObject;
        m_body.GetComponent<STObjectDealDamage>().UpdateDamage(m_bodyDamage);
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        spine.AnimationState.Start -= OnAnimStart;
        spine.AnimationState.Complete -= OnAnimComplete;
        spine.AnimationState.Event -= OnAnimEvent;
    }

    public override void OnResumeAfterHit()
    {

    }

    public override void Dead()
    {
        base.Dead();
        SoundManager.PlaySound3D(m_audioDie, 20f, false, transform.position);
        StopMove();
        ColliderBody(false);
        StopAllCoroutines();
    }

    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        Destroy(gameObject);
    }

    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        base.OnHit(attackerInfor);
        GameController.UpdateBossHp((int)maxHP, (int)currentHP);
        SoundManager.PlaySound3D(m_hitAudio, 10f, false, transform.position);
        if (isDead)
            return;
        TrackEntry entry = spine.AnimationState.SetAnimation(1, ANIM_HIT, false);
        entry.Complete += (e) =>
        {
            if (!isDead)
                OnResumeAfterHit();
        };
    }

    public override void OnAppear()
    {
        StartCoroutine(IVisible());
    }

    public override void OnReadyPlay()
    {
        base.OnReadyPlay();
    }

    public override void OnReady()
    {

    }

    public override void Init()
    {
        m_baseScale = transform.localScale;
        m_spineScale = spine.transform.localScale;
        m_direction = m_baseScale.x > 0 ? -1 : 1;
        m_lastY = transform.position.y;
        m_dirLeft = FindWallMove(-1);
        m_dirRight = FindWallMove(1);

        spine.AnimationState.Start += OnAnimStart;
        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;
        spine.gameObject.SetActive(false);
    }

    public override void StartBoss()
    {
        m_start = true;
        FirstSkill();
    }

    private void Update()
    {
        if (isDead || !m_start)
            return;
        if (m_destination == null)
            return;
        myRigidbody.velocity = m_moveDirection * m_baseSpeed;
        if (Vector2.Distance((Vector2)transform.position, m_destination.Value) <= 0.3f)
        {
            onCompleted?.Invoke();
            StopMove();
        }
    }

    // ------------------------------ timer ---------------------------- //
    private IEnumerator IVisible()
    {
        m_virtualCamera.SetActive(true);
        spine.transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(1f);
        spine.gameObject.SetActive(true);
        spine.AnimationState.SetAnimation(0, ANIM_APPEAR, false);
        SoundManager.PlaySound3D(m_audioAppear, 50f, false, transform.position);
        yield return null;
        yield return null;
        yield return null;
        spine.transform.localScale = m_spineScale;
    }

    private IEnumerator IDelayStart()
    {
        m_bodyState = BodyState.Idle;
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        m_virtualCamera.SetActive(false);
        yield return new WaitForSeconds(1f);
        GameController.BossReady();
    }

    private IEnumerator IScheduleFirstSkill()
    {
        TrackEntry trackEntry = spine.AnimationState.SetAnimation(0, ANIM_IDLE, false);
        yield return new WaitForSeconds(trackEntry.AnimationEnd);
        if (isDead)
            yield break;
        FirstSkill();
    }

    private IEnumerator IScheduleSecondSkill()
    {
        TrackEntry trackEntry = spine.AnimationState.SetAnimation(0, ANIM_IDLE, false);
        yield return new WaitForSeconds(trackEntry.AnimationEnd);
        if (isDead)
            yield break;
        LookAtPlayer();
        SecondSkill();
    }

    private IEnumerator IScheduleFourSkill()
    {
        LookAtPlayer();
        SoundManager.PlaySound3D(m_audioSkillChargeVoice, 10f, false, transform.position);
        TrackEntry trackEntry = spine.AnimationState.SetAnimation(0, ANIM_ATTACK4_READY, false);
        yield return new WaitForSeconds(trackEntry.AnimationEnd);
        if (isDead)
            yield break;
        FourSkill();
    }

    private IEnumerator IWaitJump()
    {
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK3_JUMP_UP, false);
        float lastY = transform.position.y;
        yield return null;
        //
        while (transform.position.y - lastY > 0f)
        {
            lastY = transform.position.y;
            yield return null;
        }
        if (isDead)
            yield break;
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK3_JUMP_DOWN, false);
        ColliderBody(false);
    }

    private IEnumerator IPushBody()
    {
        int frame = 5;
        while (frame > 0)
        {
            yield return new WaitForEndOfFrame();
            frame--;
            myRigidbody.velocity = m_direction * knockbackDirectionOffset;
        }
        HitPlayer();
        StopMove();
    }

    // ------------------------------ skill ---------------------------- //
    private void FirstSkill()
    {
        LookAtPlayer();
        //
        m_baseSpeed = m_runSpeed;
        Vector2 targetPos = new Vector2(player.transform.position.x, m_lastY) + new Vector2(-m_direction * M_RANGE_ATTACK, 0);
        MoveTo(targetPos, false, () =>
        {
            if (isDead)
                return;
            spine.AnimationState.SetAnimation(0, ANIM_ATTACK1, false);
        });
    }

    private void SecondSkill()
    {
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK2, false);
    }

    private void ReadyJump()
    {
        SoundManager.PlaySound3D(m_audioSkillSlamVoice, 10f, false, transform.position);
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK3_READY, false);
    }

    private void ThirdSkill()
    {
        LookAtPlayer();
        float rangeWall = Vector2.Distance(transform.position, FindWallMove());
        float rangeTarget = Vector2.Distance(transform.position, transform.position + new Vector3(m_rangeJump * m_direction, 0));
        Vector3 targetPos = transform.position + new Vector3(Mathf.Min(rangeWall, rangeTarget) * m_direction, 0);
        transform.DOJump(targetPos, m_jumpPower, 1, m_timeJump).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (isDead)
                return;
            ColliderBody(true);
            spine.AnimationState.SetAnimation(0, ANIM_ATTACK3_END, false);
        });
        StartCoroutine(IWaitJump());
    }

    private void FourSkill()
    {
        Vector2 targetPos = m_direction < 0 ? m_dirLeft : m_dirRight;
        m_baseSpeed = m_goresSpeed;
        MoveTo(targetPos, true, () =>
        {
            SoundManager.PlaySound3D(m_audioSkillChargeImpact, 30f, false, transform.position);
            GameController.VibrateCustom(new Vector3(0.3f, 0.3f), 0.5f);
            StartCoroutine(IScheduleFirstSkill());
        });
        if (isDead)
            return;
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK4_RUN, true);
    }

    private void ColliderBody(bool active)
    {
        m_body.SetActive(active);
        m_collider.enabled = active;
    }

    private void MoveTo(Vector2? des, bool ignore, Action onComplete = null)
    {
        m_destination = des;
        onCompleted = onComplete;
        if (m_destination != null)
        {
            m_moveDirection = (m_destination.Value - (Vector2)transform.position).normalized;
            if (ignore)
                return;
            LookAtPlayer();
            if (m_bodyState == BodyState.Move)
                return;
            spine.AnimationState.SetAnimation(0, ANIM_RUN, true);
        }
    }

    private void StopMove()
    {
        MoveTo(null, false);
        myRigidbody.velocity = Vector2.zero;
    }

    private Vector2 FindWallMove()
    {
        Vector2 des = new Vector2(transform.position.x, m_lastY);
        Vector2 origin = (Vector2)transform.position + new Vector2(0, 1);
        RaycastHit2D hit2D = Physics2D.Raycast(origin, Vector2.right * m_direction, 100f, m_layerObstacle);
        if (hit2D.collider == null)
            des = (Vector2)transform.position + new Vector2(20f * m_direction, 0);
        else
        {
            des = hit2D.point;
            des.x -= m_direction * 2f;
            des.y = m_lastY;
        }
        return des;
    }

    private Vector2 FindWallMove(int direction)
    {
        Vector2 des = new Vector2(transform.position.x, m_lastY);
        Vector2 origin = (Vector2)transform.position + new Vector2(0, 1);
        RaycastHit2D hit2D = Physics2D.Raycast(origin, Vector2.right * direction, 100f, m_layerObstacle);
        if (hit2D.collider == null)
            des = (Vector2)transform.position + new Vector2(20f * direction, 0);
        else
        {
            des = hit2D.point;
            des.x -= direction * 2f;
            des.y = m_lastY;
        }
        return des;
    }

    private void LookAtPlayer()
    {
        m_direction = transform.position.x > player.transform.position.x ? -1 : 1;
        transform.localScale = new Vector3(m_baseScale.x * -m_direction, m_baseScale.y, 1);
    }

    private void SpawnTrail(DamageDealerInfo data)
    {
        float quantity = Vector2.Distance(transform.position + new Vector3(-m_direction, 0), FindWallMove()) / M_EFFECT_DISTANCE;
        GameObject trail = Instantiate(m_effectTrail, transform.position + new Vector3(m_direction, 0), Quaternion.identity, transform.parent);
        trail.SetActive(true);
        trail.GetComponent<STObjectDealDamageTrail>().Init(m_targetContactFilter, data, (int)quantity, m_direction);
    }

    // ------------------------------ public method ---------------------------- //
    public void HitPlayer()
    {
        List<Collider2D> results = new List<Collider2D>();
        m_attackArea.OverlapCollider(m_targetContactFilter, results);
        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].tag == GameTag.PLAYER)
            {
                DamageDealerInfo damageDealerInfor = new DamageDealerInfo();
                damageDealerInfor.damage = m_bodyDamage;
                damageDealerInfor.attacker = transform;
                STGameController.HitPlayer(damageDealerInfor);
            }
        }
    }
    // ------------------------------ event ---------------------------- //
    void OnAnimStart(TrackEntry trackEntry)
    {
        if (isDead)
            return;

        switch (trackEntry.Animation.Name)
        {
            case ANIM_APPEAR:
            case ANIM_IDLE:
            case ANIM_ATTACK3_READY:
            case ANIM_ATTACK4_READY:
                m_bodyState = BodyState.Idle;
                break;
            case ANIM_RUN:
                m_bodyState = BodyState.Move;
                break;
            case ANIM_ATTACK1:
                m_attackIndex = 0;
                m_bodyState = BodyState.Attack;
                break;
            case ANIM_ATTACK3_JUMP_UP:
            case ANIM_ATTACK3_JUMP_DOWN:
            case ANIM_ATTACK3_END:
                m_bodyState = BodyState.Jump;
                break;
            case ANIM_ATTACK4_RUN:
                m_bodyState = BodyState.Move;
                break;
        }
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case ANIM_APPEAR:
                StartCoroutine(IDelayStart());
                break;
            case ANIM_ATTACK1:
                LookAtPlayer();
                if (isNextPhase)
                    ReadyJump();
                else
                    StartCoroutine(IScheduleSecondSkill());
                break;
            case ANIM_ATTACK2:
                FirstSkill();
                break;
            case ANIM_ATTACK3_READY:
                GameController.VibrateCustom(new Vector3(0.3f, 0.3f), 0.5f);
                ThirdSkill();
                break;
            case ANIM_ATTACK3_END:
                StartCoroutine(IScheduleFourSkill());
                break;
        }
    }

    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (isDead)
            return;

        if (e.Data.Name == EVENT_STEP)
        {
            SoundManager.PlaySound3D(m_audioFootsteps, 5f, false, transform.position);
            GameController.ShakeCameraWeak();
            return;
        }

        switch (trackEntry.Animation.Name)
        {
            case ANIM_APPEAR:
                switch (e.Data.Name)
                {
                    case EVENT_SHAKE:
                        GameController.ShakeCameraLoop(1f);
                        break;
                    case EVENT_LAND:
                        GameController.VibrateCustom(new Vector3(0.3f, 0.3f), 0.5f);
                        break;
                }
                break;
            case ANIM_ATTACK1:
                if (e.Data.Name != EVENT_ATTACK)
                    return;
                GameController.ShakeCameraWeak();
                if (m_pushRotinue != null)
                    StopCoroutine(m_pushRotinue);
                m_pushRotinue = StartCoroutine(IPushBody());
                SoundManager.PlaySound3D(m_attackIndex % 2 == 0 ? m_audioSkillAttackVoice1 : m_audioSkillAttackVoice2, 10f, false, transform.position);
                m_attackIndex++;
                break;
            case ANIM_ATTACK2:
            case ANIM_ATTACK3_END:
                if (e.Data.Name != EVENT_ATTACK)
                    return;
                SoundManager.PlaySound3D(m_audioSkillSlamImpact, 50f, false, transform.position);
                GameController.VibrateCustom(new Vector3(0.3f, 0.3f), 0.5f);
                DamageDealerInfo damageDealerInfo = new DamageDealerInfo();
                damageDealerInfo.damage = m_trailDamage;
                damageDealerInfo.attacker = transform;
                SpawnTrail(damageDealerInfo);
                break;
        }
    }
    // ----------------------------------- send message -------------------------------- //
    private void OnWall(Collider2D other)
    {
        if (m_bodyState != BodyState.Move) return;
        onCompleted?.Invoke();
        StopMove();
    }
}
