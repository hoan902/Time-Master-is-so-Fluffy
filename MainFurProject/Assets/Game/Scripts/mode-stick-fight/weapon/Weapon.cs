using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour
{
    protected const string EVENT_HIT = "OnHit";
    protected const string EVENT_BEAT_WALL = "OnBeatWall";
    protected const string EVENT_WALL_KNOCKBACK = "WallKnockback";
    protected const string EVENT_BEAT_CRITICAL = "OnBeatCritical";
    protected const string EVENT_BEAT_NORMAL = "OnBeatNormal";

    public string skin;
    public string animationFightGroundDefault = "";
    public string animationFightAirDefault = "";
    public int baseDamage = 20;
    public float criticalTimeFreeze = 1f/60*20;
    [Min(0)]
    public float freezeTime = 0f;

    [Header("Combo Foward")]
    public bool isComboFoward;
    public float moveFowardSpeed = 20f;
    public float ignoreVelocityDuration = 0.1f;
    [Space]

    public AudioClip[] m_clipFight;
    //[HideInInspector]
    public Collider2D enemyAttackArea;
    //[HideInInspector]
    public Collider2D wallAttackArea;
    //[HideInInspector]
    public ContactFilter2D enemyContactFilter;
    //[HideInInspector]
    public ContactFilter2D wallContactFilter;
    public List<WeaponSkill> weaponSkills;
    public List<WeaponCombo> weaponCombos;
    
    // TODO (custom inspector) disable one if another selected
    public bool hasStrain;
    [Min(0)] public float strainFreezeTime = 0f;
    public string strainAnimation = "";
    public string strainFullAnimation = "";
    [SerializeField] private AudioClip m_strainClip;
    public bool hasCombo => weaponCombos != null && weaponCombos.Count > 1;
    
    //[HideInInspector]
    public STPlayerStats player;

    public Action strainEvent;
    public Action strainFullEvent;
    public Action fightEvent;

    protected bool canFreezeFight;
    protected bool canHit;
    protected int strainIndex;
    protected bool stopMoveAtBegin;
    
    public bool straining { get; protected set; }
    public int comboIndex { get; private set; }

    public string fightAnimation
    {
        get
        {
            if (hasStrain)
                return player.isGround
                    ? weaponSkills[strainIndex].animtaionFightGround
                    : weaponSkills[strainIndex].animtaionFightAir;
            if (hasCombo)
                return player.isGround
                    ? weaponCombos[comboIndex].animtaionFightGround
                    : weaponCombos[comboIndex].animtaionFightAir;
            return player.isGround ? animationFightGroundDefault : animationFightAirDefault;
        }
    }
    
    public bool canMove => !stopMoveAtBegin && !straining;

    private int m_damage
    {
        get
        {
            if (hasStrain)
                return weaponSkills[strainIndex].damage;
            if (hasCombo)
                return weaponCombos[comboIndex].damage;
            return baseDamage;
        }
    }

    protected DamageDealerInfo attackData
    {
        get
        {
            bool critical = Random.Range(0, 100) <= STGameConstant.PLAYER_CRITICAL_RATE;
            DamageDealerInfo info = new DamageDealerInfo()
            {
                damage = critical ? (int)(baseDamage * STGameConstant.PLAYER_CRITICAL_DAMAGE_RATE) : m_damage,
                critical = critical,
                attacker = player.transform
            };
            return info;
        }
    }
    
    private bool m_needFreeze => canFreezeFight && !Mathf.Approximately(freezeTime, 0);
    private bool m_isStrainFull => strainIndex >= (weaponSkills.Count - 1);
    private Coroutine m_timerStrain;

    public virtual void Init(GameObject playerObj)
    {
        player = playerObj.GetComponent<STPlayerStats>();
        comboIndex = -1;
        strainIndex = 0;
        stopMoveAtBegin = false;
    }

    public virtual void StartHit()
    {
        canHit = true;
        SoundManager.PlaySound(m_clipFight[Random.Range(0, m_clipFight.Length)], false);
    }

    public virtual void ComboUpdate()
    {
        comboIndex++;
        STPlayerController playerController = player.GetComponent<STPlayerController>();
        if (isComboFoward)
        {
            if (comboIndex > 0 && comboIndex <= (weaponCombos.Count - 1))
            {
                int direction = (int)Mathf.Sign(player.transform.localScale.x);
                playerController.IgnoreVelocity(ignoreVelocityDuration);
                LeanFowardAttack(direction, moveFowardSpeed);
            }
        }
        if (comboIndex >= weaponCombos.Count)
            comboIndex = 0;
        Fight();
    }

    public virtual void StrainUpdate()
    {
        strainIndex++;
        if (strainIndex >= (weaponSkills.Count - 1))
        {
            strainFullEvent?.Invoke();
            SoundManager.PlaySound(m_strainClip, false);
        }
    }

    public virtual void ExtraAttack()
    {
        
    }

    public virtual void ResetDefault()
    {
        strainIndex = 0;
    }

    public virtual void ResetCombo()
    {
        comboIndex = -1;
    }

    public void Strain()
    {
        strainEvent?.Invoke();
        m_timerStrain = StartCoroutine(IStrain());
    }
    
    public void CancelStrainIfNeeded()
    {
        if (m_timerStrain != null)
            StopCoroutine(m_timerStrain);
        m_timerStrain = null;
        if(straining)
            UnFreeze();
        straining = false;
    }

    public void Fight()
    {
        fightEvent?.Invoke();
        if (m_needFreeze)
            Freeze();
        CancelStrainIfNeeded();
    }

    public void EndHit()
    {
        canHit = false;
    }

    public virtual void Freeze()
    {
        canFreezeFight = false;
        StartCoroutine(IIgnoreFall(freezeTime));
    }

    public void UnFreeze()
    {
        canFreezeFight = true;
    }
    void LeanFowardAttack(int dir, float speed)
    {
        Rigidbody2D m_physicBody = player.GetComponent<Rigidbody2D>();
        Vector2 velocity = m_physicBody.velocity;
        velocity.x += dir * speed;
        m_physicBody.velocity = velocity;
    }
    
    IEnumerator IIgnoreFall(float duration)
    {
        Rigidbody2D m_physicBody = player.GetComponent<Rigidbody2D>();
        float start = Time.time;
        Vector2 velocity = m_physicBody.velocity;
        velocity.y = 0;
        m_physicBody.velocity = velocity;
        float startPosY = m_physicBody.position.y;
        while ((Time.time - start) < duration)
        {
            yield return new WaitForFixedUpdate();
            velocity = m_physicBody.velocity;
            if(velocity.y < 0)
                velocity.y = 0;
            m_physicBody.velocity = velocity;
            if (m_physicBody.position.y < startPosY)
                m_physicBody.position = new Vector2(m_physicBody.position.x, startPosY);
        }
    }
    
    IEnumerator IStrain()
    {
        Rigidbody2D m_physicBody = player.GetComponent<Rigidbody2D>();
        straining = true;
        yield return new WaitForSeconds(0.05f);
        bool canFreeze = canFreezeFight && !Mathf.Approximately(strainFreezeTime, 0);
        canFreezeFight = false;
        if(canFreeze)
            m_physicBody.velocity = Vector2.zero;
        float startPosY = m_physicBody.position.y;
        float startTime = Time.time;
        float startTimeFreeze = startTime;
        while (straining)
        {
            yield return new WaitForFixedUpdate();
            float timeNow = Time.time;
            if (canFreeze && (timeNow - startTimeFreeze) <= strainFreezeTime)
            {
                Vector2 velocity = m_physicBody.velocity;
                velocity.y = 0;
                m_physicBody.velocity = velocity;
                if (m_physicBody.position.y < startPosY)
                    m_physicBody.position = new Vector2(m_physicBody.position.x, startPosY);
            }
            if ((timeNow - startTime) >= weaponSkills[strainIndex].strainTime && !m_isStrainFull)
            {
                startTime = Time.time;
                StrainUpdate();
            }
        }
    }
}

[System.Serializable]
public class WeaponSkill
{
    public string animtaionFightGround;
    public string animtaionFightAir;
    public float strainTime;
    public int damage;
}

[System.Serializable]
public class WeaponCombo
{
    public string animtaionFightGround;
    public string animtaionFightAir;
    public int damage;
    public Collider2D attackArea;
}
