using System;
using System.Collections;
using DG.Tweening;
using Spine;
using Spine.Unity;
using Spine.Unity.Examples;
using UnityEngine;
using UnityEngine.Serialization;
using Event = Spine.Event;
using Random = UnityEngine.Random;

public class STPlayerView : MonoBehaviour
{
    private const float M_HIT_TIME_FREEZE = 1f / 60 * 20;

    [Min(0)] 
    [SerializeField] private float m_slowMotionFightTime = 0.2f;
    [Min(0)]
    [SerializeField] private float m_slowMotionDashTime = 0.5f;
    [SerializeField] private Vector2 m_dashShakeForce = Vector2.zero;
    [SerializeField] private bool m_activeSoundFight;
    [HideInInspector]
    [SerializeField] private Transform m_hitPosition;
    [HideInInspector]
    [SerializeField] private SkeletonAnimation m_spine;
    [HideInInspector]
    [SerializeField] private GameObject m_fxJump;
    [HideInInspector]
    [SerializeField] private GameObject m_fxHit;
    [HideInInspector]
    [SerializeField] private GameObject m_fxHitWall;
    [HideInInspector]
    [SerializeField] private GameObject m_fxCritical;
    [HideInInspector]
    [SerializeField] private GameObject m_fxHeal;
    [HideInInspector]
    [SerializeField] private GameObject m_fxDash;
    [HideInInspector]
    [SerializeField] private GameObject m_fxExplosion;
    [HideInInspector]
    [SerializeField] private SkeletonGhost m_ghost;
    [HideInInspector]
    [SerializeField] private GameObject m_magnet;
    [HideInInspector]
    [SerializeField] private Animator m_animator;
    [HideInInspector]
    [SerializeField] private AudioClip m_clipJumpGround;
    [HideInInspector]
    [SerializeField] private AudioClip m_clipJumpAir;
    [HideInInspector]
    [SerializeField] private AudioClip m_clipFightWall;
    [HideInInspector]
    [SerializeField] private AudioClip m_clipSlide;
    [HideInInspector]
    [SerializeField] private AudioClip m_clipDash;
    [HideInInspector]
    [SerializeField] private AudioClip m_clipDead;
    [HideInInspector]
    [SerializeField] private AudioClip m_clipFinish;
    [HideInInspector]
    [SerializeField] private AudioClip[] m_clipFootSteps;
    [HideInInspector]
    [SerializeField] private AudioClip m_clipHitEffect;
    [HideInInspector]
    [SerializeField] private AudioClip m_clipHitVoice;

    private DamageDealerInfo m_hitCache; //cache for enemy deal damage
    private Coroutine m_timerDash;
    private Coroutine m_timerDashSlowMotion;
    
    private AudioClip m_soundFootStep;
    private GameObject m_soundSlide;

    private STPlayerStats m_stats;
    private Weapon m_currentWeapon => m_stats.weapon;

    private void Awake()
    {
        m_stats = GetComponent<STPlayerStats>();
        //
        STGameController.updatePlayerHpEvent += OnUpdateHp;
        STGameController.hitPlayerEvent += OnHit;
        GameController.changeSkinEvent += OnChangeSkin;
        GameController.activeMagnetEvent += OnActiveMagnet;
        GameController.buffHeartEvent += OnBuffHeart;
        GameController.loadSavePointEvent += OnRevive;
        MainController.equipWeaponEvent += OnEquipWeapon;
    }

    void Start()
    {
        m_spine.SetMixSkin(MainModel.CurrentSkin, MainModel.CurrentWeapon);
        m_spine.AnimationState.Event += OnAnimationEvent;
        m_spine.AnimationState.Complete += OnAnimationComplete;
    }

    private void OnDestroy()
    {
        STGameController.updatePlayerHpEvent -= OnUpdateHp;
        STGameController.hitPlayerEvent -= OnHit;
        GameController.changeSkinEvent -= OnChangeSkin;
        GameController.activeMagnetEvent -= OnActiveMagnet; 
        GameController.buffHeartEvent -= OnBuffHeart;
        GameController.loadSavePointEvent -= OnRevive;
        MainController.equipWeaponEvent -= OnEquipWeapon;
    }
    /////////////////////////////////////////////GAME EVENT HANDLER//////////////////////////////////
    void OnChangeSkin(string skin)
    {
        GameObject go = Instantiate(m_fxExplosion, transform.parent, false);
        go.transform.position = transform.position;
        m_spine.SetMixSkin(skin, MainModel.currentWeapon);
    }

    void OnUpdateHp(int lastValue, int currentValue)
    {
        if(currentValue >= lastValue)
        {
            GameObject fxHeal = Instantiate(m_fxHeal, transform);
            fxHeal.transform.localPosition = Vector3.zero;
            fxHeal.SetActive(true);
        }
    }

    void OnHit(DamageDealerInfo damage)
    {
        m_hitCache = damage;
    }

