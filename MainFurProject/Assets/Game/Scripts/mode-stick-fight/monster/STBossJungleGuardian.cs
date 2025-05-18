using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class STBossJungleGuardian : STObjectBoss
{
    private enum BodyState
    {
        Idle,
        Attack
    }

    private const int M_LIMIT_OBSTACLE = 20;
    private const float M_TIME_DEFAULT_IDLE = 1.6f;
    private const int M_COUNTER_ATTACK = 3;

    private const string ANIM_APPEAR = "xuat_hien";
    private const string ANIM_IDLE = "idle";
    private const string ANIM_ATTACK1 = "skill_1";
    private const string ANIM_ATTACK2 = "skill_2";
    private const string ANIM_ATTACK3 = "skill_3";
    private const string ANIM_HIT = "hit";

    private const string EVENT_ATTACK = "attack";

    [Range(0f, 1f)][SerializeField] private float m_nextPhaseRatio = 1f;
    [SerializeField] private int m_bodyDamage = 10;
    [SerializeField] private int m_bulletDamage = 10;
    [SerializeField] private int m_thunderDamage = 10;
    [SerializeField] private float m_speedBullet = 8f;
    [SerializeField] private float m_maxHeightCannonShoot = 8f;
    [SerializeField] private float m_timeDelayIdleSkill = 0f;
    [SerializeField] private float m_timeDelayBulletShoot = 0.2f;
    [SerializeField] private float m_timeDelayBulletCannon = 0.1f;
    [SerializeField] private float m_rainThunderTime = 0.1f;
    [SerializeField] private float m_densityBulletShoot = 3;
    [SerializeField] private float m_densityBulletCannon = 10;
    [SerializeField] private float m_densityThunder = 10;

    [SerializeField] private Transform m_headShoot;
    [SerializeField] private Transform m_backShoot;
    [SerializeField] private BoxCollider2D m_handCollider;
    [SerializeField] private BoxCollider2D m_thunderArea;
    [SerializeField] private BoxCollider2D m_rainBulletArea;
    [SerializeField] private GameObject m_virtualCamera;
    [SerializeField] private GameObject m_bullet;
    [SerializeField] private GameObject m_bulletCannon;
    [SerializeField] private GameObject m_thunderEffect;
    [SerializeField] private LayerMask m_layerObstacle;

    [Header("Sounds")]
    [SerializeField] private AudioClip m_audioAppear;
    [SerializeField] private AudioClip m_audioDie;
    [SerializeField] private AudioClip m_audioGiantSlam;
    [SerializeField] private AudioClip m_audioGiantVoice;
    [SerializeField] private AudioClip m_audioMutilBullet;
    [SerializeField] private AudioClip m_audioTripleBullet;
    [SerializeField] private AudioClip m_hitAudio;

    private bool isNextPhase { get => (m_currentHP / maxHP) <= m_nextPhaseRatio; }
    private Vector3 m_scaleBody { get => new Vector3(m_boneBody.ScaleX, m_boneBody.ScaleY, 1); }

    private BodyState m_bodyState;
    private Vector2 m_baseScale;
    private Vector2 m_spineScale;
    private int m_direction;
    private int m_attackIndex;
    private int m_faceDirection;
    private bool m_start;
    private bool m_scale = false;
    private Bone m_boneBody;
    private GameObject m_body;
    private CircleCollider2D m_collider;
    private BoxCollider2D m_colliderBody;
    private Collider2D[] m_arrayCollider2D;
    private List<Vector2> m_cacheSpawnPosition;

    public override void Awake()
    {
        base.Awake();

        m_body = transform.Find("body").gameObject;
        m_body.GetComponent<STObjectDealDamage>().UpdateDamage(m_bodyDamage);
        m_handCollider.GetComponent<STObjectDealDamage>().UpdateDamage(m_bodyDamage);
        m_colliderBody = m_body.GetComponent<BoxCollider2D>();
        m_collider = GetComponent<CircleCollider2D>();
        m_cacheSpawnPosition = new List<Vector2>();
        m_arrayCollider2D = new Collider2D[M_LIMIT_OBSTACLE];
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        spine.AnimationState.Start -= OnAnimStart;
        spine.AnimationState.Complete -= OnAnimComplete;
        spine.AnimationState.Event -= OnAnimEvent;
    }

    private void FixedUpdate()
    {
        if (!m_scale)
            return;
        m_body.transform.localScale = m_scaleBody;
        Bounds bounds = m_colliderBody.bounds;
        m_collider.offset = new Vector2(Mathf.Abs(transform.position.x - bounds.center.x), Mathf.Abs(transform.position.y - bounds.center.y));
        m_collider.radius = bounds.extents.y;
    }

    public override void OnResumeAfterHit()
    {

    }

    public override void Dead()
    {
        base.Dead();
        SoundManager.PlaySound(m_audioDie, false);
        m_scale = false;
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
        m_boneBody = spine.skeleton.FindBone("9");
        m_direction = m_baseScale.x > 0 ? -1 : 1;
        m_faceDirection = m_direction;
        m_scale = false;
        m_attackIndex = 0;

        spine.AnimationState.Start += OnAnimStart;
        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;

        StartCoroutine(IDelayStopAnimAppear());
    }

    public override void StartBoss()
    {
        m_start = true;
        FirstSkill();
    }

    // ------------------------------ timer ---------------------------- //
    private IEnumerator IVisible()
    {
        m_virtualCamera.SetActive(true);
        yield return new WaitForSeconds(1f);
        SoundManager.PlaySound3D(m_audioAppear, 50f, false, transform.position);
        spine.AnimationState.SetAnimation(0, ANIM_APPEAR, false);
        spine.timeScale = 1f;
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

    private IEnumerator IDelayStopAnimAppear()
    {
        yield return null;
        yield return null;
        yield return null;
        spine.timeScale = 0f;
    }

    private IEnumerator IScheduleFirstSkill()
    {
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        float duration = isNextPhase ? m_timeDelayIdleSkill : M_TIME_DEFAULT_IDLE;
        yield return new WaitForSeconds(duration);
        FirstSkill();
    }

    private IEnumerator IScheduleSecondSkill()
    {
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        float duration = isNextPhase ? m_timeDelayIdleSkill : M_TIME_DEFAULT_IDLE;
        yield return new WaitForSeconds(duration);
        SecondSkill();
    }

    private IEnumerator IScheduleThirdSkill()
    {
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        float duration = m_timeDelayIdleSkill;
        yield return new WaitForSeconds(duration);
        ThirdSkill();
    }

    private IEnumerator ISpawnBulletShoot(float duration)
    {
        yield return new WaitForSeconds(duration);
        Vector3 target = player.transform.position + Vector3.up;
        Vector2 direction = (target - m_headShoot.transform.position).normalized;
        float angleOfBullet = Vector2.SignedAngle(Vector2.right, direction);
        GameObject bullet = Instantiate(m_bullet, m_headShoot.transform.position, Quaternion.Euler(0, 0, angleOfBullet), transform.parent);
        bullet.SetActive(true);
        STEnemyBullet bulletComp = bullet.GetComponent<STEnemyBullet>();
        STObjectDealDamage bulletDealDamage = bullet.GetComponent<STObjectDealDamage>();
        bulletDealDamage.UpdateDamage(m_bulletDamage);
        bulletComp.Init(direction, m_speedBullet);
    }

    private IEnumerator ISpawnThunder(float duration, Vector2 targetPos)
    {
        yield return new WaitForSeconds(duration);
        GameObject thunder = Instantiate(m_thunderEffect, targetPos, m_thunderEffect.transform.rotation, transform.root);
        thunder.SetActive(true);
        thunder.GetComponent<STObjectDealDamage>().UpdateDamage(m_thunderDamage);
    }

    private IEnumerator ISpawnBulletCannon(float duration, Vector2 targetPos)
    {
        yield return new WaitForSeconds(duration);
        Vector2 shootPoint = m_backShoot.transform.position;
        Vector2 direction = (targetPos - shootPoint).normalized;
        float angleOfBullet = Vector2.SignedAngle(Vector2.right, direction);
        GameObject bullet = Instantiate(m_bulletCannon, shootPoint, Quaternion.Euler(0, 0, angleOfBullet), transform.root);
        bullet.SetActive(true);
        Rigidbody2D bulletRig = bullet.GetComponent<Rigidbody2D>();
        bullet.GetComponent<STObjectDealDamage>().UpdateDamage(m_bulletDamage);

        Vector2 finalVelocity = Blobcreate.ProjectileToolkit.Projectile.VelocityByHeight(bullet.transform.position, targetPos, m_maxHeightCannonShoot);
        bulletRig.AddForce(finalVelocity * bulletRig.mass, ForceMode2D.Impulse);
        bullet.GetComponent<STEnemyBullet>().Init(Vector2.zero, 0);
        bulletRig.gravityScale = 1;
    }

    // ------------------------------ private method ---------------------------- //
    private void FirstSkill()
    {
        LookAtPlayer();
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK1, false);
    }

    private void SecondSkill()
    {
        LookAtPlayer();
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK2, false);
    }

    private void ThirdSkill()
    {
        LookAtPlayer();
        spine.AnimationState.SetAnimation(0, ANIM_ATTACK3, false);
    }

    private void Shoot()
    {
        for (int i = 0; i < m_densityBulletShoot; i++)
        {
            float duration = m_timeDelayBulletShoot * i;
            StartCoroutine(ISpawnBulletShoot(duration));
        }
    }

    private void CannonShoot()
    {
        m_cacheSpawnPosition = GetRandomPointInsideArea(50, 200, m_rainBulletArea);

        List<int> choosenPos = new List<int>();
        int total = m_cacheSpawnPosition.Count;
        for (int i = 0; i < m_densityBulletCannon; i++)
        {
            Vector2 targetPos;
            int randPosIndex = 0;
            int temp = 0;
            do
            {
                if (temp > total)
                    break;
                randPosIndex = Random.Range(0, total);
                temp++;
            } while (choosenPos.Contains(randPosIndex) || IsObstacle(m_cacheSpawnPosition[randPosIndex], Vector2.one));
            choosenPos.Add(randPosIndex);
            targetPos = m_cacheSpawnPosition[randPosIndex];
            float duration = m_timeDelayBulletCannon * i;
            StartCoroutine(ISpawnBulletCannon(duration, targetPos));
        }
    }

    private void RainThunder()
    {
        m_cacheSpawnPosition = GetRandomPointInsideArea(50, 200, m_thunderArea);

        float maxTime = 0f;
        List<int> choosenPos = new List<int>();
        int total = m_cacheSpawnPosition.Count;
        for (int i = 0; i < m_densityThunder; i++)
        {
            Vector2 targetPos;
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
            targetPos = m_cacheSpawnPosition[randPosIndex];

            float duration = Random.Range(0f, m_rainThunderTime);
            maxTime = (duration > maxTime) ? duration : maxTime;

            StartCoroutine(ISpawnThunder(duration, targetPos));
        }
    }

    private void ColliderBody(bool active)
    {
        m_body.SetActive(active);
        m_collider.enabled = active;
        m_handCollider.enabled = active;
    }

    private void LookAtPlayer()
    {
        m_direction = transform.position.x > player.transform.position.x ? -1 : 1;
        transform.localScale = new Vector3(m_baseScale.x * -m_direction * m_faceDirection, m_baseScale.y, 1);
    }

    private bool IsObstacle(Vector2 origin, Vector2 size)
    {
        m_arrayCollider2D = new Collider2D[M_LIMIT_OBSTACLE];
        return Physics2D.OverlapAreaNonAlloc(origin, size, m_arrayCollider2D, m_layerObstacle) > 0;
    }

    private List<Vector2> GetRandomPointInsideArea(int minQuantity, int maxQuantity, BoxCollider2D area)
    {
        List<Vector2> spawnPosition = new List<Vector2>();
        int total = Random.Range(minQuantity, maxQuantity);
        for (int i = 0; i < total; i++)
        {
            Vector2 rand = GetRandomPointInsideCollider(area);
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

    // ------------------------------ event ---------------------------- //
    void OnAnimStart(TrackEntry trackEntry)
    {
        if (isDead)
            return;

        switch (trackEntry.Animation.Name)
        {
            case ANIM_APPEAR:
            case ANIM_IDLE:
                m_bodyState = BodyState.Idle;
                break;
            case ANIM_ATTACK1:
                m_attackIndex++;
                m_bodyState = BodyState.Attack;
                break;
            case ANIM_ATTACK2:
                m_scale = true;
                SoundManager.PlaySound3D(m_audioGiantVoice, 30f, false, transform.position);
                m_bodyState = BodyState.Attack;
                break;
            case ANIM_ATTACK3:
                m_bodyState = BodyState.Attack;
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
                if (m_attackIndex >= M_COUNTER_ATTACK)
                {
                    m_attackIndex = 0;
                    if (isNextPhase)
                        StartCoroutine(IScheduleThirdSkill());
                    else
                        StartCoroutine(IScheduleSecondSkill());
                }
                else
                {
                    FirstSkill();
                }
                break;
            case ANIM_ATTACK2:
                m_scale = false;
                m_handCollider.enabled = false;
                m_body.transform.localScale = Vector3.one;
                StartCoroutine(IScheduleFirstSkill());
                break;
            case ANIM_ATTACK3:
                StartCoroutine(IScheduleSecondSkill());
                break;
        }
    }

    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (isDead)
            return;

        switch (trackEntry.Animation.Name)
        {
            case ANIM_ATTACK1:
                if (e.Data.Name != EVENT_ATTACK)
                    return;
                SoundManager.PlaySound3D(m_audioTripleBullet, 50f, false, transform.position);
                Shoot();
                break;
            case ANIM_ATTACK2:
                if (e.Data.Name != EVENT_ATTACK)
                    return;
                m_handCollider.enabled = true;
                SoundManager.PlaySound3D(m_audioGiantSlam, 30f, false, transform.position);
                GameController.VibrateCustom(new Vector3(0.3f, 0.3f), 0.5f);
                RainThunder();
                break;
            case ANIM_ATTACK3:
                if (e.Data.Name != EVENT_ATTACK)
                    return;
                SoundManager.PlaySound3D(m_audioMutilBullet, 30f, false, transform.position);
                CannonShoot();
                break;
        }
    }
}
