using System.Collections;
using CleverCrow.Fluid.FSMs;
using UnityEngine;

public class STPlayerController : MonoBehaviour
{
    private const string M_EVENT_JUMP_VIEW = "OnJumpView";

    private readonly Vector2 M_FACE_BOX = new Vector2(0.1f, 0.6f);
    private readonly Vector2 M_FACE_BOX_OFFSET = new Vector2(0.35f, 0.5f);
    private const int M_MAX_AIR_JUMP = 1;
    private const int M_MAX_AIR_DASH = 1;

    [Header("General")]
    [Min(0)] [SerializeField] private float m_timeSaveCombo = 1f;
    [Min(0)] [SerializeField] private float m_moveSpeed = 10f;
    [Min(0)] [SerializeField] private float m_jumpSpeed = 10f;
    [Min(0)] [SerializeField] private float m_baseKnockbackStrength = 20f;
    [Min(0)] [SerializeField] private float m_baseKnockbackTime = 0.25f;
    [Min(0)] [SerializeField] private float m_fightMoveRatio = 0.2f;
    [Min(0)] [SerializeField] private float m_lastTimeJumpFromGround = 0.05f;
    [Min(0)] [SerializeField] private float m_lastTimeJumpFromWall = 0.05f;

    [Header("Wall Settings")] 
    [SerializeField] private bool m_activeWall = false;
    [Min(0)] [SerializeField] private float m_wallKnockbackStrength = 10f;
    [Min(0)] [SerializeField] private float m_wallKnockbackTime = 0.1f;
    [Min(0)] [SerializeField] private float m_wallKnockbackDrag = 1f;
    [Min(1)] [SerializeField] private float m_wallStick = 1f;
    [Min(0)] [SerializeField] private float m_wallBreakStrength = 10f;
    [Min(0)] [SerializeField] private float m_wallBackStrength = 20f;
    [Min(0)] [SerializeField] private float m_wallBackSpeed = 2f;

    [Header("Dash Settings")] 
    [SerializeField] private bool m_activeDash = false;
    [Min(0)] [SerializeField] private float m_dashSpeed = 20f;
    [Min(0)] [SerializeField] private float m_dashTimeDuration = 0.5f;
    [Min(0)] [SerializeField] private float m_dashTimeIdle = 1f;
   
    [HideInInspector]
    [SerializeField] private ContactFilter2D m_groundLayer;
    [HideInInspector]
    [SerializeField] private LayerMask m_wallLayer;
    [HideInInspector]
    [SerializeField] private Rigidbody2D m_physicBody;
    [HideInInspector]
    [SerializeField] private Collider2D m_botBox;

    private bool m_isDead => STGameModel.hp <= 0;
    
    private Vector3 m_baseScale;
    private float m_faceDirection => Mathf.Sign(transform.localScale.x);
    private MoveDirection m_moveDirection = MoveDirection.None;//use for move real time
    private MoveDirection m_moveInput = MoveDirection.None;//use for check input
    private float m_baseVelocX = 0;//base velocity from input
    private float m_extendVelocityX = 0;// use to control some case like climb wall...
    private float m_velocityXMultiplier = 1f;
    
    // private bool m_inSkillCombo => m_animator.GetBool(M_VARIABLE_KEEP_COMBO);
    private int m_airJumpCounter;
    private int m_airDashCounter;
    private bool m_groundJumped;//jumped on ground
    private bool m_lockChangeFaceDirection;

    private bool m_stop => m_fsm.CurrentState.Id.Equals(STPlayerState.Lock);
    private bool m_bonusFight;//save next fight if fire input active when fighting
    private bool m_hurting => m_fsm.CurrentState.Id.Equals(STPlayerState.Hurt);
    private bool m_dashing => m_fsm.CurrentState.Id.Equals(STPlayerState.Dash);
    private bool m_fighting => m_fsm.CurrentState.Id.Equals(STPlayerState.Fight);
    private bool m_activeFightInput;
    private bool m_canRepeatFight;
    private bool m_canDash = true;
    private bool m_ignoreVelocity = false;
    private DamageDealerInfo m_hitCache;//cache for enemy deal damage
    private float m_firstTimeLeaveGround;//first time leave ground
    private float m_firstTimeLeaveWall;//first time leave wall
    private float m_firstTimeWall;//last time stick to wall and begin fall
    private MoveDirection m_lastWallDirection;//last direction stick to wall
    
