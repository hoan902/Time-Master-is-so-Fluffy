using DG.Tweening;
using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STBossBoneLord : STObjectBoss
{
    private enum BodyState
    {
        Idle,
        Attack,
        Move,
        Summon,
        Run
    }
    private const string ANIM_APPEAR = "xuathien";
    private const string ANIM_IDLE = "idle";
    private const string ANIM_ATTACK1 = "skill_1";
    private const string ANIM_ATTACK2_BURROWED_DOWN = "skill_2_1";
    private const string ANIM_ATTACK2_MOVE_UNDERGROUND = "skill_2_2";
    private const string ANIM_ATTACK2_RISE_UP = "skill_2_3";
    private const string ANIM_ATTACK3 = "skill_3";
    private const string ANIM_DEAD = "dead";
    private const string ANIM_RUN = "run";

    private const string EVENT_ATTACK = "attack";
    private const string EVENT_DONE = "done";
    private const string EVENT_SHAKE = "shake";
    private const string EVENT_GROWL = "growl";

    [Range(0f, 1f)][SerializeField] private float m_nextPhaseRatio = 1f;
    [SerializeField] private float m_timeScaleAnimNextPhase = 1.3f;
    [SerializeField] private float m_moveSpeed = 5f;
    [SerializeField] private int m_damage = 10;
    [SerializeField] private int m_bulletDamage = 10;
    [SerializeField] private int m_rainStoneDamage = 5;
    [SerializeField] private float m_followTargetUndergroundTime = 3f;
    [SerializeField] private float m_rainStoneTime = 5f;
    [SerializeField] private float m_waitFinishSkill3Time = 2f;
    [SerializeField] private int m_densityBullet = 3;
    [SerializeField] private int m_densityRainStone = 20;
    [SerializeField] private int m_densityStoneRiseUp = 2;
    [SerializeField] private float m_rainStoneGravity = 2f;
    [SerializeField] private int m_maxMonsterSpawn = 3;
    [SerializeField] private float m_maxBulletHeight = 5f;

    [SerializeField] private GameObject m_portal;
    [SerializeField] private GameObject m_virtualCamera;
    [SerializeField] private Transform m_shotPoint;
    [SerializeField] private GameObject m_objectBullet;
    [SerializeField] private BoxCollider2D m_areaRain;
    [SerializeField] private BoxCollider2D m_areaSummon;
    [SerializeField] private GameObject m_childPortal;
    [SerializeField] private GameObject m_monster;
    [SerializeField] private LayerMask m_layerMask;

    [Header("Sounds")]
    [SerializeField] private AudioClip m_appearSlam;
    [SerializeField] private AudioClip m_appearVoice;
    [SerializeField] private AudioClip m_groundDig;
    [SerializeField] private AudioClip m_groundDigVoice;
    [SerializeField] private AudioClip m_meteorCallSlam;
    [SerializeField] private AudioClip m_meteorCallVoice;
    [SerializeField] private AudioClip m_meteorTossVoice;
    [SerializeField] private AudioClip m_hitAudio;

    private bool isNextPhase { get => (m_currentHP / maxHP) <= m_nextPhaseRatio; }

    private BodyState m_bodyState;
    private Vector2 m_baseScale;
    private Vector2 m_spineScale;
    private int m_direction;
    private float m_baseSpeed;
    private float m_baseTimeScale;
    private float m_currentTimeScale;
    private bool m_start;
    private Vector2? m_destination;
    private Vector2 m_moveDirection;
    private List<GameObject> m_portals;
    private List<GameObject> m_currentMonsters;
    private List<Vector2> m_cacheSpawnPosition;
    private GameObject m_body;
    private Collider2D m_collider;
    private GameObject m_audioGroundDig;

    public override void Awake()
    {
        base.Awake();

        m_baseSpeed = m_moveSpeed;
        m_body = transform.Find("body").gameObject;
        m_portals = new List<GameObject>();
        m_currentMonsters = new List<GameObject>();
        m_collider = GetComponent<Collider2D>();
        m_areaRain.transform.SetParent(transform.parent);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        spine.AnimationState.Start -= OnAnimStart;
        spine.AnimationState.Complete -= OnAnimComplete;
        spine.AnimationState.Event -= OnAnimEvent;

        GameController.monsterDeadEvent -= OnMonsterDead;
    }

    public override void OnResumeAfterHit()
    {

    }

    public override void Dead()
    {
        base.Dead();
        StopMove();
        StopAllCoroutines();

        foreach (GameObject monster in m_currentMonsters)
        {
            Destroy(monster);
        }
        foreach (GameObject portal in m_portals)
        {
            Destroy(portal);
        }
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
        m_currentTimeScale = isNextPhase ? m_timeScaleAnimNextPhase : m_baseTimeScale;
        if (spine.timeScale != m_currentTimeScale)
            spine.timeScale = m_currentTimeScale;
        SoundManager.PlaySound3D(m_hitAudio, 10f, false, transform.position);
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

        m_baseTimeScale = spine.timeScale;
        m_currentTimeScale = isNextPhase ? m_timeScaleAnimNextPhase : m_baseTimeScale;

        spine.AnimationState.Start += OnAnimStart;
        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;
        spine.gameObject.SetActive(false);

        GameController.monsterDeadEvent += OnMonsterDead;
    }

    public override void StartBoss()
    {
        m_start = true;
        FirstSkill();
    }

    private void FixedUpdate()
    {
        if (isDead || !m_start)
            return;
        if (m_destination == null)
            return;
        myRigidbody.velocity = m_moveDirection * m_baseSpeed;
        bool stopMove = false;
        if (m_bodyState == BodyState.Run)
            stopMove = Vector3.Distance(transform.position, m_destination.Value) <= (0.2f * m_moveSpeed / m_baseSpeed);
        else
            stopMove = Mathf.Approximately(Vector2.Distance(transform.position, m_destination.Value), 0f);
        if (stopMove)
        {
            StopMove();
            if (m_bodyState == BodyState.Run)
            {
                StartCoroutine(IScheduleResetAllSkill());
            }
        }
    }

    // ------------------------------ timer ---------------------------- //
    private IEnumerator IVisible()
    {
        m_virtualCamera.SetActive(true);
        yield return new WaitForSeconds(1f);
        spine.transform.localScale = Vector3.zero;
        spine.gameObject.SetActive(true);
        spine.AnimationState.SetAnimation(0, ANIM_APPEAR, false);
        yield return null;
        yield return null;
        yield return null;
        spine.transform.localScale = m_spineScale;
    }

    private IEnumerator IDelayStart()
    {
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        m_virtualCamera.SetActive(false);
        yield return new WaitForSeconds(1f);
        GameController.BossReady();
    }

    private IEnumerator IScheduleSecondSkill()
    {
        TrackEntry trackEntry = spine.AnimationState.SetAnimation(0, ANIM_IDLE, false);
        yield return new WaitForSeconds(trackEntry.AnimationEnd);
        BurrowedDownSecondSkill();
    }

    private IEnumerator IFollowTargetSecondSkill()
    {
        MoveSecondSkill();
        float duration = m_followTargetUndergroundTime;
        float defaultTime = 0.1f * m_currentTimeScale;
        m_audioGroundDig = SoundManager.PlaySound3D(m_groundDig, 20f, true, transform.position);
        m_audioGroundDig.transform.SetParent(transform);
        m_audioGroundDig.transform.localPosition = Vector3.zero;
        while (duration > 0)
        {
            Vector2 target = player.transform.position;
            target.y = transform.position.y;
            MoveTo(target, m_currentTimeScale);
            yield return new WaitForSeconds(defaultTime);
            duration -= defaultTime;
        }
        Destroy(m_audioGroundDig);
        StopMove();
        RiseUpSecondSkill();
    }

    private IEnumerator IScheduleThirdSkill()
    {
        TrackEntry trackEntry = spine.AnimationState.SetAnimation(0, ANIM_IDLE, false);
        yield return new WaitForSeconds(trackEntry.AnimationEnd);
        ThirdSkill();
    }

    private IEnumerator IWaitMove()
    {
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        yield return new WaitForSeconds(m_waitFinishSkill3Time);
        MoveToWall();
    }

    private IEnumerator IScheduleResetAllSkill()
    {
        TrackEntry trackEntry = spine.AnimationState.SetAnimation(0, ANIM_IDLE, false);
        yield return new WaitForSeconds(trackEntry.AnimationEnd);
        FirstSkill();
    }

    private IEnumerator IWaitStone(float duration, Vector2 localPos)
    {
        yield return new WaitForSeconds(duration);
        GameObject bullet = Instantiate(m_objectBullet, Vector3.zero, Quaternion.identity);
        bullet.GetComponent<STEnemyBullet>().Init(Vector2.zero, 0);
        bullet.GetComponent<STObjectDealDamage>().UpdateDamage(m_rainStoneDamage);
        Rigidbody2D bulletRig = bullet.GetComponent<Rigidbody2D>();
        bullet.transform.position = localPos;
        bullet.SetActive(true);
        bulletRig.gravityScale = m_rainStoneGravity;
    }

    private IEnumerator IDelayDestroyPortal()
    {
        yield return new WaitForSeconds(2.2f);
        foreach (GameObject portal in m_portals)
        {
            Destroy(portal);
        }
    }

    private IEnumerator IShakeRainStone(float duration)
    {
        float time = 0.1f;
        int total = (int)(duration / time) + 1;
        for (int i = 0; i < total; i++)
        {
            GameController.VibrateCustom(new Vector3(0.2f, 0.2f), time);
            yield return new WaitForSeconds(time);
        }
    }

    // ------------------------------ skill ---------------------------- //
    private void FirstSkill()
    {
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK1, false);
    }

    private void BurrowedDownSecondSkill()
    {
        SoundManager.PlaySound3D(m_groundDigVoice, 20, false, transform.position);
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK2_BURROWED_DOWN, false);
    }

    private void MoveSecondSkill()
    {
        m_bodyState = BodyState.Move;
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK2_MOVE_UNDERGROUND, true);
    }

    private void RiseUpSecondSkill()
    {
        SoundManager.PlaySound3D(m_groundDigVoice, 20, false, transform.position);
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK2_RISE_UP, false);
    }

    private void ThirdSkill()
    {
        SoundManager.PlaySound3D(m_meteorCallVoice, 20, false, transform.position);
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK3, false);
    }

    private void MoveToWall()
    {
        m_bodyState = BodyState.Run;
        Vector2 target = FindWallMove();
        MoveTo(target, m_currentTimeScale);
        spine.AnimationState.SetAnimation(0, ANIM_RUN, true);
    }

    private void ColliderBody(bool active)
    {
        m_body.SetActive(active);
        m_collider.enabled = active;
    }

    private void ThrowStone()
    {
        SoundManager.PlaySound3D(m_meteorTossVoice, 50f, false, m_shotPoint.position);
        List<Vector3> arrayVec = new List<Vector3>();
        float maxDistance = Vector2.Distance((Vector2)transform.position, (Vector2)player.transform.position) / 3f * m_direction;
        for (int i = -1; i < m_densityBullet - 1; i++)
        {
            Vector2 vec = new Vector2(maxDistance * i, 0);
            arrayVec.Add(vec);
        }
        for (int i = 0; i < m_densityBullet; i++)
        {
            GameObject bullet = Instantiate(m_objectBullet, m_shotPoint.position, Quaternion.identity, transform.root);
            bullet.SetActive(true);
            Rigidbody2D bulletRig = bullet.GetComponent<Rigidbody2D>();
            Vector2 targetPos = player.transform.position + arrayVec[i];
            targetPos.y = transform.position.y;
            bullet.GetComponent<STObjectDealDamage>().UpdateDamage(m_bulletDamage);

            Vector2 finalVelocity = Blobcreate.ProjectileToolkit.Projectile.VelocityByHeight(bullet.transform.position, targetPos, m_maxBulletHeight - (m_densityBullet - i) / 3f);
            bulletRig.AddForce(finalVelocity * bulletRig.mass, ForceMode2D.Impulse);
            bullet.GetComponent<STEnemyBullet>().Init(Vector2.zero, 0);
            bulletRig.gravityScale = 1;
        }
    }

    private void RainStone()
    {
        float maxTime = 0f;
        for (int i = 0; i < m_densityRainStone; i++)
        {
            int temp = 0;
            Vector2 rand = Vector2.zero;
            do
            {
                if (temp > 100)
                {
                    Debug.LogFormat("[DEBUG] - {0} - Don't choose position stone!!!", i);
                    break;
                }
                temp++;
                rand = GetRandomPointInsideCollider(m_areaRain);
            } while (IsObstacle(rand, Vector2.one * 3f));

            float duration = Random.Range(0f, m_rainStoneTime);
            maxTime = (duration > maxTime) ? duration : maxTime;
            StartCoroutine(IWaitStone(duration, rand));
        }
        //StartCoroutine(IShakeRainStone(maxTime));
    }

    private void SpawnMonster()
    {
        m_currentMonsters.Clear();
        m_cacheSpawnPosition = GetRandomPointInsideAreaSummon(20, 100);
        List<int> choosenPos = new List<int>();
        for (int i = 0; i < m_maxMonsterSpawn; i++)
        {
            Vector2 spawnPos;
            int randPosIndex = 0;
            do
            {
                randPosIndex = Random.Range(0, m_cacheSpawnPosition.Count);
            } while (choosenPos.Contains(randPosIndex) || IsObstacle(m_cacheSpawnPosition[randPosIndex], Vector2.one));
            choosenPos.Add(randPosIndex);
            spawnPos = m_cacheSpawnPosition[randPosIndex];

            GameObject monster = Instantiate(m_monster, spawnPos, Quaternion.identity, transform.parent);
            monster.SetActive(false);
            STObjectMonster monsterComp = monster.GetComponent<STObjectMonster>();
            monsterComp.PauseBehaviour();
            m_currentMonsters.Add(monster);

            GameObject portal = Instantiate(m_childPortal, spawnPos, Quaternion.identity, transform.parent);
            portal.SetActive(true);
            portal.transform.localScale = Vector3.zero;
            m_portals.Add(portal);

            portal.transform.DOScale(Vector3.one, 1).OnComplete(() =>
            {
                monster.SetActive(true);
                Vector3 baseScale = monster.transform.localScale;
                monster.transform.localScale = Vector3.zero;

                monster.transform.DOScale(baseScale, 1).OnComplete(() =>
                {
                    monsterComp.StartBehaviour();
                });
            });
        }
        StartCoroutine(IDelayDestroyPortal());
    }

    private void RiseUpThrowStone(int stone)
    {
        float xPos = 4;
        Vector3 localPos = transform.position + new Vector3(0, 2f);
        for (int i = 1; i <= stone; i++)
        {
            // left
            GameObject bullet = Instantiate(m_objectBullet, localPos, Quaternion.identity, transform.root);
            bullet.SetActive(true);
            Rigidbody2D bulletRig = bullet.GetComponent<Rigidbody2D>();
            Vector2 targetPos = (Vector2)localPos + new Vector2(xPos * i, 0);
            targetPos.y = localPos.y;
            bullet.GetComponent<STObjectDealDamage>().UpdateDamage(m_bulletDamage);

            Vector2 finalVelocity = Blobcreate.ProjectileToolkit.Projectile.VelocityByHeight(bullet.transform.position, targetPos, m_maxBulletHeight - (m_densityBullet - i) / 3f);
            bulletRig.AddForce(finalVelocity * bulletRig.mass, ForceMode2D.Impulse);
            bullet.GetComponent<STEnemyBullet>().Init(Vector2.zero, 0);
            bulletRig.gravityScale = 1;

            // right
            bullet = Instantiate(m_objectBullet, localPos, Quaternion.identity, transform.root);
            bullet.SetActive(true);
            bulletRig = bullet.GetComponent<Rigidbody2D>();
            targetPos = (Vector2)localPos - new Vector2(xPos * i, 0);
            targetPos.y = localPos.y;
            bullet.GetComponent<STObjectDealDamage>().UpdateDamage(m_bulletDamage);

            finalVelocity = Blobcreate.ProjectileToolkit.Projectile.VelocityByHeight(bullet.transform.position, targetPos, m_maxBulletHeight - (m_densityBullet - i) / 3f);
            bulletRig.AddForce(finalVelocity * bulletRig.mass, ForceMode2D.Impulse);
            bullet.GetComponent<STEnemyBullet>().Init(Vector2.zero, 0);
            bulletRig.gravityScale = 1;
        }
    }

    private void MoveTo(Vector2? des, float moveSpeedRatio = 1)
    {
        m_destination = des;
        if (m_destination != null)
        {
            m_moveSpeed = m_baseSpeed * moveSpeedRatio;
            m_moveDirection = (m_destination.Value - (Vector2)transform.position).normalized;
            UpdateDirection(transform.position.x > des.Value.x ? 1 : -1);
        }
    }

    private void StopMove()
    {
        MoveTo(null);
        myRigidbody.velocity = Vector2.zero;
    }

    private void UpdateDirection(int direction)
    {
        m_direction = direction;
        transform.localScale = new Vector3(m_baseScale.x * m_direction, m_baseScale.y, 1);
    }

    private Vector2 FindWallMove()
    {
        Vector2 des = transform.position;
        Vector2 origin = (Vector2)transform.position + new Vector2(0, 1);
        RaycastHit2D hit2D = Physics2D.Raycast(origin, Vector2.right * -m_direction, 100f, m_layerMask);
        if (hit2D.collider == null)
            des = (Vector2)transform.position + new Vector2(20f * -m_direction, 0);
        else
        {
            des = hit2D.point;
            des.x += m_direction * 2f;
            des.y = transform.position.y;
        }
        return des;
    }

    private List<Vector2> GetRandomPointInsideAreaSummon(int minQuantity, int maxQuantity)
    {
        List<Vector2> spawnPosition = new List<Vector2>();
        int total = Random.Range(minQuantity, maxQuantity);
        for (int i = 0; i < total; i++)
        {
            Vector2 rand = GetRandomPointInsideCollider(m_areaSummon);
            spawnPosition.Add(rand);
        }
        return spawnPosition;
    }

    private Vector3 GetRandomPointInsideCollider(BoxCollider2D boxCollider)
    {
        Vector2 extents = boxCollider.size / 2f;
        Vector2 point = new Vector2(
            Random.Range(-extents.x, extents.x),
            Random.Range(-extents.y, extents.y));
        return boxCollider.transform.TransformPoint(point);
    }

    private bool IsObstacle(Vector2 origin, Vector2 size)
    {
        return Physics2D.OverlapBox(origin, size, 0, m_layerMask);
    }
    // ------------------------------ event ---------------------------- //
    void OnAnimStart(TrackEntry trackEntry)
    {
        if (isDead)
            return;
        if (trackEntry.Animation.Name != ANIM_RUN)
            UpdateDirection(transform.position.x > player.transform.position.x ? 1 : -1);

        switch (trackEntry.Animation.Name)
        {
            case ANIM_APPEAR:
            case ANIM_IDLE:
                m_bodyState = BodyState.Idle;
                break;
            case ANIM_ATTACK1:
                m_bodyState = BodyState.Attack;
                break;
            case ANIM_ATTACK2_RISE_UP:
                m_bodyState = BodyState.Idle;
                break;
            case ANIM_ATTACK3:
                m_bodyState = BodyState.Summon;
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
                StartCoroutine(IScheduleSecondSkill());
                break;
            case ANIM_ATTACK2_BURROWED_DOWN:
                StartCoroutine(IFollowTargetSecondSkill());
                break;
            case ANIM_ATTACK2_RISE_UP:
                StartCoroutine(IScheduleThirdSkill());
                break;
            case ANIM_ATTACK3:
                StartCoroutine(IWaitMove());
                break;
        }
    }

    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (isDead)
            return;
        UpdateDirection(transform.position.x > player.transform.position.x ? 1 : -1);
        switch (trackEntry.Animation.Name)
        {
            case ANIM_APPEAR:
                switch (e.Data.Name)
                {
                    case EVENT_GROWL:
                        SoundManager.PlaySound3D(m_appearVoice, 50f, false, transform.position);
                        GameController.ShakeCameraLoop(1f);
                        break;
                    case EVENT_SHAKE:
                        SoundManager.PlaySound3D(m_appearSlam, 10f, false, transform.position);
                        GameController.VibrateCustom(new Vector3(0.3f, 0.3f), 0.5f);
                        break;
                }
                break;
            case ANIM_ATTACK1:
                if (e.Data.Name != EVENT_ATTACK)
                    return;
                GameController.VibrateCustom(new Vector3(0.3f, 0.3f), 0.5f);
                ThrowStone();
                break;
            case ANIM_ATTACK2_BURROWED_DOWN:
                if (e.Data.Name != EVENT_DONE)
                    return;
                GameController.ShakeCamera();
                ColliderBody(false);
                break;
            case ANIM_ATTACK2_RISE_UP:
                if (e.Data.Name != EVENT_ATTACK)
                    return;
                GameController.VibrateCustom(new Vector3(0.3f, 0.3f), 0.5f);
                ColliderBody(true);
                if (isNextPhase)
                {
                    RiseUpThrowStone(m_densityStoneRiseUp);
                }
                break;
            case ANIM_ATTACK3:
                if (e.Data.Name != EVENT_ATTACK)
                    return;
                GameController.VibrateCustom(new Vector3(0.3f, 0.3f), 0.5f);
                SoundManager.PlaySound3D(m_meteorCallSlam, 20, false, transform.position);
                RainStone();
                SpawnMonster();
                break;
        }
    }

    private void OnMonsterDead(GameObject monster)
    {
        if (isDead)
            return;
        if (!m_currentMonsters.Contains(monster))
            return;
        m_currentMonsters.Remove(monster);
    }
}