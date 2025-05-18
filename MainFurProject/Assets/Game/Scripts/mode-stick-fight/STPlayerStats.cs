using System;
using CleverCrow.Fluid.FSMs;
using UnityEngine;

public enum STPlayerState
{
    Other,
    Hurt,
    Fight,
    Dash,
    Lock
}

public class STPlayerStats : MonoBehaviour
{
    private const string M_VARIABLE_FIGHT = "Fight";
    private const string M_VARIABLE_IS_FIGHT = "IsFight";
    private const string M_VARIABLE_IS_GROUND = "IsGround";
    private const string M_VARIABLE_SPEED_X = "SpeedX";
    private const string M_VARIABLE_SPEED_Y = "SpeedY";
    private const string M_VARIABLE_HURT = "Hurt";
    private const string M_VARIABLE_IS_HURT = "IsHurt";
    private const string M_VARIABLE_HURT_INDEX = "HurtIndex";
    private const string M_VARIABLE_IS_DEAD = "IsDead";
    private const string M_VARIABLE_JUMP = "Jump";
    private const string M_VARIABLE_JUMP_INDEX = "JumpIndex";
    private const string M_VARIABLE_DASH = "Dash";
    private const string M_VARIABLE_IS_DASH = "IsDash";
    private const string M_VARIABLE_FLIP = "Flip";
    private const string M_VARIABLE_IS_WALL = "IsWall";
    private const string M_VARIABLE_IS_FINISH = "IsFinish";
    private const string M_VARIABLE_IS_BLINK = "IsBlink";
    private const string M_VARIABLE_DEAD = "Dead";
    private const string M_VARIABLE_FINISH = "Finish";
    
    [HideInInspector]
    [SerializeField] private Animator m_animator;

    [HideInInspector] public Transform parent;
    public IFsm fsm;
    [HideInInspector] public Weapon weapon;

    public bool isGround
    {
        get => m_animator.GetBool(M_VARIABLE_IS_GROUND);
        set
        {
            m_animator.SetBool(M_VARIABLE_IS_GROUND, value);
            if(value)
                weapon.UnFreeze();
        }
    }

    public bool isWall
    {
        get => m_animator.GetBool(M_VARIABLE_IS_WALL);
        set
        {
            m_animator.SetBool(M_VARIABLE_IS_WALL, value);
            if(value)
                weapon.UnFreeze();
        }
    }

    public bool isHurt
    {
        get => m_animator.GetBool(M_VARIABLE_IS_HURT);
        set => m_animator.SetBool(M_VARIABLE_IS_HURT, value);
    }

    public int jumpIndex
    {
        set
        {
            m_animator.SetInteger(M_VARIABLE_JUMP_INDEX, value);
            m_animator.SetTrigger(M_VARIABLE_JUMP);
        }
    }
    

    public bool isDash
    {
        get => m_animator.GetBool(M_VARIABLE_IS_DASH);
        set => m_animator.SetBool(M_VARIABLE_IS_DASH, value);
    }

    public bool isFight
    {
        get => m_animator.GetBool(M_VARIABLE_IS_FIGHT);
        set => m_animator.SetBool(M_VARIABLE_IS_FIGHT, value);
    }

    public float speedY
    {
        set => m_animator.SetFloat(M_VARIABLE_SPEED_Y, value);
    }
    
    public float speedX
    {
        set => m_animator.SetFloat(M_VARIABLE_SPEED_X, value);
    }
    
    public bool isDead
    {
        set => m_animator.SetBool(M_VARIABLE_IS_DEAD, value);
    }
    
    public bool isBlink
    {
        set => m_animator.SetBool(M_VARIABLE_IS_BLINK, value);
    }


    public bool isFinish
    {
        set
        {
            m_animator.SetBool(M_VARIABLE_IS_FINISH, value);
            m_animator.SetTrigger(M_VARIABLE_FINISH);
        }
    }

    [HideInInspector]public bool isMove { get; set; }

    /// ///////////////////////////////////////////////////////////////
    private void Awake()
    {
        parent = transform.parent;
    }

    public void Flip()
    {
        m_animator.SetTrigger(M_VARIABLE_FLIP);
    }

    public void ResetFire()
    {
        isFight = false;
        m_animator.ResetTrigger(M_VARIABLE_FIGHT);
    }

    public void ClearTriggerFire()
    {
        m_animator.ResetTrigger(M_VARIABLE_FIGHT);
    }

    public void UpdateWeapon(GameObject go)
    {
        if (weapon != null)
            Destroy(weapon.gameObject);
        weapon = go.GetComponent<Weapon>();
        weapon.Init(gameObject);
    }

    public void Fight()
    {
        isMove = false;
        isFight = true;
        m_animator.SetTrigger(M_VARIABLE_FIGHT);
    }

    public void Hurt()
    {
        isMove = false;
        isHurt = true;
        m_animator.SetInteger(M_VARIABLE_HURT_INDEX, 0);
        m_animator.SetTrigger(M_VARIABLE_HURT);
    }

    public void Dash()
    {
        speedX = 0;
        isDash = true;
        m_animator.SetTrigger(M_VARIABLE_DASH);
    }

    public void Dead()
    {
        isDead = true;
        isDash = false;
        isFight = false;
        speedX = 0;
        isBlink = false;
        isMove = false;
        m_animator.SetTrigger(M_VARIABLE_DEAD);
    }

    public void Finish()
    {
        isDash = false;
        isFight = false;
        isBlink = false;
        isMove = false;
    }

    public void Jump(int index)
    {
        jumpIndex = index;
        m_animator.SetTrigger(M_VARIABLE_JUMP);
    }
}