    //timer
    private Coroutine m_timerIgnoreVelocity;
    private Coroutine m_timerClimb;
    private Coroutine m_timerBounce;
    private Coroutine m_timerGround;
    private Coroutine m_timerImmunity;
    private Coroutine m_timerStayOnAir;
    private Coroutine m_timerCombo;
    private Coroutine m_timerDash;
    
    private readonly Collider2D[] m_groundHits = new Collider2D[1];

    private STPlayerStats m_stats;
    private IFsm m_fsm => m_stats.fsm;
    private Weapon m_weapon => m_stats.weapon;

    void Awake()
    {
        Physics2D.simulationMode = SimulationMode2D.Update;
        Physics2D.queriesStartInColliders = false;
        m_activeDash = PlayerPrefs.GetInt(DataKey.FIRST_BOSS_KILLED, 0) != 0;
        m_stats = GetComponent<STPlayerStats>();
        //
        MainController.equipWeaponEvent += OnEquipWeapon;
        InputController.leftAction += OnMoveLeft;
        InputController.rightAction += OnMoveRight;
        InputController.releaseMove += OnMoveStop;
        InputController.fightAction += OnFight;
        InputController.fightExitAction += OnFightExit;
        InputController.jumpAction += OnJump;
        InputController.dashAction += OnDash;
        STGameController.hitPlayerEvent += OnHurt;
        STGameController.updatePlayerImmunityEvent += OnImmunity;

        GameController.loadSavePointEvent += OnRevive;
        GameController.teleportEvent += OnTeleport;
        GameController.maskTeleClosedEvent += OnMaskTeleClosed;
        GameController.finishEvent += OnFinish;
        GameController.activeInputEvent += OnActiveInput;
        //
        m_baseScale = transform.localScale;
        //
        STGameController.UpdatePlayerHp(STGameConstant.PLAYER_MAX_HEALTH);
    }

    void OnDestroy()
    {
        MainController.equipWeaponEvent -= OnEquipWeapon;
        InputController.leftAction -= OnMoveLeft;
        InputController.rightAction -= OnMoveRight;
        InputController.releaseMove -= OnMoveStop;
        InputController.fightAction -= OnFight;
        InputController.fightExitAction -= OnFightExit;
        InputController.jumpAction -= OnJump;
        InputController.dashAction -= OnDash;
        STGameController.hitPlayerEvent -= OnHurt;
        STGameController.updatePlayerImmunityEvent -= OnImmunity;
        
        GameController.loadSavePointEvent -= OnRevive;
        GameController.teleportEvent -= OnTeleport;
        GameController.maskTeleClosedEvent -= OnMaskTeleClosed;
        GameController.finishEvent -= OnFinish;
        GameController.activeInputEvent -= OnActiveInput;

        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
    }

    private void Start()
    {
        m_stats.fsm = new FsmBuilder()
            .Owner(gameObject)
            .Default(STPlayerState.Other)
            .State(STPlayerState.Other, (state) =>
            {
                state.Enter(OnFsmEnter);
                state.Exit(OnFsmExit);
                state.SetTransition(STPlayerState.Fight);
                state.SetTransition(STPlayerState.Hurt);
                state.SetTransition(STPlayerState.Dash);
                state.SetTransition(STPlayerState.Lock);
            })
            .State(STPlayerState.Fight, (state) =>
            {
                state.Enter(OnFsmFightEnter);
                state.Exit(OnFsmExit);
                state.SetTransition(STPlayerState.Other);
                state.SetTransition(STPlayerState.Hurt);
                state.SetTransition(STPlayerState.Lock);
            })
            .State(STPlayerState.Hurt, (state) =>
            {
                state.Enter(OnFsmHurtEnter);
                state.Exit(OnFsmExit);
                state.SetTransition(STPlayerState.Other);
                state.SetTransition(STPlayerState.Lock);
            })
            .State(STPlayerState.Dash, (state) =>
            {
                state.Enter(OnFsmEnter);
                state.Exit(OnFsmExit);
                state.SetTransition(STPlayerState.Other);
                state.SetTransition(STPlayerState.Hurt);
                state.SetTransition(STPlayerState.Lock);
            })
            .State(STPlayerState.Lock, (state) =>
            {
                state.Enter(OnFsmEnter);
                state.Exit(OnFsmExit);
                state.SetTransition(STPlayerState.Other);
            })
            .Build();
        //
        MainController.EquipWeapon(ConfigLoader.instance.config.GetWeaponBySkinName(MainModel.CurrentWeapon).weaponName);
    }

