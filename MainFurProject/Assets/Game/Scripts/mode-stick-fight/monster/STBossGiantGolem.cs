using UnityEngine;
using System.Collections;
using Spine;
using Spine.Unity;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class STBossGiantGolem : STObjectBoss
{
    [SerializeField] private float m_firstSkillReadyTime = 1.5f;
    [SerializeField] private float m_firstSkillAttackSpeed = 6f;
    [SerializeField] private float m_bulletSpeed = 6f;
    [SerializeField] private float m_thirdSkillSpeed = 10f;
    [SerializeField] private int m_handDamage = 10;
    [SerializeField] private int m_bulletDamge = 10;
    [SerializeField] private int m_bulletAmount = 6;
    [SerializeField] private float m_bigBulletSpeed = 6f;
    [SerializeField] private int m_bigBulletDamage;
    [SerializeField] private float m_stunTime = 5f;
    
    // references
    [SerializeField] private GameObject m_virtualCamera;
    [SerializeField] private Transform m_shotPoint;
    [SerializeField] private GameObject m_bullet;
    [SerializeField] private GameObject m_bigBullet;
    [SerializeField] private STBossGiantGolemHand m_leftHand;
    [SerializeField] private STBossGiantGolemHand m_rightHand;

    [SerializeField] private AudioClip m_audioSlamImpact;
    [SerializeField] private AudioClip m_audioSlamVoice;
    [SerializeField] private AudioClip m_audioPunch;
    [SerializeField] private AudioClip m_audioPunchVoice;
    [SerializeField] private AudioClip m_audioShotVoice;
    [SerializeField] private AudioClip m_audioAppear;
    
    private Vector3 m_startLeftHandPos;
    private Vector3 m_startRightHandPos;
    private bool m_start;
    private int m_firstSkillCounter;
    private float m_minManaFractionToSecondStage = 0.5f;
    private bool m_isStun;
    private int m_secondStageSkillPlayed;
    private bool m_swipeLeftHand;
    private Vector3 m_slamHandPos;

    private const string ANIM_IDLE = "idle";
    private const string ANIM_SKILL_1_1 = "skill_1_1";
    private const string ANIM_SKILL_1_2 = "skill_1_2";
    private const string ANIM_SKILL_1_3_R = "skill_1_3_R";
    private const string ANIM_SKILL_1_3_L = "skill_1_3_L";
    private const string ANIM_SKILL_1_3_HAND = "skill_1_3";
    private const string ANIM_SKILL_1_4 = "skill_1_4";
    private const string ANIM_SKILL_2 = "skill_2";
    private const string ANIM_SKILL_3_READY = "skill_3_ready";
    private const string ANIM_SKILL_3_R = "skill_3_at_R";
    private const string ANIM_SKILL_3_L = "skill_3_at_L";
    private const string ANIM_SKILL_3_HAND = "skill_3";
    private const string ANIM_SKILL_4 = "skill_4";
    private const string ANIM_STUN = "stun";
    private const string ANIM_APPEAR = "appear";
    
    public Vector3 topLeftHandPos;
    public Vector3 topRightHandPos;
    public Vector3 leftHandPos;
    public Vector3 rightHandPos;
    
    public bool SecondStage{get => (m_currentHP / maxHP) <= m_minManaFractionToSecondStage;}

    public override void Awake()
    {
        base.Awake();
        m_startLeftHandPos = m_leftHand.transform.position;
        m_startRightHandPos = m_rightHand.transform.position;
        SetupAllDamager();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();

        spine.AnimationState.Complete -= OnAnimComplete;
        spine.AnimationState.Event -= OnAnimEvent;
    }
    public override void OnResumeAfterHit()
    {
        
    }
    public override void Dead()
    {
        base.Dead();
        StopAllCoroutines();
        m_leftHand.Dead(deadAnimation);
        m_rightHand.Dead(deadAnimation);
        m_leftHand.ActivateCollider(false);
        m_rightHand.ActivateCollider(false);
    }
    public override void OnDeadFinish()
    {
        base.OnDeadFinish();
        Destroy(gameObject);
    }
    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        base.OnHit(attackerInfor);
        // spine.AnimationState.SetAnimation(1, "hit_dau", false);
        GameController.UpdateBossHp((int)maxHP, (int)currentHP);
    }
    public override void OnAppear()
    {
        StartCoroutine(IVisible());
    }
    public override void Init()
    {
        visibleCollider.radius = visibleRange;

        Color tempColor = Color.white;
        tempColor.a = 0;
        spine.skeleton.SetColor(tempColor);
        m_leftHand.SetColor(tempColor);
        m_rightHand.SetColor(tempColor);

        spine.AnimationState.Complete += OnAnimComplete;
        spine.AnimationState.Event += OnAnimEvent;
    }
    public override void StartBoss()
    {
        m_start = true;
        StartCoroutine(ScheduleFirstSkill());
    }

    IEnumerator IVisible()
    {
        SoundManager.PlaySound(m_audioPunchVoice, false);
        m_virtualCamera.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(IDelayStart());
    }
    IEnumerator IDelayStart()
    {
        SoundManager.PlaySound(m_audioAppear, false);
        TrackEntry entry = spine.AnimationState.SetAnimation(0, ANIM_APPEAR, false);
        StartCoroutine(IShakeCamera(entry.AnimationEnd));
        yield return new WaitForSeconds(0.2f);
        spine.skeleton.SetColor(Color.white);
        m_leftHand.SetColor(Color.white);
        m_rightHand.SetColor(Color.white);
        m_leftHand.SetAnimation(ANIM_APPEAR, false);
        m_rightHand.SetAnimation(ANIM_APPEAR, false);
        yield return new WaitForSeconds(entry.AnimationEnd);
        m_leftHand.SetAnimation(ANIM_IDLE, true);
        m_rightHand.SetAnimation(ANIM_IDLE, true);
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        m_virtualCamera.SetActive(false);
        yield return new WaitForSeconds(2f);
        GameController.BossReady();
    }

    void OnAnimComplete(TrackEntry trackEntry)
    {
        if(isDead)
            return;
        switch(trackEntry.Animation.Name)
        {
            case ANIM_SKILL_1_1:
                spine.AnimationState.SetAnimation(0, ANIM_SKILL_1_2, true);
                m_leftHand.SetAnimation(ANIM_SKILL_1_2, true);
                m_rightHand.SetAnimation(ANIM_SKILL_1_2, true);
                m_slamHandPos = new Vector3(player.transform.position.x, m_startLeftHandPos.y, 0);
                m_leftHand.ActivateCollider(true);
                m_rightHand.ActivateCollider(true);
                break;
            case ANIM_SKILL_1_3_L:
            case ANIM_SKILL_1_3_R:
                m_firstSkillCounter++;
                if (m_firstSkillCounter >= 2)
                {
                    m_firstSkillCounter = 0;
                    EndFirstSkill();
                }
                else
                {
                    StartCoroutine(ScheduleFirstSkill());
                }
                m_leftHand.ActivateCollider(false);
                m_rightHand.ActivateCollider(false);
                break;
            case ANIM_SKILL_1_4:
                if (SecondStage)
                {
                    m_secondStageSkillPlayed++;
                    if (m_secondStageSkillPlayed >= 3)
                    {
                        m_secondStageSkillPlayed = 0;
                        FourthSkill();
                    }
                    else
                    {
                        int random = Random.Range(0, 100);
                        if (random < 20)
                            StartCoroutine(ScheduleFirstSkill());
                        else
                            ThirdSkill();
                    }
                }
                else
                    SecondSkill();
                break;
            case ANIM_SKILL_2:
                StartCoroutine(IStun());
                break;
            case ANIM_SKILL_3_READY:
                m_swipeLeftHand = Random.Range(0, 100) < 50;
                StartSwipe();
                break;
            case ANIM_SKILL_3_L:
            case ANIM_SKILL_3_R:
                m_secondStageSkillPlayed++;
                if (m_secondStageSkillPlayed >= 3)
                {
                    m_secondStageSkillPlayed = 0;
                    FourthSkill();
                }
                else
                {
                    int random = Random.Range(0, 100);
                    if (random < 20)
                        StartCoroutine(ScheduleFirstSkill());
                    else
                        ThirdSkill();
                }
                  break;  
            case ANIM_SKILL_4:
                StartCoroutine(IStun());
                break;
        }
    }
    void OnAnimEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if(isDead)
            return;
        switch(trackEntry.Animation.Name)
        {
            case ANIM_SKILL_2:
                ShotCircular();
                break;
            case ANIM_SKILL_4:
                ShotCircular();
                ShootBigBullet();
                break;
            case ANIM_SKILL_3_L:
            case ANIM_SKILL_3_R:
                SwipeHand();
                break;
        }
    }
    
    //
    IEnumerator IShakeCamera(float shakeTime)
    {
        float shakeTimer = 0;
        while (shakeTimer < shakeTime)
        {
            GameController.ShakeCamera();
            yield return null;
            shakeTimer += Time.deltaTime;
        }
    }
    
    // skill 1
    IEnumerator ScheduleFirstSkill()
    {
        FirstSkill();
        yield return new WaitForSeconds(m_firstSkillReadyTime);
        bool playerOnLeft = player.transform.position.x < transform.position.x;
        if(playerOnLeft)
            SlamLeftHand();
        else
            SlamRightHand();
    }
    void FirstSkill()
    {
        SoundManager.PlaySound(m_audioSlamVoice, false);
        spine.AnimationState.SetAnimation(0, ANIM_SKILL_1_1, false);
        m_leftHand.SetAnimation(ANIM_SKILL_1_1, false);
        m_rightHand.SetAnimation(ANIM_SKILL_1_1, false);
        float animDuration = spine.Skeleton.Data.FindAnimation(ANIM_SKILL_1_1).Duration;
        m_leftHand.MoveTo(topLeftHandPos, animDuration);
        m_rightHand.MoveTo(topRightHandPos, animDuration);
    }
    void SlamLeftHand()
    {
        spine.AnimationState.SetAnimation(0, ANIM_SKILL_1_3_L, false);
        m_leftHand.SetAnimation(ANIM_SKILL_1_3_HAND, false);
        float animDuration = spine.Skeleton.Data.FindAnimation(ANIM_SKILL_1_3_L).Duration;
        m_leftHand.MoveTo(m_slamHandPos, animDuration, () => PunchGround());
    }
    void SlamRightHand()
    {
        spine.AnimationState.SetAnimation(0, ANIM_SKILL_1_3_R, false);
        m_rightHand.SetAnimation(ANIM_SKILL_1_3_HAND, false);
        float animDuration = spine.Skeleton.Data.FindAnimation(ANIM_SKILL_1_3_R).Duration;
        m_rightHand.MoveTo(m_slamHandPos, animDuration, () => PunchGround());
    }
    void PunchGround()
    {
        GameController.ShakeCamera();
        SoundManager.PlaySound(m_audioSlamImpact, false);
    }
    void EndFirstSkill()
    {
        spine.AnimationState.SetAnimation(0, ANIM_SKILL_1_4, false);
        m_leftHand.SetAnimation(ANIM_SKILL_1_4, false);
        m_rightHand.SetAnimation(ANIM_SKILL_1_4, false);
        float animDuration = spine.Skeleton.Data.FindAnimation(ANIM_SKILL_1_4).Duration;
        m_leftHand.MoveTo(m_startLeftHandPos, animDuration, null, () => ShakeCameraWeak());
        m_rightHand.MoveTo(m_startRightHandPos, animDuration, null, () => ShakeCameraWeak());
    }
    void ShakeCameraWeak()
    {
        GameController.ShakeCameraWeak();   
        m_leftHand.ActivateCollider(false);
        m_rightHand.ActivateCollider(false);
    }
    
    // skill 2
    void SecondSkill()
    {
        spine.AnimationState.SetAnimation(0, ANIM_SKILL_2, false);
        m_leftHand.MoveTo(m_startLeftHandPos, 0.5f);
        m_rightHand.MoveTo(m_startRightHandPos, 0.5f);
        m_leftHand.SetAnimation(ANIM_IDLE, true);
        m_rightHand.SetAnimation(ANIM_IDLE, true);
    }
    void ShotCircular()
    {
        SoundManager.PlaySound(m_audioShotVoice, false);
        int randAngle = Random.Range(0, 180);
        float anglePerShot = 360 / m_bulletAmount;
        for(int i = 0; i < m_bulletAmount; i++)
        {
            float angle = i * anglePerShot + randAngle;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 direction = rotation * Vector3.down;
            Shoot(m_shotPoint.position, direction.normalized);     
        }
    }
    void Shoot(Vector3 startPos, Vector3 direction)
    {
        GameObject bullet = Instantiate(m_bullet, startPos, Quaternion.identity, transform.parent);
        bullet.SetActive(true);
        bullet.GetComponent<STEnemyBullet>().Init(direction, m_bulletSpeed);
        // SoundManager.PlaySound3D(m_audioShot, 10, false, transform.position);
    }
    void ShootBigBullet()
    {
        Vector3 direction = (player.transform.position - m_shotPoint.position).normalized;
        GameObject bullet = Instantiate(m_bigBullet, m_shotPoint.position, Quaternion.identity, transform.parent);
        bullet.SetActive(true);
        bullet.GetComponent<STEnemyBullet>().Init(direction, m_bigBulletSpeed);
        // SoundManager.PlaySound3D(m_audioShot, 10, false, transform.position);
    }
    
    // Skill 3
    void ThirdSkill()
    {
        spine.AnimationState.SetAnimation(0, ANIM_SKILL_3_READY, false);
        m_leftHand.SetAnimation(ANIM_SKILL_3_READY, false);
        m_rightHand.SetAnimation(ANIM_SKILL_3_READY, false);
        float animDuration = spine.Skeleton.Data.FindAnimation(ANIM_SKILL_3_READY).Duration;
        m_leftHand.MoveTo(leftHandPos, animDuration);
        m_rightHand.MoveTo(rightHandPos, animDuration);
    }
    void StartSwipe()
    {
        m_leftHand.ActivateCollider(true);
        m_rightHand.ActivateCollider(true);
        SoundManager.PlaySound(m_audioPunchVoice, false);
        string animToPlay = m_swipeLeftHand ? ANIM_SKILL_3_L : ANIM_SKILL_3_R;
        spine.AnimationState.SetAnimation(0, animToPlay, false);
    }
    void SwipeHand()
    {
        SoundManager.PlaySound(m_audioPunch, false);
        Vector3 startPos = m_swipeLeftHand ? leftHandPos : rightHandPos;
        Vector3 endPos = m_swipeLeftHand ? rightHandPos : leftHandPos;
        STBossGiantGolemHand handUsing = m_swipeLeftHand ? m_leftHand : m_rightHand;

        handUsing.SetAnimation(ANIM_SKILL_3_HAND, true);
        handUsing.MoveTo(endPos, 1f, () => SwipeComplete(handUsing, startPos), () => GameController.ShakeCameraWeak());
    }
    void SwipeComplete(STBossGiantGolemHand hand, Vector3 startPos)
    {
        hand.transform.position = startPos;
        hand.SetAnimation(ANIM_IDLE, true);
        m_leftHand.ActivateCollider(false);
        m_rightHand.ActivateCollider(false);
    }
    // Skill 4
    void FourthSkill()
    {
        spine.AnimationState.SetAnimation(0, ANIM_SKILL_4, false);
        m_leftHand.SetAnimation(ANIM_SKILL_4, false);
        m_rightHand.SetAnimation(ANIM_SKILL_4, false);
        m_leftHand.MoveTo(m_startLeftHandPos, 1f);
        m_rightHand.MoveTo(m_startRightHandPos, 1f);
    }
    // Stun
    IEnumerator IStun()
    {
        if(m_isStun)
            yield break;
        m_isStun = true;
        spine.AnimationState.SetAnimation(0, ANIM_STUN, true);
        m_leftHand.SetAnimation(ANIM_STUN, true);
        m_leftHand.SetAnimation(ANIM_STUN, true);
        m_leftHand.ActivateCollider(false);
        m_rightHand.ActivateCollider(false);

        yield return new WaitForSeconds(m_stunTime);
        
        spine.AnimationState.SetAnimation(0, ANIM_IDLE, true);
        m_leftHand.SetAnimation(ANIM_IDLE, true);
        m_leftHand.SetAnimation(ANIM_IDLE, true);
        
        StartCoroutine(ScheduleFirstSkill());
        
        m_isStun = false;
    }
    
    void SetupAllDamager()
    {
        m_leftHand.GetComponentInChildren<STObjectDealDamage>().UpdateDamage(m_handDamage);
        m_rightHand.GetComponentInChildren<STObjectDealDamage>().UpdateDamage(m_handDamage);
        m_bullet.GetComponent<STObjectDealDamage>().UpdateDamage(m_bulletDamge);
        m_bullet.GetComponent<STObjectDealDamage>().UpdateDamage(m_bigBulletDamage);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Handles.Label(leftHandPos + Vector3.up, "LEFT");
        Gizmos.DrawWireSphere(leftHandPos, 0.5f);
        Handles.Label(rightHandPos + Vector3.up, "RIGHT");
        Gizmos.DrawWireSphere(rightHandPos, 0.5f);
        
        Gizmos.color = Color.green;
        Handles.Label(topLeftHandPos + Vector3.up, "TOP LEFT");
        Gizmos.DrawWireSphere(topLeftHandPos, 0.5f);
        Handles.Label(topRightHandPos + Vector3.up, "TOP RIGHT");
        Gizmos.DrawWireSphere(topRightHandPos, 0.5f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center.position, visibleRange);
        visibleCollider.radius = visibleRange;
    }
#endif
}
