using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using System.Linq;
using Spine.Unity;

public class STBossKratos : STObjectBoss
{
    [SerializeField] private string m_startSecondPhaseKey = "";
    [SerializeField] private float m_runSpeed = 6f;
    [SerializeField] private int m_trailDamage = 10;
    [SerializeField] private float m_secondPhaseFriction = 0.5f;
    [SerializeField] private float m_idleBetweenAttackTime = 2f;
    [SerializeField] private float m_idleBetweenSecondSkillTime = 0.5f;
    [SerializeField] private float m_attackSlipForceX = 10;
    [SerializeField] private float m_jumpForce = 20;

    [SerializeField] private Collider2D m_collider;
    [SerializeField] private GameObject m_trailPrefab;
    [SerializeField] private GameObject m_virtualCamera;
    [SerializeField] private ContactFilter2D m_playerLayer;
    [SerializeField] private LayerMask m_wallLayer;
    [SerializeField] private Collider2D m_detectPlayerCollider;
    [SerializeField] private string[] m_jumpAnims;
    [SerializeField] private string[] m_attackAnims; // 1,2,3 is on ground, 3 is on air
    [SerializeField] private Collider2D[] m_hitPlayerArea;
    [SerializeField] private int[] m_damagesBySkill;
    [SerializeField] private AudioClip[] m_audiosBySkill;
    [SerializeField] private AudioClip[] m_audioVoiceBySkill;
    [SerializeField] private AudioClip m_audioTrail;

    private Vector2 m_baseScale;
    private int m_direction;
    private bool m_start;
    private BodyState m_currentState;
    private int m_secondPhaseSkillCounter;
    private bool m_fightOnAir;
    private List<GameObject> m_trails;
    private bool m_detectingPlayerOnAir;
    private float m_baseGravityScale;

    private const string ANIM_APPREAR = "idle";
    private const string ANIM_IDLE = "idle";
    private const string ANIM_RUN = "run";
    private enum BodyState
    {
        Idle,
        Move,
        Attack
    }

#if UNITY_EDITOR
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
                GameController.DoTrigger(m_startSecondPhaseKey, true);
                Idle();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            switch (m_secondPhaseSkillCounter)
            {
                case 0:
                    StartCoroutine(SecondSkill());
                    m_secondPhaseSkillCounter++;
                    break;
                case 1:
                    StartCoroutine(FirstSkill());
                    m_secondPhaseSkillCounter++;
                    break;
                case 2:
                    if (!player.isGround)
                        StartCoroutine(ThirdSkill());
                    else
                        StartCoroutine(FirstSkill());
                    m_secondPhaseSkillCounter = 0;
                    break;
            }
        }
    }