    void FixedUpdate()
    {
        Vector2 velocity = m_physicBody.velocity;
        //moving
        
        if (!m_ignoreVelocity && !m_stop && m_weapon.canMove)
        {
            if (m_stats.isWall)
                velocity.x = 0;
            else
                velocity.x = (m_baseVelocX + m_extendVelocityX) * m_velocityXMultiplier;
            if (!m_stats.isGround && m_stats.isWall && velocity.y < 0) //stick to wall
                velocity.y = Mathf.Min(0, velocity.y / Mathf.Max(1, m_wallStick - (Time.time - m_firstTimeWall)));
            m_physicBody.velocity = velocity;
            m_stats.speedY = velocity.y;
        }

        //check ground
        Vector2 position = m_physicBody.position;
        int hitCount = m_botBox.OverlapCollider(m_groundLayer, m_groundHits);
        m_stats.isGround = hitCount > 0;//&& m_physicBody.IsTouching(m_groundHits[0]);;

        //wall check
        bool lastWall = m_stats.isWall;
        if (m_activeWall)
        {
            if (velocity.y > 0 || m_stats.isGround || m_fighting || m_hurting)
                m_stats.isWall = false;
            else
            {
                bool ignoreCheck = false;
                Vector2 direction = Vector2.right;
                if (m_dashing)
                    ignoreCheck = true; //direction = m_faceDirection > 0 ? Vector2.right : Vector2.left;
                else
                {
                    switch (m_moveInput)
                    {
                        case MoveDirection.None:
                            if (m_stats.isWall)
                                direction = m_lastWallDirection == MoveDirection.Right ? Vector2.right : Vector2.left;
                            else
                                ignoreCheck = true;
                            break;
                        case MoveDirection.Left:
                            direction = Vector2.left;
                            break;
                        case MoveDirection.Right:
                            direction = Vector2.right;
                            break;
                    }
                }

                if (ignoreCheck)
                    m_stats.isWall = false;
                else
                {
                    Vector2 faceOffset = M_FACE_BOX_OFFSET;
                    faceOffset.x = direction.x > 0 ? faceOffset.x : -faceOffset.x;
                    Vector2 centerWall = position + faceOffset;
                    RaycastHit2D hit = Physics2D.BoxCast(centerWall, M_FACE_BOX, 0, direction, 0.1f, m_wallLayer);
                    m_stats.isWall = hit.collider != null && m_physicBody.IsTouching(hit.collider);
                }
            }
        }
        else
            m_stats.isWall = false;

        if (m_stats.isGround || m_stats.isWall)
        {
            if (m_stats.isGround)
                m_firstTimeLeaveGround = Time.time;
            if (m_stats.isWall)
                m_firstTimeLeaveWall = Time.time;
            m_airJumpCounter = M_MAX_AIR_JUMP;
            m_airDashCounter = M_MAX_AIR_DASH;
            if (m_stats.isWall)
            {
                if (!lastWall)
                    m_firstTimeWall = Time.time;
                if (m_moveInput != MoveDirection.None)
                    m_lastWallDirection = m_moveInput;
            }
        }

        //when player from slide to ground and keep press move need update direction
        if (m_moveInput != MoveDirection.None && lastWall)
            UpdateMove(true);

        if (m_stats.isGround)
            GameController.UpdateTargetYCamera(m_physicBody.position.y);
    }

    private void OnDrawGizmos()
    {
        Vector2 faceOffset = M_FACE_BOX_OFFSET;
        faceOffset.x = m_faceDirection > 0 ? faceOffset.x : -faceOffset.x;
        Vector2 center = (Vector2)transform.position + faceOffset;
        Gizmos.DrawWireCube(center, M_FACE_BOX);
    }
    
    ////////////////////////////////////////FSM EVENT///////////////////////////////////////
    void OnFsmFightEnter(IAction action)
    {
        m_bonusFight = false;
        m_canRepeatFight = false;
        m_velocityXMultiplier = m_fightMoveRatio;
        m_stats.Fight();
        //need to update move if input active to fix case fight immediately after flip
        if(m_moveInput != MoveDirection.None)
            UpdateMove(true);
        m_weapon.ResetDefault();
        if (m_weapon.hasStrain)
        {
            if (m_activeFightInput)
                m_weapon.Strain();
            else
                m_weapon.Fight();
        }
        else if (m_weapon.hasCombo)
        {
            if (m_timerCombo != null)
                StopCoroutine(m_timerCombo);
            m_weapon.ComboUpdate();
        }
        else
            m_weapon.Fight();
    }

