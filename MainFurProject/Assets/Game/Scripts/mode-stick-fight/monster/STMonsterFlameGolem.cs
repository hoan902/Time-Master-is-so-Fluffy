using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class STMonsterFlameGolem : STObjectMonster
{
    [Header("Config")]
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_bulletDamage = 10;
    [SerializeField] private float m_patrolTimescale = 1f;
    [SerializeField] private float m_flyTimeScale = 2f;
    [SerializeField] private float m_moveTime = 4f;
    [SerializeField] private float m_atkDelayTime = 1f;
    [SerializeField] private Vector2 m_activeRangeSize = Vector2.one;
    [SerializeField] private Vector2 m_activeRangeOffset = Vector2.zero;
    [SerializeField] private float m_shakeCameraDistance = 10;
    [SerializeField] private Vector3[] m_path;
    [SerializeField] private LayerMask m_groundLayer;

    [Header("Reference")]
    [SerializeField] private STObjectDealDamage m_bodyDamageDealer;
    [SerializeField] private BoxCollider2D m_activeRangeCollider;
    [SerializeField] private GameObject m_bullet;
    [SerializeField] private STObjectDealDamage m_bulletDamageDealer;
    [SerializeField] private Transform m_shotPoint;
    [SerializeField] private GameObject m_body;
    [SerializeField] private AudioClip m_audioWalk;
    [SerializeField] private AudioClip m_audioAttack;
    [SerializeField] private AudioClip m_audioDead;

    [Header("Anim Name")]
    [SpineAnimation]
    [SerializeField] private string m_animAtk;
    [SpineAnimation]
    [SerializeField] private string m_animAtk2;
    [SpineAnimation]
    [SerializeField] private string m_animDead;
    [SpineAnimation]
    [SerializeField] private string m_animFly;
    [SpineAnimation]
    [SerializeField] private string m_animIdle;

    private int m_direction;
    private Vector2 m_baseScale;
    private Bounds m_bounds;
    private GameObject m_soundMove;
    private BoxCollider2D m_collider;
    private bool m_moving;
    private bool m_playerInRange;
    private Tweener m_moveTweener;
    private float m_lastPosX;
    private bool m_canAttack = true;
    private Coroutine m_deadCoroutine;
    private int m_lastDirection;

    //------------------------------ Mono Behaviour basic class --------------------------------
    private void Start()
    {
        if (!myRigidbody.simulated)
            return;
        Patrol();
    }
    private void FixedUpdate()
    {
        if (isDead || !startBehaviour)
            return;
        if (m_moveTweener == null)
            return;
        if (m_moveTweener.timeScale < 0.01f)
            return;
        //is not moving (attking or doing something else)
        if (!m_moving)
            return;

        //update dir when move
        /*int direction = transform.position.x < m_lastPosX ? -1 : 1;
        UpdateDirection(direction);
        m_lastPosX = transform.position.x;*/
    }

    //------------------------------ Override classes (also have some Mono Behaviour) --------------------------------
    public override void Awake()
    {
        base.Awake();
        m_lastPosX = transform.position.x;
        m_collider = GetComponent<BoxCollider2D>();
        m_baseScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
        m_direction = transform.localScale.x > 0 ? 1 : -1;
        m_bounds = m_collider.bounds;
        m_activeRangeCollider.size = m_activeRangeSize;
        m_activeRangeCollider.offset = m_activeRangeOffset;
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
        m_bodyDamageDealer.UpdateDamage(m_bodyDamage);
        m_lastDirection = m_direction;

        if (m_path.Length > 0)
        {
            transform.position = m_path[0];
            m_moveTweener = transform.DOPath(m_path, m_moveTime).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
            m_moveTweener.OnWaypointChange((a) => 
            {
                int direction = transform.position.x > m_lastPosX ? -1 : 1;
                UpdateDirection(direction);
                m_lastPosX = transform.position.x;
            });
            m_moveTweener.Pause();
        }

        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        StopSoundMove();

        spine.AnimationState.Complete -= OnAnimComplete;
        spine.AnimationState.Event -= OnAnimEvent;
    }
    public override void Attack()
    {
        if(isDead)
            return;
        base.Attack();
        m_canAttack = false;
        //if attack avaliable:
        //int curDir = m_direction;
        UpdateDirection(transform.position.x > player.transform.position.x ? -1 : 1);
        if (m_moveTweener != null)
            m_moveTweener.Pause();
        m_moving = false;
        spine.AnimationState.SetAnimation(0, m_animAtk2, false).Complete += (a) =>
        {
            //UpdateDirection(curDir);
            //if (m_moveTweener != null)
            //{
            //    m_moveTweener.timeScale = m_flyTimeScale;
            //    m_moveTweener.Play();
            //}
            //m_moving = true;
            //spine.AnimationState.SetAnimation(0, m_animFly, true);
            //StopSoundMove();
            //PlaySoundMove(m_audioRun);
        };
    }
    public override void AttackComplete()
    {
        if (isDead)
            return;
        base.AttackComplete();
        StartCoroutine(IEDelayAttack());

        if(!m_playerInRange)
            Patrol();
    }
    public override void PlayerInRange(Collider2D other)
    {
        if (isDead)
            return;

        m_playerInRange = true;
        if (m_canAttack)
            Attack();
    }

    public override void PlayerOutRange(Collider2D other)
    {
        if (isDead || bodyState == State.Attacking)
            return;
        m_playerInRange = false;

        if(bodyState != State.Attacking)
            Patrol();
    }

    public override void Dead()
    {
        if (m_deadCoroutine != null)
            return;
        StopAllCoroutines();
        m_moveTweener?.Kill();
        m_deadCoroutine = StartCoroutine(IDead());
    }
    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        Destroy(gameObject);
    }
    public override void OnResumeAfterHit()
    {
        
    }
    public override void StartBehaviour()
    {
        base.StartBehaviour();
        Patrol();
        m_activeRangeCollider.gameObject.SetActive(true);
    }
    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
        m_activeRangeCollider.gameObject.SetActive(false);
    }


    //------------------------------ Private --------------------------------
    void OnAnimComplete(TrackEntry trackEntry)
    {
        if(isDead)
            return;
        switch(trackEntry.Animation.Name)
        {
            case string atkAnim when atkAnim == m_animAtk2:
                //Check if player out of range yet -> either patrol or continue attack
                AttackComplete();
                break;
        }
    }
    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (isDead)
            return;
        switch (trackEntry.Animation.Name)
        {
            case string atkAnim when atkAnim == m_animAtk2:
                SoundManager.PlaySound3D(m_audioAttack, 15, false, transform.position);
                DropFireBoulder();
                break;
        }
    }

    void DropFireBoulder()
    {
        GameObject fireBoulder = Instantiate(m_bullet, m_shotPoint.position, Quaternion.identity, transform.root);
        fireBoulder.SetActive(true);
        Rigidbody2D fireBoulderRgbd = fireBoulder.GetComponent<Rigidbody2D>();
        //fireBallLeftRgbd.velocity = finalVelocityLeft;
        Vector2 dir = (m_direction == 1 ? Vector2.right : Vector2.left);
        fireBoulderRgbd.AddForce(dir * fireBoulderRgbd.mass, ForceMode2D.Impulse);
        fireBoulder.GetComponent<STEnemyPhysicBoulder>().Init(dir, 5);
    }
    void Patrol()
    {
        if (m_moveTweener != null)
        {
            m_moveTweener.timeScale = m_patrolTimescale;
            m_moveTweener.Play();
        }
        m_moving = true;
        if(spine.AnimationState.GetCurrent(0).Animation.Name != m_animIdle)
            spine.AnimationState.SetAnimation(0, m_animIdle, true);
        StopSoundMove();
        PlaySoundMove(m_audioWalk);
    }
    void UpdateDirection(int direction)
    {
        m_direction = direction;
        transform.localScale = new Vector3(m_baseScale.x * direction, m_baseScale.y, 1);
        healthProgress.transform.parent.localScale = new Vector3(m_direction, 1, 1);
    }
    void PlaySoundMove(AudioClip audioClip)
    {
        if (m_soundMove != null || isDead)
            return;
        m_soundMove = SoundManager.PlaySound3D(audioClip, 10, true, transform.position);
    }
    void StopSoundMove()
    {
        if (m_soundMove != null)
            Destroy(m_soundMove);
    }
    bool CheckGrounded()
    {
        RaycastHit2D leftCastHit = Physics2D.Raycast(m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, 0), Vector2.down, m_collider.bounds.extents.y + 0.3f, m_groundLayer);
        RaycastHit2D rightCastHit = Physics2D.Raycast(m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, 0), Vector2.down, m_collider.bounds.extents.y + 0.3f, m_groundLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, 0), Vector3.down * (m_collider.bounds.extents.y + 0.3f), Color.yellow);
        Debug.DrawRay(m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, 0), Vector3.down * (m_collider.bounds.extents.y + 0.3f), Color.yellow);