#endif

    public override void Awake()
    {
        base.Awake();

        m_baseScale = transform.localScale;
        m_baseGravityScale = myRigidbody.gravityScale;
        SetupAllDamager();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();

        spine.AnimationState.Complete -= OnAnimComplete;
        spine.AnimationState.Event -= OnAnimEvent;
        CutSceneController.transitionEvent -= OnCutScene;
    }
    public override void OnResumeAfterHit()
    {

    }
    public override void Dead()
    {
        StopAllCoroutines();
        if (deadKeyTrigger != "")
        {
            StartCoroutine(IDelayCutsceneEnd());
        }        
        else
            base.Dead();
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
        if (currentHP / maxHP >= m_secondPhaseFriction && m_startSecondPhaseKey != "")
        {
            GameController.DoTrigger(m_startSecondPhaseKey, true);
            Idle();
        }
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
        base.OnReady();
    }
    public override void Init()
    {
        //m_baseScale = Vector3.one;
        m_direction = m_baseScale.x > 0 ? 1 : -1;
        spine.SetMixSkin("4", "w6");

        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;
        CutSceneController.transitionEvent += OnCutScene;
    }
    public override void StartBoss()
    {
        m_start = true;
        List<Collider2D> playerColliders = player.GetComponents<Collider2D>().ToList();
        foreach (Collider2D col in playerColliders)
        {
            Physics2D.IgnoreCollision(m_collider, col);
        }
        MoveToPlayer();
    }
    IEnumerator IVisible()
    {
        m_virtualCamera.SetActive(true);
        yield return new WaitForSeconds(1f);
        yield return new WaitForSeconds(1f);
        StartCoroutine(IDelayStart());
    }
    IEnumerator IDelayStart()
    {
        spine.transform.localScale = Vector3.one;
        TrackEntry entry = spine.AnimationState.SetAnimation(0, ANIM_APPREAR, false);
        yield return new WaitForSeconds(entry.AnimationEnd);
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        m_virtualCamera.SetActive(false);
        yield return new WaitForSeconds(2f);
        GameController.BossReady();
    }

    private void OnCutScene(bool isFull, float duration, bool finish)
    {
        if (!finish || isDead)
            return;
        switch (m_secondPhaseSkillCounter)
        {
            case 0:
                StartCoroutine(SecondSkill());
                m_secondPhaseSkillCounter++;
                break;
            case 1:
                StartCoroutine(FirstSkill());
                m_secondPhaseSkillCounter++;
                break;
            case 2:
                if (!player.isGround)
                    StartCoroutine(ThirdSkill());
                else
                    StartCoroutine(FirstSkill());
                m_secondPhaseSkillCounter = 0;
                break;
        }
    }
    void OnAnimComplete(TrackEntry trackEntry)
    {
        if(isDead)
            return;
        //switch(trackEntry.Animation.Name)
        //{
            
        //}

    }
    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (isDead)
            return;
        if (e.Data.Name == "attack")
        {
            for (int i = 0; i < m_attackAnims.Length; i++)
            {
                if (m_attackAnims[i] == trackEntry.Animation.Name)
                {
                    SoundManager.PlaySound3D(m_audiosBySkill[i], 20, false, transform.position);
                    SoundManager.PlaySound3D(m_audioVoiceBySkill[i], 20, false, transform.position);
                    HitPlayer(i);
                    if(m_fightOnAir)
                    {
                        if(i == 2)
                            SpawnTrail();
                    }
                    else
                        SlipForward();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDead || !m_start)
            return;
        if(m_currentState == BodyState.Move)
        {
            Vector2 moveDirection = m_direction > 0 ? Vector2.right : Vector2.left;
            myRigidbody.velocity = moveDirection * m_runSpeed;

            if (m_direction < 0 && player.transform.position.x > transform.position.x + 1)
                UpdateDirection(1);
            else if (m_direction > 0 && player.transform.position.x < transform.position.x - 1)
                UpdateDirection(-1);

            if (Walled())
                UpdateDirection(-m_direction);
        }
    }

    void HitPlayer(int skillIndex)
    {
        List<Collider2D> results = new List<Collider2D>();
        m_hitPlayerArea[skillIndex].OverlapCollider(m_playerLayer, results);
        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].tag == GameTag.PLAYER)
            {
                DamageDealerInfo damageDealerInfor = new DamageDealerInfo();
                damageDealerInfor.damage = m_damagesBySkill[skillIndex];
                damageDealerInfor.attacker = transform;
                STGameController.HitPlayer(damageDealerInfor);
            }
        }
    }
    void Idle()
    {
        StopAllCoroutines();
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        StopMoveX();

    }
    void MoveToPlayer()
    {
        if (m_currentState == BodyState.Move)
            return;
        m_detectPlayerCollider.gameObject.SetActive(true);
        UpdateDirection(player.transform.position.x < transform.position.x ? -1 : 1);
        m_currentState = BodyState.Move;
        spine.AnimationState.SetAnimation(0, ANIM_RUN, true);
    }
    void SlipForward()
    {
        myRigidbody.AddForce(new Vector2(m_attackSlipForceX * m_direction, 0), ForceMode2D.Impulse);
    }
    void UpdateDirection(int direction)
    {
        m_direction = direction;
        transform.localScale = new Vector3(-m_direction * m_baseScale.x, m_baseScale.y, 1);
    }
    void StopMoveX()
    {
        m_currentState = BodyState.Idle;
        Vector2 velocity = myRigidbody.velocity;
        velocity.x = 0;
        myRigidbody.velocity = velocity;
    }
    void Freeze()
    {
        myRigidbody.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
    }
    void Unfreeze()
    {
        myRigidbody.gravityScale = m_baseGravityScale - 0.1f;
        myRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        myRigidbody.gravityScale = m_baseGravityScale;
    }
    bool IsGrounded()
    {
        RaycastHit2D raycastHit2D;
        raycastHit2D = Physics2D.Raycast(transform.position, Vector2.down, 0.3f, m_wallLayer);
#if UNITY_EDITOR
        Debug.DrawRay(transform.position, Vector2.down * 0.2f, Color.blue);
#endif
        return raycastHit2D.collider != null;
    }
    bool ReachJumpAttackPosition()
    {
        RaycastHit2D raycastHit2D;
        raycastHit2D = Physics2D.Raycast(transform.position, Vector2.down, 5, m_wallLayer);
#if UNITY_EDITOR
        Debug.DrawRay(transform.position, Vector2.down * 5, Color.blue);
#endif
        return raycastHit2D.collider != null;
    }
    bool Walled()
    {
        RaycastHit2D raycastHit2D;
        raycastHit2D = Physics2D.Raycast(center.position, m_direction < 0 ? Vector2.left : Vector2.right, m_collider.bounds.size.x, m_wallLayer);
#if UNITY_EDITOR
        Debug.DrawRay(transform.position, (m_direction < 0 ? Vector2.left : Vector2.right) * m_collider.bounds.size.x, Color.yellow);
#endif
        return raycastHit2D.collider != null;
    }
    void SpawnTrail()
    {
        m_trails = new List<GameObject>();
        RaycastHit2D raycastHit2D;
        raycastHit2D = Physics2D.Raycast(center.position, m_direction < 0 ? Vector2.left : Vector2.right, 20, m_wallLayer);
        if (raycastHit2D.collider == null)
            return;
#if UNITY_EDITOR
        Debug.DrawLine(center.position, raycastHit2D.point, Color.green, 5f);
#endif
        float distance = Mathf.Abs(center.position.x - raycastHit2D.point.x);
        int numberOfEffect = (int)distance;
        // spawn
        for(int i = 0; i < numberOfEffect; i++)
        {
            GameObject eff = Instantiate(m_trailPrefab, transform.parent);
            eff.SetActive(false);
            eff.transform.position = transform.position + new Vector3(m_direction * (i + 1), 0, 0);
            m_trails.Add(eff);
        }
        StartCoroutine(ITrailSpreadOut());
    }
    IEnumerator ITrailSpreadOut()
    {
        if (m_trails.Count == 0)
            yield break;
        for(int i = 0; i < m_trails.Count; i++)
        {
            Collider2D effectCollider = m_trails[i].GetComponent<Collider2D>();
            m_trails[i].SetActive(true);
            SoundManager.PlaySound3D(m_audioTrail, 10, false, m_trails[i].transform.position);
            GameController.ShakeCameraWeak();
            effectCollider.enabled = true;
            yield return new WaitForSeconds(0.02f);
            effectCollider.enabled = false;
            yield return new WaitForSeconds(0.02f);
        }
    }
    
    IEnumerator IDelayCutsceneEnd()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1;
        base.Dead();
    }
    IEnumerator FirstSkill()
    {
        m_fightOnAir = false;
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        yield return new WaitForSeconds(m_idleBetweenSecondSkillTime);
        TrackEntry first = spine.AnimationState.SetAnimation(0, m_attackAnims[4], false);
        yield return new WaitForSeconds(first.AnimationEnd);
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        yield return new WaitForSeconds(m_idleBetweenSecondSkillTime);
        first = spine.AnimationState.SetAnimation(0, m_attackAnims[1], false);
        yield return new WaitForSeconds(first.AnimationEnd);
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        yield return new WaitForSeconds(m_idleBetweenSecondSkillTime);
        first = spine.AnimationState.SetAnimation(0, m_attackAnims[2], false);
        yield return new WaitForSeconds(first.AnimationEnd);
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        StopMoveX();
        yield return new WaitForSeconds(m_idleBetweenAttackTime);
        MoveToPlayer();
    }
    IEnumerator SecondSkill()
    {
        m_fightOnAir = false;
        TrackEntry first = spine.AnimationState.SetAnimation(0, m_attackAnims[3], false);
        yield return new WaitForSeconds(first.AnimationEnd);
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        yield return new WaitForSeconds(m_idleBetweenSecondSkillTime);
        TrackEntry second = spine.AnimationState.SetAnimation(0, m_attackAnims[2], false);
        yield return new WaitForSeconds(second.AnimationEnd);
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        yield return new WaitForSeconds(m_idleBetweenSecondSkillTime);
        TrackEntry third = spine.AnimationState.SetAnimation(0, m_attackAnims[2], false);
        yield return new WaitForSeconds(third.AnimationEnd);
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        yield return new WaitForSeconds(m_idleBetweenAttackTime);
        MoveToPlayer();
    }
    IEnumerator ThirdSkill()
    {
        m_fightOnAir = true;
        myRigidbody.AddForce(new Vector2(0, m_jumpForce), ForceMode2D.Impulse);
        TrackEntry trackEntry = spine.AnimationState.SetAnimation(0, m_jumpAnims[0], false);
        yield return new WaitForSeconds(trackEntry.AnimationEnd);
        trackEntry = spine.AnimationState.SetAnimation(0, m_jumpAnims[1], true);
        //yield return new WaitForSeconds(0.2f);

        //
        Freeze();
        TrackEntry first = spine.AnimationState.SetAnimation(0, m_attackAnims[0], false);
        yield return new WaitForSeconds(first.AnimationEnd);
        Unfreeze();
        TrackEntry second = spine.AnimationState.SetAnimation(0, m_attackAnims[1], false);
        yield return new WaitForSeconds(second.AnimationEnd);
        //Unfreeze();

        trackEntry = spine.AnimationState.SetAnimation(0, m_jumpAnims[2], false);
        yield return new WaitForSeconds(trackEntry.AnimationEnd);
        trackEntry = spine.AnimationState.SetAnimation(0, m_jumpAnims[3], true);

        while(!IsGrounded())
        {
            yield return null;
        }

        trackEntry = spine.AnimationState.SetAnimation(0, m_attackAnims[2], false);
        yield return new WaitForSeconds(trackEntry.AnimationEnd);

        trackEntry = spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);

        yield return new WaitForSeconds(m_idleBetweenAttackTime);
        MoveToPlayer();
    }

    void SetupAllDamager()
    {
        m_trailPrefab.GetComponent<STObjectDealDamage>().UpdateDamage(m_trailDamage);
    }
    IEnumerator EvaluatePlayerOnAir()
    {
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        StopMoveX();
        m_detectingPlayerOnAir = true;
        bool playerOnAir = !player.isGround;
        float timer = 0;
        while(timer <= 1f && !playerOnAir)
        {
            playerOnAir = !player.isGround;
            yield return null;
            timer += Time.deltaTime;
        }
        if(playerOnAir)
            StartCoroutine(ThirdSkill());
        else
            StartCoroutine(FirstSkill());
        m_detectingPlayerOnAir = false;
    }

    // Message Listener
    public void ReachPlayer(Collider2D other)
    {
        if (m_currentState == BodyState.Attack || isDead)
            return;
        if (other.offset.y < 0.5f)
            return;
        m_detectPlayerCollider.gameObject.SetActive(false);
        m_currentState = BodyState.Attack;
        StopMoveX();

        if(currentHP / maxHP >= m_secondPhaseFriction)
        {
            StartCoroutine(FirstSkill());
        }
        else if(!m_detectingPlayerOnAir)
        {
            switch(m_secondPhaseSkillCounter)
            {
                case 0:
                    StartCoroutine(SecondSkill());
                    m_secondPhaseSkillCounter++;
                    break;
                case 1:
                    StartCoroutine(FirstSkill());
                    m_secondPhaseSkillCounter++;
                    break;
                case 2:
                    StartCoroutine(EvaluatePlayerOnAir());
                    //if(!player.isGround)
                    //    StartCoroutine(ThirdSkill());
                    //else
                    //    StartCoroutine(FirstSkill());
                    m_secondPhaseSkillCounter = 0;
                    break;
            }
        }
    }
}