    void OnFsmHurtEnter(IAction action)
    {
        m_weapon.CancelStrainIfNeeded();
        m_bonusFight = false;
        m_weapon.ResetCombo();
        if(m_isDead)
        {
            OnDead();
            return;
        }
        m_stats.Hurt();
    }

    void OnFsmEnter(IAction action)
    {
        //Debug.Log($"State Enter - {action.ParentState.Id}");
        switch (action.ParentState.Id)
        {
            case STPlayerState.Other:
                m_canRepeatFight = true;
                break;
            case STPlayerState.Dash:
                if (!m_stats.isGround)
                    m_airDashCounter--;
                m_stats.Dash();
                m_timerDash = StartCoroutine(IDash());
                break;
            case STPlayerState.Lock:
                m_baseVelocX = 0;
                Vector2 velocity = m_physicBody.velocity;
                velocity.x = 0;
                m_physicBody.velocity = velocity;
                break;
        }
    }
   
    void OnFsmExit(IAction action)
    {
        //Debug.Log($"State Exit - {action.ParentState.Id}");
        switch (action.ParentState.Id)
        {
            case STPlayerState.Dash:
                if(m_timerDash != null)
                    StopCoroutine(m_timerDash);
                m_stats.isDash = false;
                if (m_moveInput == MoveDirection.None)
                {
                    m_baseVelocX = 0;
                    m_stats.speedX = 0;
                }
                else
                {
                    m_moveDirection = m_moveInput;
                    UpdateMove(true);
                }
                StartCoroutine(IDashRefesh());
                break;
            case STPlayerState.Fight:
                m_velocityXMultiplier = 1;
                m_stats.isFight = false;
                m_lockChangeFaceDirection = false;
                UpdateFaceDirection();
                m_weapon.CancelStrainIfNeeded();
                if(m_timerCombo != null)
                    StopCoroutine(m_timerCombo);
                m_timerCombo = StartCoroutine(ICombo());
                break;
            case STPlayerState.Hurt:
                m_stats.isHurt = false;
                m_moveDirection = MoveDirection.None;
                break;
        }
    }

    /////////////////////////////////////////////GAME EVENT HANDLER//////////////////////////////////
    void OnRevive(Vector2? position)
    {
        m_canDash = true;
        m_baseVelocX = 0;
        m_extendVelocityX = 0;
        m_moveDirection = MoveDirection.None;
        //
        m_stats.isDead = false;
        StartCoroutine(IRevive());
        m_fsm.CurrentState.Transition(STPlayerState.Other);
    }
    
    void OnDead()
    {
        m_stats.Dead();
        m_baseVelocX = 0;
        m_extendVelocityX = 0;
        m_weapon.CancelStrainIfNeeded();
        m_fsm.CurrentState.Transition(STPlayerState.Lock);
    }
    
    void OnHurt(DamageDealerInfo damage)
    {
        if(m_stop)
            return;
        m_hitCache = damage;
        m_fsm.CurrentState.Transition(STPlayerState.Hurt);
    }

    void OnTeleport(Vector3 destination)
    {
        FreezePosition();
    }
    void OnMaskTeleClosed(Vector3 destination)
    {
        transform.position = destination;
        UnFreeze();
    }
    private void OnActiveInput(bool active)
    {
        if (!active)
            OnMoveStop();
    }
    
    void OnImmunity(bool active, bool fromHit)
    {
        if(m_stop)
            return;
        if (m_timerImmunity != null)
            StopCoroutine(m_timerImmunity);
        m_timerImmunity = null;
        if(active)
            m_timerImmunity = StartCoroutine(IImmunity(fromHit));
    }
    
    void OnEquipWeapon(string skin)
    {
        m_bonusFight = false;
    }
    