    void OnActiveMagnet()
    {
        m_magnet.SetActive(true);
    }
    
    void OnBuffHeart(Vector2 savePoint)
    {
        Color tempColor = Color.white;
        tempColor.a = 0;
        m_spine.Skeleton.SetColor(tempColor);
        transform.position = savePoint;
    }
    
    void OnRevive(Vector2? position)
    {
        StartCoroutine(IRevive());
    }

    void OnEquipWeapon(string skin)
    {
        WeaponConfig config = ConfigLoader.instance.config.GetWeaponBySkinName(skin);
        if(config == null)
            return;
        m_spine.SetMixSkin(MainModel.CurrentSkin, skin);
        //
        GameObject newWeapon = Instantiate(config.weapon.gameObject, transform);
        newWeapon.transform.SetLocalPositionAndRotation(Vector2.zero, Quaternion.identity);
        m_stats.UpdateWeapon(newWeapon);
        m_currentWeapon.strainEvent += OnStrain;
        m_currentWeapon.fightEvent += OnFight;
        m_currentWeapon.strainFullEvent += OnStrainFull;
    }

    void OnStrain()
    {
        m_spine.AnimationState.SetAnimation(0, m_currentWeapon.strainAnimation, true);
    }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
    void OnFight()
    {
        TrackEntry entry = m_spine.AnimationState.SetAnimation(0, m_currentWeapon.fightAnimation, false);
        CancelStrain();
        entry.Complete += (t) =>
        {
            m_stats.fsm.CurrentState.Transition(STPlayerState.Other);
        };
    }

    void OnStrainFull()
    {
        if(!string.IsNullOrEmpty(m_currentWeapon.strainFullAnimation))
            m_spine.AnimationState.SetAnimation(10, m_currentWeapon.strainFullAnimation, false);
    }
    ///////////////////////////////////PRIVATE METHOD//////////////////////////////////
    void Dead()
    {
        StopAllCoroutines();
        m_spine.skeleton.SetColor(Color.white);
        m_fxDash.SetActive(false);
        m_ghost.ghostingEnabled = false;
        //
        m_timerDash = null;
        //create effect
        if (m_hitCache != null && m_hitCache.attacker != null)
        {
            GameObject go = Instantiate(m_fxHit, transform.parent, false);
            Vector3 mid = (m_hitCache.attacker.position + transform.position) / 2;
            go.transform.position = mid;
            Destroy(go, 3f);
        }
    }

    void CancelStrain()
    {
        m_spine.AnimationState.AddEmptyAnimation(10, 0.1f, 0);
    }
    //////////////////////////////////////////////////////////////////////////////////

    private void OnAnimationEvent(TrackEntry trackEntry, Event e)
    {
        switch (e.Data.Name)
        {
            case "attack":
                m_currentWeapon.StartHit();
                SendMessage("OnWeaponHit", SendMessageOptions.DontRequireReceiver);
                break;
            case "repeat":
                m_currentWeapon.EndHit();
                if (m_activeSoundFight)
                {
                    AudioClip[] clips = ConfigLoader.instance.config.GetSkin(MainModel.CurrentSkin).fightVoices;
                    SoundManager.PlaySound(clips[Random.Range(0, clips.Length)], false);
                }
                SendMessage("OnWeaponRepeat", SendMessageOptions.DontRequireReceiver);
                break;
            case "idle_freeze":
                StartCoroutine(IHit());
                break;
            case "foot_step":
                //SoundManager.PlaySound(m_soundFootStep, false);
                break;
            case "fx_attack":
                StartCoroutine(ISlowMotion());
                break;
            case "extra_attack":
                if(m_currentWeapon.skin == "w12")
                    m_currentWeapon.ExtraAttack();
                break;
            case "shake":
                GameController.ShakeCamera();
                break;
        }
    }
    
    private void OnAnimationComplete(TrackEntry trackentry)
    {
        switch (trackentry.Animation.Name)
        {
            case "dead":
                GameController.RestartLevel();
                break;
            case "win":
                GameController.CompleteLevel();
                break;
        }
    }
    //////////////////////////////////////////////////////////////////////////////////
    //call via send message
    void OnStateEnter(string state)
    {
        switch (state)
        {
            case "Hurt-1":
                GameController.ShakeCamera();
                SendMessage("OnHitStart", SendMessageOptions.DontRequireReceiver);
                break;
            case "Dash":
                SoundManager.PlaySound(m_clipDash, false);
                if(m_animator.GetBool("IsGround"))
                    m_fxDash.SetActive(true);
                if(m_timerDash != null)
                    StopCoroutine(m_timerDash);
                m_timerDash = StartCoroutine(IDash());
                break;
            case "Run":
                //m_soundFootStep = m_clipFootSteps[Random.Range(0, m_clipFootSteps.Length)];
                m_stats.isMove = true;
                break;
            case "Slide":
                if(m_soundSlide != null)
                    Destroy(m_soundSlide);
                m_soundSlide = SoundManager.PlaySound(m_clipSlide, true);
                SendMessage("OnSlideStart", SendMessageOptions.DontRequireReceiver);
                break;
            case "Dead":
                SoundManager.PlaySound(m_clipDead, false);
                Dead();
                break;
            case "Finish":
                SoundManager.PlaySound(m_clipFinish, false);
                break;
        }
    }

