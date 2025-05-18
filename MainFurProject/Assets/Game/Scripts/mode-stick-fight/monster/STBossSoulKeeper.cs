using DG.Tweening;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class STBossSoulKeeper : STObjectBoss
{
    private const float M_TIME_ZOOM_SHIELD = 0.5f;

    private enum BodyState
    {
        Idle,
        Attack,
        Summon,
        Defense,
        Stun
    }
    private const string ANIM_APPEAR = "xuathien";
    private const string ANIM_IDLE = "idle";
    private const string ANIM_STUN = "stun";
    private const string ANIM_ATTACK1 = "skill1";
    private const string ANIM_ATTACK2 = "skill2";
    private const string ANIM_ATTACK3 = "skill3";
    private const string ANIM_ATTACK4 = "skill4";

    private const string ANIM_DEAD = "dead";
    private const string ANIM_HIT = "hit";

    private const string EVENT_ATTACK = "attack";
    private const string EVENT_SHAKE = "shake";

    [SerializeField] private NextPhaseBoss[] m_skillSpawn;
    [Space(10)]

    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_bulletDamage = 10;
    [SerializeField] private float m_bulletSpeed = 8f;
    [SerializeField] private float m_timeGrowUpBoneFly = 3f;
    [SerializeField] private float m_timeDelayShootBullet = 0.2f;
    [SerializeField] private float m_timeDelayShieldDamage = 0.5f;
    [SerializeField] private int m_densitySoulCreep = 3;

    [SerializeField] private GameObject m_virtualCamera;
    [SerializeField] private Transform m_shotPoint;
    [SerializeField] private GameObject m_objectBullet;
    [SerializeField] private BoxCollider2D m_areaSummonSoul;
    [SerializeField] private BoxCollider2D m_areaSummonEntity;
    [SerializeField] private GameObject m_effectShield;
    [SerializeField] private GameObject m_effectBeatWall;
    [SerializeField] private GameObject m_childPortal;
    [SerializeField] private GameObject m_soulCreep;
    [SerializeField] private LayerMask m_layerMask;

    [Header("Sounds")]
    [SerializeField] private AudioClip m_audioAppear;
    [SerializeField] private AudioClip m_audioDie;
    [SerializeField] private AudioClip m_audioShoot;
    [SerializeField] private AudioClip m_audioShield;
    [SerializeField] private AudioClip m_audioSummonEntity;
    [SerializeField] private AudioClip m_audioSummonSoul;
    [SerializeField] private AudioClip m_hitAudio;

    private float m_remainHp { get => m_currentHP / maxHP; }

    private BodyState m_bodyState;
    private Vector2 m_baseScale;
    private Vector2 m_spineScale;
    private Vector2 m_shieldScale;
    private int m_currentPhase;
    private bool m_immortal;
    private bool m_hasNextPhase;
    private bool m_hasShoot;
    private List<GameObject> m_portals;
    private List<GameObject> m_currentMonsters;
    private List<GameObject> m_currentSouls;
    private List<Vector2> m_cacheSpawnPosition;
    private GameObject m_body;


    public override void Awake()
    {
        base.Awake();

        m_body = transform.Find("body").gameObject;
        m_body.GetComponent<STObjectDealDamage>().UpdateDamage(m_bodyDamage);
        m_effectShield.GetComponent<STObjectDealDamageOverTime>().UpdateDamage(m_bodyDamage, m_timeDelayShieldDamage);
        m_portals = new List<GameObject>();
        m_currentMonsters = new List<GameObject>();
        m_currentSouls = new List<GameObject>();
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
        StopAllCoroutines();

        m_body.SetActive(false);
        m_effectShield.SetActive(false);
        SoundManager.PlaySound3D(m_audioDie, 50f, false, transform.position);

        foreach (GameObject monster in m_currentMonsters)
        {
            Destroy(monster);
        }
        foreach (GameObject soul in m_currentSouls)
        {
            Destroy(soul);
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
        ShowBeatWallEffect();
        if (m_immortal || isDead)
            return;
        base.OnHit(attackerInfor);
        GameController.UpdateBossHp((int)maxHP, (int)currentHP);
        SoundManager.PlaySound3D(m_hitAudio, 15f, false, transform.position);
        UpdatePhase();
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
        m_currentPhase = 0;
        m_immortal = false;
        m_hasNextPhase = false;
        m_hasShoot = false;
        m_baseScale = transform.localScale;
        m_spineScale = spine.transform.localScale;
        m_shieldScale = m_effectShield.transform.localScale;
        spine.AnimationState.Start += OnAnimStart;
        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;
        spine.gameObject.SetActive(false);

        GameController.monsterDeadEvent += OnMonsterDead;
    }

    public override void StartBoss()
    {
        SkillSpawnEntity();
    }

    // ------------------------------ timer ---------------------------- //
    private IEnumerator IVisible()
    {
        m_virtualCamera.SetActive(true);
        yield return new WaitForSeconds(1f);
        spine.transform.localScale = Vector3.zero;
        spine.gameObject.SetActive(true);
        spine.AnimationState.SetAnimation(0, ANIM_APPEAR, false);
        m_bodyState = BodyState.Idle;
        spine.transform.localScale = m_spineScale;
    }

    private IEnumerator IDelayStart()
    {
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        m_virtualCamera.SetActive(false);
        yield return new WaitForSeconds(1f);
        GameController.BossReady();
    }

    private IEnumerator IGrowUpBoneFly()
    {
        m_hasShoot = true;
        float duration = m_timeGrowUpBoneFly + 2f; // 2s spawn monsters
        yield return new WaitForSeconds(duration);
        if (isDead)
            yield break;
        for (int i = 0; i < m_currentSouls.Count; i++)
        {
            if (m_currentSouls[i] == null)
                continue;

            STMonsterSoulCreep monsterComp = m_currentSouls[i].GetComponent<STMonsterSoulCreep>();
            monsterComp.SendMessage("OnGrowUp", SendMessageOptions.DontRequireReceiver);
        }
        m_hasShoot = false;
    }

    private IEnumerator IStun()
    {
        Stun();
        float duration = m_skillSpawn[m_currentPhase].timeStun;
        Debug.Log("[DEBUG] - Time stun = " + duration);
        while (duration > 0)
        {
            yield return null;
            duration -= Time.deltaTime;
            if (isDead)
                break;
        }
        if (isDead)
            yield break;
        RefreshAllSkill();
    }

    private IEnumerator IDelayAttackBullet()
    {
        Idle();
        yield return new WaitForSeconds(m_timeDelayShootBullet);
        if (isDead) yield break;
        SkillAttackBullet();
    }

    private IEnumerator IDelayDestroyPortal()
    {
        yield return new WaitForSeconds(2.2f);
        foreach (GameObject portal in m_portals)
        {
            Destroy(portal);
        }
    }
    // ------------------------------ skill ---------------------------- //
    private void SkillSpawnEntity()
    {
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK3, false);
    }

    private void RefreshAllSkill()
    {
        if (!m_hasNextPhase)
        {
            SkillSpawnEntity();
            return;
        }
        m_hasNextPhase = false;
        SkillSpawnSoul();
    }

    private void SkillSpawnSoul()
    {
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK1, false);
    }

    private void SkillShield()
    {
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK2, false);
    }

    private void SkillAttackBullet()
    {
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK4, false);
    }

    private void Idle()
    {
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
    }

    private void Stun()
    {
        spine.AnimationState.SetAnimation(0, ANIM_STUN, true);
    }

    private void SpawnEntity()
    {
        m_currentMonsters.Clear();
        m_cacheSpawnPosition = GetRandomPointInsideAreaSummon(50, 100, m_areaSummonEntity);

        MonsterSummon[] monsters = m_skillSpawn[m_currentPhase].monsterSummon;

        if (monsters.Length < 1)
            return;

        List<int> choosenPos = new List<int>();
        int total = m_cacheSpawnPosition.Count;
        for (int i = 0; i < monsters.Length; i++)
        {
            int index = i;
            Vector2 spawnPos;
            int randPosIndex = 0;
            int temp = 0;
            do
            {
                if (temp > total)
                    break;
                randPosIndex = Random.Range(0, m_cacheSpawnPosition.Count);
                temp++;
            } while (choosenPos.Contains(randPosIndex) || IsObstacle(m_cacheSpawnPosition[randPosIndex], Vector2.one));
            choosenPos.Add(randPosIndex);
            spawnPos = m_cacheSpawnPosition[randPosIndex];
            if (monsters[index].type == MonsterActionType.Ground)
            {
                RaycastHit2D hit = Physics2D.Raycast(spawnPos, Vector2.down, 10, m_layerMask);
                if (hit.collider != null)
                    spawnPos.y = hit.point.y;
            }

            GameObject monster = Instantiate(monsters[index].monster, spawnPos, Quaternion.identity, transform.parent);
            monster.SetActive(true);
            STObjectMonster monsterComp = monster.GetComponent<STObjectMonster>();
            monsterComp.PauseBehaviour();
            monster.SetActive(false);
            m_currentMonsters.Add(monster);

            Debug.LogFormat("[DEBUG] - monster = {0} - position = {1}", monster.name, spawnPos);

            monster.SetActive(true);
            Vector3 baseScale = monster.transform.localScale;
            monster.transform.localScale = Vector3.zero;

            monster.transform.DOScale(baseScale, 1).OnComplete(() =>
            {
                monsterComp.StartBehaviour();
            });
        }
    }

    private void SpawnSoul()
    {
        m_currentSouls.Clear();
        m_cacheSpawnPosition = GetRandomPointInsideAreaSummon(50, 100, m_areaSummonSoul);

        List<int> choosenPos = new List<int>();
        int total = m_cacheSpawnPosition.Count;
        for (int i = 0; i < m_densitySoulCreep; i++)
        {
            int index = i;
            Vector2 spawnPos;
            int randPosIndex = 0;
            int temp = 0;
            do
            {
                if (temp > total)
                    break;
                randPosIndex = Random.Range(0, m_cacheSpawnPosition.Count);
                temp++;
            } while (choosenPos.Contains(randPosIndex) || IsObstacle(m_cacheSpawnPosition[randPosIndex], Vector2.one));
            choosenPos.Add(randPosIndex);
            spawnPos = m_cacheSpawnPosition[randPosIndex];

            GameObject monster = Instantiate(m_soulCreep, spawnPos, Quaternion.identity, transform.parent);
            monster.SetActive(true);
            STMonsterSoulCreep monsterComp = monster.GetComponent<STMonsterSoulCreep>();
            monsterComp.PauseBehaviour();
            monster.SetActive(false);
            m_currentSouls.Add(monster);

            GameObject portal = Instantiate(m_childPortal, spawnPos, Quaternion.identity, transform.parent);
            portal.SetActive(true);
            portal.transform.localScale = Vector3.zero;
            m_portals.Add(portal);

            portal.transform.DOScale(Vector3.one, 1).OnComplete(() =>
            {
                monsterComp.StartBehaviour();
                monster.SetActive(true);
                Vector3 baseScale = monster.transform.localScale;
                monster.transform.localScale = Vector3.zero;
                monster.transform.DOScale(baseScale, 1);
            });
        }
        StartCoroutine(IDelayDestroyPortal());
    }

    private void ActiveShield(bool active)
    {
        m_immortal = active;
        m_effectShield.SetActive(active);
        if (active)
        {
            m_effectShield.transform.localScale = Vector3.zero;
            ZoomShield(m_shieldScale, M_TIME_ZOOM_SHIELD);
        }
    }

    private void ZoomShield(Vector3 scale, float duration)
    {
        m_effectShield.transform.DOScale(scale, duration);
    }

    private void Shoot()
    {
        Vector3 target = player.transform.position + Vector3.up;
        Vector2 direction = (target - m_shotPoint.transform.position).normalized;
        float angleOfBullet = Vector2.SignedAngle(Vector2.right, direction);
        GameObject bullet = Instantiate(m_objectBullet, m_shotPoint.transform.position, Quaternion.Euler(0, 0, angleOfBullet), transform.parent);
        bullet.SetActive(true);
        STEnemyBullet bulletComp = bullet.GetComponent<STEnemyBullet>();
        STObjectDealDamage bulletDealDamage = bullet.GetComponent<STObjectDealDamage>();
        bulletDealDamage.UpdateDamage(m_bulletDamage);
        bulletComp.Init(direction, m_bulletSpeed);
    }

    private List<Vector2> GetRandomPointInsideAreaSummon(int minQuantity, int maxQuantity, BoxCollider2D areaSummon)
    {
        List<Vector2> spawnPosition = new List<Vector2>();
        int total = Random.Range(minQuantity, maxQuantity);
        for (int i = 0; i < total; i++)
        {
            Vector2 rand = GetRandomPointInsideCollider(areaSummon);
            spawnPosition.Add(rand);
        }
        return spawnPosition;
    }

    private Vector3 GetRandomPointInsideCollider(BoxCollider2D boxCollider)
    {
        Vector2 extents = boxCollider.size / 2f;
        Vector2 point = new Vector2(
            Random.Range(-extents.x, extents.x),
            Random.Range(-extents.y, extents.y)) + boxCollider.offset;
        return boxCollider.transform.TransformPoint(point);
    }

    private bool IsObstacle(Vector2 origin, Vector2 size)
    {
        return Physics2D.OverlapBox(origin, size, 0, m_layerMask);
    }

    private void UpdatePhase()
    {
        if (m_currentPhase == m_skillSpawn.Length - 1)
            return;
        for (int i = m_skillSpawn.Length - 1; i >= 0; i--)
        {
            if (m_remainHp <= m_skillSpawn[i].ratioPhase && m_currentPhase < i)
            {
                m_hasNextPhase = true;
                m_currentPhase = i;
                return;
            }
        }
    }

    private void ShowBeatWallEffect()
    {
        if (!m_immortal)
            return;
        Instantiate(m_effectBeatWall, transform.position + Vector3.up, Quaternion.identity, transform.parent);
    }
    // ------------------------------ event ---------------------------- //
    void OnAnimStart(TrackEntry trackEntry)
    {
        if (isDead)
            return;

        switch (trackEntry.Animation.Name)
        {
            case ANIM_IDLE:
                m_bodyState = BodyState.Idle;
                break;
            case ANIM_ATTACK2:
                m_bodyState = BodyState.Defense;
                break;
            case ANIM_ATTACK1:
            case ANIM_ATTACK3:
                m_bodyState = BodyState.Summon;
                break;
            case ANIM_ATTACK4:
                m_bodyState = BodyState.Attack;
                break;
            case ANIM_STUN:
                m_bodyState = BodyState.Stun;
                break;
        }
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        if (isDead || trackEntry.Animation.Name == ANIM_IDLE)
            return;

        switch (trackEntry.Animation.Name)
        {
            case ANIM_APPEAR:
                StartCoroutine(IDelayStart());
                break;
            case ANIM_ATTACK1:
                SkillAttackBullet();
                StartCoroutine(IGrowUpBoneFly());
                break;
            case ANIM_ATTACK2:
                Idle();
                break;
            case ANIM_ATTACK3:
                SkillShield();
                break;
            case ANIM_ATTACK4:
                if (m_hasShoot)
                    StartCoroutine(IDelayAttackBullet());
                else
                    SkillSpawnEntity();
                break;
        }
    }

    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (isDead)
            return;

        switch (trackEntry.Animation.Name)
        {
            case ANIM_APPEAR:
                if (e.Data.Name != EVENT_SHAKE)
                    return;
                SoundManager.PlaySound3D(m_audioAppear, 50f, false, transform.position);
                GameController.ShakeCameraLoop(1f);
                break;
            case ANIM_ATTACK1:
                if (e.Data.Name != EVENT_ATTACK)
                    return;
                SoundManager.PlaySound3D(m_audioSummonSoul, 20f, false, transform.position);
                SpawnSoul();
                break;
            case ANIM_ATTACK2:
                if (e.Data.Name != EVENT_ATTACK)
                    return;
                SoundManager.PlaySound3D(m_audioShield, 30f, false, transform.position);
                ActiveShield(true);
                break;
            case ANIM_ATTACK3:
                if (e.Data.Name != EVENT_ATTACK)
                    return;
                SoundManager.PlaySound3D(m_audioSummonEntity, 30f, false, transform.position);
                SpawnEntity();
                break;
            case ANIM_ATTACK4:
                if (e.Data.Name != EVENT_ATTACK)
                    return;
                SoundManager.PlaySound3D(m_audioShoot, 20f, false, transform.position);
                Shoot();
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
        //
        if (m_currentMonsters.Count > 0)
            return;
        ActiveShield(false);
        StartCoroutine(IStun());
    }
}

public enum MonsterActionType
{
    Ground,
    Fly
}

[Serializable]
public class NextPhaseBoss
{
    public string phaseName;
    [Range(0f, 1f)] public float ratioPhase;
    public float timeStun = 4f;
    public MonsterSummon[] monsterSummon;
}

[Serializable]
public class MonsterSummon
{
    public MonsterActionType type;
    public GameObject monster;
}