    void OnFinish(Vector2 startFinishPoint, Vector2 finishPoint)
    {
        m_fsm.CurrentState.Transition(STPlayerState.Lock);
        m_stats.Finish();
        StartCoroutine(IFinish());
    }
    /////////////////////////////////////////////INPUT HANDLER///////////////////////////////////////
    private void OnJump()
    {
        if(m_hurting)
            return;
        Vector2 velocity = m_physicBody.velocity;
        CancelBounceIfNeeded();
        //player can jump on ground, wall if it stand on ground, wall or leave before m_lastTimeJumpFromGround,m_lastTimeJumpFromWall
        bool isTimeGround = (Time.time - m_firstTimeLeaveGround) <= m_lastTimeJumpFromGround && velocity.y < -0.1f;
        bool isTimeWall = (Time.time - m_firstTimeLeaveWall) <= m_lastTimeJumpFromWall && velocity.y < -0.1f;
        bool fakeGround = (m_stats.isGround && (m_groundHits[0] == null || m_physicBody.IsTouching(m_groundHits[0]))) || m_stats.isWall || isTimeGround || isTimeWall;
        if(m_stop || m_ignoreVelocity || (fakeGround && m_groundJumped) || (!fakeGround && m_airJumpCounter < 1))
            return;
        switch (m_fsm.CurrentState.Id)
        {
            case STPlayerState.Fight:
                if (m_weapon.straining)
                {
                    m_weapon.CancelStrainIfNeeded();
                    m_fsm.CurrentState.Transition(STPlayerState.Other);
                }
                break;
            case STPlayerState.Dash:
                m_fsm.CurrentState.Transition(STPlayerState.Other);
                break;
        }
        if (isTimeGround)
            m_firstTimeLeaveGround = 0;
        if (isTimeWall)
            m_firstTimeLeaveWall = 0;
        if (fakeGround)
            StartCoroutine(IGroundJump());
        else
            m_airJumpCounter--;
        int jumpIndex = fakeGround ? 0 : 1;
        m_stats.Jump(jumpIndex);
        velocity.y = m_jumpSpeed;
        if (m_stats.isWall && !m_stats.isGround)
            m_timerClimb = StartCoroutine(IClimb());
        else if (m_timerClimb != null)
        {
            StopCoroutine(m_timerClimb);
            m_timerClimb = null;
            m_extendVelocityX = 0;
        }
        m_physicBody.velocity = velocity;
        //
        SendMessage(M_EVENT_JUMP_VIEW, jumpIndex);
        UpdateFaceDirection();
    }

    private void OnFight()
    {
        if(m_stop)
            return;
        m_activeFightInput = true;
        if (m_canRepeatFight)
        {
            if (m_fighting)
                m_fsm.CurrentState.Enter();
            else
                m_fsm.CurrentState.Transition(STPlayerState.Fight);
        }
        else
            if (m_fighting && (!m_weapon.hasCombo || m_weapon.comboIndex < (m_weapon.weaponCombos.Count - 1)))////don't save bonus fight for last combo
                m_bonusFight = true;
    }

    private void OnFightExit()
    {
        m_activeFightInput = false;
        if(m_stop || !m_weapon.hasStrain || !m_weapon.straining)
            return;
        m_weapon.Fight();
    }

    private void OnMoveStop()
    {
        m_moveInput = MoveDirection.None;
        if(m_stop || m_dashing || m_moveDirection == MoveDirection.None)
            return;
        m_moveDirection = MoveDirection.None;
        m_stats.speedX = (int)m_moveDirection;
        m_baseVelocX = 0;
    }

    private void OnMoveRight()
    {
        m_moveInput = MoveDirection.Right;
        if(m_stop || m_dashing || m_ignoreVelocity || m_moveDirection == MoveDirection.Right || !m_weapon.canMove)
            return;
        bool isFlip = !m_stats.isMove || m_moveDirection == MoveDirection.None;
        m_moveDirection = MoveDirection.Right;
        UpdateMove(isFlip);
        m_stats.Flip();
    }

    private void OnMoveLeft()
    {
        m_moveInput = MoveDirection.Left;
        if(m_stop || m_dashing || m_ignoreVelocity || m_moveDirection == MoveDirection.Left || !m_weapon.canMove)
            return;
        bool isFlip = !m_stats.isMove || m_moveDirection == MoveDirection.None;
        m_moveDirection = MoveDirection.Left;
        UpdateMove(isFlip);
        m_stats.Flip();
    }
    
    private void OnDash()
    {
        if(m_stop || !m_activeDash || (m_stats.isGround && !m_canDash) || m_ignoreVelocity || (!m_stats.isGround && m_airDashCounter < 1))
            return;
        m_fsm.CurrentState.Transition(STPlayerState.Dash);
    }
    /////////////////////////////////////////////////////PRIVATE/////////////////////////////////////////////
    