    //call via send message
    void OnStateExit(string state)
    {
        switch (state)
        {
            case "Fight":
                m_currentWeapon.EndHit();
                CancelStrain();
                break;
            case "Hurt-1":
                m_stats.fsm.CurrentState.Transition(STPlayerState.Other);
                break;
            case "Dash":
                if(m_timerDash != null)
                    StopCoroutine(m_timerDash);
                m_timerDash = null;
                m_ghost.ghostingEnabled = false;
                m_fxDash.SetActive(false);
                break;
            case "Run-Flip":
                SendMessage("OnFlip", SendMessageOptions.DontRequireReceiver);
                break;
            case "Run":
                m_stats.isMove = false;
                break;
            case "Slide":
                if(m_soundSlide != null)
                    Destroy(m_soundSlide);
                break;
        }
    }

    //call via send message
    void OnJumpView(int index)
    {
        switch (index)
        {
            case 0:
                SoundManager.PlaySound(m_clipJumpGround, false);
                StartCoroutine(IFXJump("jump"));
                break;
            case 1:
                SoundManager.PlaySound(m_clipJumpAir, false);
                StartCoroutine(IFXJump("jump_double"));
                break;
        }
    }

    //call via send message
    void OnBeatCritical()
    {
        GameObject go = Instantiate(m_fxCritical, transform.parent, false);
        go.transform.position = m_hitPosition.position;
        Destroy(go, 3f);
    }

    //call via send message
    void OnBeatNormal()
    {
        GameObject go = Instantiate(m_fxCritical, transform.parent, false);
        go.transform.position = m_hitPosition.position;
        Destroy(go, 3f);
    }

    void OnBeatWall(Vector3 collidedPoint = default)
    {
        if (collidedPoint == Vector3.zero)
            collidedPoint = m_hitPosition.position;
        SoundManager.PlaySound(m_clipFightWall, false);
        GameObject go = Instantiate(m_fxHitWall, transform.parent, false);
        go.transform.position = collidedPoint;
        Destroy(go, 3f);
    }

    void OnDashContinue()
    {
        if (m_timerDashSlowMotion != null)
            StopCoroutine(m_timerDashSlowMotion);
        m_timerDashSlowMotion = StartCoroutine(IDashSlowMotion());

        if(m_dashShakeForce != Vector2.zero)
        {
            GameController.VibrateCustom(m_dashShakeForce, 0.4f);
        }
    }
    ////////////////////////////////////////TIMER///////////////////////////////////
    IEnumerator IFXJump(string animName)
    {
        GameObject go = Instantiate(m_fxJump, transform.parent, true);
        go.SetActive(true);
        yield return new WaitForEndOfFrame();
        SkeletonAnimation anim = go.GetComponent<SkeletonAnimation>();
        TrackEntry entry = anim.AnimationState.SetAnimation(0, animName, false);
        entry.Complete += (t) =>
        {
            Destroy(go);
        };
    }

    IEnumerator IHit()
    {
        SoundManager.PlaySound(m_clipHitEffect, false, false, true);
        //create effect
        if (m_hitCache != null && m_hitCache.attacker != null)
        {
            GameObject go = Instantiate(m_fxHit, transform.parent, false);
            Vector3 mid = (m_hitCache.attacker.position + transform.position) / 2;
            go.transform.position = mid;
            Destroy(go, 3f);
        }
        yield return new WaitForEndOfFrame();
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(M_HIT_TIME_FREEZE);
        Time.timeScale = 1;
        SoundManager.PlaySound(m_clipHitVoice, false);
    }

    IEnumerator IDash()
    {
        yield return new WaitForSeconds(0.1f);
        m_ghost.ghostingEnabled = true;
        m_timerDash = null;
    }

    IEnumerator ISlowMotion()
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(m_slowMotionFightTime);
        if(!Mathf.Approximately(Time.timeScale, 0))
            Time.timeScale = 1;
    }

    IEnumerator IDashSlowMotion()
    {
        if (m_slowMotionDashTime == 0)
            yield break;
        Time.timeScale = 0.2f;
        yield return new WaitForSecondsRealtime(m_slowMotionDashTime);
        Time.timeScale = 1;
    }

    IEnumerator IRevive()
    {
        Color c = Color.white;
        c.a = 0;
        m_spine.Skeleton.SetColor(c);
        yield return new WaitForSeconds(0.15f);
        m_spine.Skeleton.SetColor(Color.white);
    }
}