#endif
        return leftCastHit.collider != null || rightCastHit != null;
    }

    //------------------------------ Public --------------------------------
    public Vector3[] GetPath()
    {
        return m_path;
    }
    public void UpdatePath(Vector3 value, int index)
    {
        m_path[index] = value;
    }

    //------------------------------ IEnummerator --------------------------------
    IEnumerator IEDelayAttack()
    {
        spine.AnimationState.SetAnimation(0, m_animIdle, true);
        if (m_moveTweener != null)
            m_moveTweener.Pause();
        yield return new WaitForSeconds(m_atkDelayTime);
        m_canAttack = true;
        //Attack();
    }
    IEnumerator IDead()
    {
        spine.AnimationState.SetAnimation(0, "idle", true);
        m_moveTweener.Kill();
        myRigidbody.bodyType = RigidbodyType2D.Dynamic;
        m_body.SetActive(false);
        StopSoundMove();

        while(!CheckGrounded())
        {
            yield return null;
        }

        base.Dead();
        SoundManager.PlaySound(m_audioDead, false);
    }
    //------------------------------ OnEditor --------------------------------
    private void OnDrawGizmosSelected()
    {
        m_activeRangeCollider.size = m_activeRangeSize;
        m_activeRangeCollider.offset = m_activeRangeOffset;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(STMonsterFlameGolem))]
public class STMonsterFlameGolemEditor : Editor
{
    void OnSceneGUI()
    {
        STMonsterFlameGolem t = (target as STMonsterFlameGolem);
        Vector3[] path = t.GetPath();
        Handles.color = Color.red;
        Handles.DrawPolyLine(path);
        for (int i = 0; i < path.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector2 pos = Handles.PositionHandle(path[i], Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "change path");
                t.UpdatePath(pos, i);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

    