    void UpdateMove(bool isFlip)
    {
        if(m_hurting)
            return;
        int moveValue = (int)m_moveDirection;
        m_baseVelocX = moveValue * m_moveSpeed;
        m_stats.speedX = Mathf.Abs(moveValue);
        if(isFlip)
            UpdateFaceDirection();
    }

    void UpdateFaceDirection()
    {
        if(m_stats.isWall || m_moveDirection == MoveDirection.None || m_lockChangeFaceDirection)
            return;
        transform.localScale = new Vector3(m_baseScale.x * (int)m_moveDirection, m_baseScale.y, m_baseScale.z);
    }
        
    
    void FreezePosition()
    {
        m_physicBody.constraints = RigidbodyConstraints2D.FreezeAll;
    }
    
    void UnFreeze()
    {
        m_physicBody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void CancelBounceIfNeeded()
    {
        if (m_timerBounce != null)
        {
            StopCoroutine(m_timerBounce);
            m_ignoreVelocity = false;
        }
        m_timerBounce = null;
    }

    ////////////////////////////////////////////TIMER/////////////////////////////////////////////
    IEnumerator IDash()
    {
        m_canDash = false;
        float start = Time.time;
        m_baseVelocX = m_faceDirection * m_dashSpeed;
        Vector2 velocity = m_physicBody.velocity;
        velocity.y = 0;
        m_physicBody.velocity = velocity;
        float startPosY = m_physicBody.position.y;
        while ((Time.time - start) < m_dashTimeDuration && m_dashing)
        {
            yield return new WaitForFixedUpdate();
            velocity = m_physicBody.velocity;
            velocity.y = 0;
            m_physicBody.velocity = velocity;
            if (m_physicBody.position.y < startPosY)
                m_physicBody.position = new Vector2(m_physicBody.position.x, startPosY);
        }
        m_timerDash = null;
        m_airJumpCounter = 1;
        m_fsm.CurrentState.Transition(STPlayerState.Other);
    }

    IEnumerator IDashRefesh()
    {
        yield return new WaitForSeconds(m_dashTimeIdle);
        m_canDash = true;
    }

    IEnumerator IGroundJump()
    {
        m_groundJumped = true;
        yield return new WaitForSeconds(0.167f);
        m_groundJumped = false;
    }

    IEnumerator IIgnoreVelocity(float time)
    {
        if (Mathf.Approximately(0, time))
            yield return new WaitForEndOfFrame();
        else
        {
            m_ignoreVelocity = true;
            yield return new WaitForSeconds(time);
            m_ignoreVelocity = false;
        }
    }

    IEnumerator IClimb()
    {
        int direction = (int)m_lastWallDirection;
        m_extendVelocityX = -direction * (m_moveInput == MoveDirection.None ? m_wallBreakStrength : m_wallBackStrength);
        MoveDirection beginInput = m_moveInput;
        bool returnWall = false;
        bool activeVelocY = false;
        float speed = m_moveInput == MoveDirection.None ? 1 : m_wallBackSpeed;
        while (!m_stats.isGround && !m_fighting && !m_dashing && !m_hurting && beginInput == m_moveInput)
        {
            float velocY = m_physicBody.velocity.y;
            if (!m_stats.isWall)
                returnWall = true;
            if (velocY > 0)
                activeVelocY = true;
            if((returnWall && m_stats.isWall) || (activeVelocY && velocY <= 0))
                break;
            m_extendVelocityX += direction * m_physicBody.velocity.y * Time.deltaTime * speed;
            yield return new WaitForFixedUpdate();
        }
        m_extendVelocityX = 0;
        m_timerClimb = null;
        if (m_moveInput != MoveDirection.None)
            UpdateMove(true);
    }

    IEnumerator IBounce()
    {
        m_ignoreVelocity = true;
        yield return new WaitForFixedUpdate();
        while (m_moveInput == MoveDirection.None && !m_stop && !m_hurting && !m_dashing && !m_fighting && !m_stats.isGround)
        {
            yield return null;
        }
        m_ignoreVelocity = false;
        m_timerBounce = null;
    }

    IEnumerator IRevive()
    {
        while (!m_stats.isGround)
        {
            yield return new WaitForEndOfFrame();
        }
        GameController.ResumePlayer();
    }

    IEnumerator IFinish()
    {
        Vector2 veloc = m_physicBody.velocity;
        veloc.x = 0;
        m_physicBody.velocity = veloc;
        while (!m_stats.isGround)
        {
            yield return new WaitForEndOfFrame();
        }
        m_stats.isFinish = true;
    }
    
    IEnumerator IImmunity(bool fromHit)
    {
        if (Mathf.Approximately(STGameConstant.IMMUNITY_TIME, 0))
        {
            STGameController.UpdatePlayerImmunity(false, false);
            m_timerImmunity = null;
            yield break;
        }
        while (m_hurting || fromHit)
        {
            if(m_hurting)
                fromHit = false;
            yield return new WaitForEndOfFrame();
        }
        if(m_stop)
            yield break;
        m_stats.isBlink = true;
        yield return new WaitForSeconds(STGameConstant.IMMUNITY_TIME);
        m_stats.isBlink = false;
        STGameController.UpdatePlayerImmunity(false, false);
        m_timerImmunity = null;
    }

    IEnumerator ICombo()
    {
        yield return new WaitForSeconds(m_timeSaveCombo);
        m_bonusFight = false;
        m_weapon.ResetCombo();
        m_timerCombo = null;
    }
    //////////////////////////////////////////SEND MESSAGE/////////////////////////////////
    void OnWeaponHit()
    {
        if(!m_fighting)
            return;
        m_lockChangeFaceDirection = true;
    }

    void OnWeaponRepeat()
    {
        m_canRepeatFight = true;
        if(!m_fighting)
            return;
        m_lockChangeFaceDirection = false;
        UpdateFaceDirection();
        if(m_bonusFight && !m_isDead && !m_stop)
            m_fsm.CurrentState.Enter();
    }

    void OnHitStart()
    {
        //Knockback
        if (Mathf.Approximately(0, m_baseKnockbackStrength))
            m_physicBody.velocity = Vector2.zero;
        else if (m_hitCache != null && m_hitCache.attacker != null)
            Knockback(m_hitCache.attacker.position, m_baseKnockbackStrength, m_baseKnockbackTime);
    }

    void OnFlip()
    {
        if(m_dashing || m_hurting || m_moveDirection == MoveDirection.None)
            return;
        UpdateFaceDirection();
    }

    void OnSlideStart()
    {
        transform.localScale = new Vector3(m_baseScale.x * -(int)m_lastWallDirection, m_baseScale.y, m_baseScale.z);
    }
    
    void OnBounce(Vector2 velocity)
    {
        m_airJumpCounter = 1;
        m_stats.Jump(0);
        m_physicBody.velocity = velocity;
        CancelBounceIfNeeded();
        m_timerBounce = StartCoroutine(IBounce());
    }

    void WallKnockback()
    {
        Transform myTransform = transform;
        Knockback(myTransform.position + new Vector3(m_faceDirection, 0),
            m_wallKnockbackStrength, m_wallKnockbackTime);
    }

    void OnActivateDash(bool toActive)
    {
        m_activeDash = toActive;
    }
    void RenewDash()
    {
        m_canDash = true;
        m_airDashCounter = M_MAX_AIR_DASH;
        if (m_timerDash != null)
            StopCoroutine(m_timerDash);
        m_timerDash = null;
        if (m_dashing)
            m_fsm.CurrentState.Enter();
    }

    /// /////////////////////////////PRIVATE METHOD//////////////////////////////////////
    void Knockback(Vector2 sourcePosition , float knockbackStrength, float time)
    {
        Vector2 playerPos = transform.position;
        Vector2 direction = playerPos.x > sourcePosition.x ? Vector2.right : Vector2.left;
        direction.y =  0.5f;
        direction = knockbackStrength * direction.normalized;
        m_physicBody.velocity = direction;
        if(m_timerIgnoreVelocity != null)
            StopCoroutine(m_timerIgnoreVelocity);
        m_timerIgnoreVelocity = StartCoroutine(IIgnoreVelocity(time));
    }

    ////////////////////////////////PUBLIC METHOD///////////////////////////////////////////
    public void IgnoreVelocity(float time)
    {
        if (m_timerIgnoreVelocity != null)
            StopCoroutine(m_timerIgnoreVelocity);
        m_timerIgnoreVelocity = StartCoroutine(IIgnoreVelocity(time));
    }
}
