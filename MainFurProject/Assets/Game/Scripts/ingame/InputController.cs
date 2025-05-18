using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Spine.Unity;
using DG.Tweening;

public class InputController : MonoBehaviour
{
    [SerializeField] private GameObject m_buttonJump;
    [SerializeField] private GameObject m_buttonFight;
    [SerializeField] private GameObject m_buttonDash;
    [SerializeField] private SkeletonGraphic m_tutorialHand;
    private string m_ballSkin;
    private string m_skinInButton;
    private string m_tempSkin;

    public static Action leftAction;
    public static Action rightAction;
    public static Action jumpAction;
    public static Action fightAction;
    public static Action fightExitAction;
    public static Action dashAction;
    public static Action releaseMove;

    private bool m_stop = true;
    private bool m_leftEnter;
    private bool m_rightEnter;
    private bool m_jumpEnter;
    private bool m_fightEnter;

    

    void Awake()
    {
        GameController.stopPlayerEvent += Stop;
        GameController.resumePlayerEvent += Resume;
        GameController.readyPlayEvent += OnReadyPlay;
        GameController.activeInputEvent += OnActive;
        GameController.activateInputTutorialEvent += OnActivateTutorial;
        //
        gameObject.SetActive(false);
    }
    private void Start()
    {
        PlayMode playMode = MainModel.gameInfo.playMode;
        m_buttonFight.SetActive(true);
        m_buttonDash.SetActive(PlayerPrefs.GetInt(DataKey.FIRST_BOSS_KILLED, 0) != 0);
    }
    void OnEnable()
    {
        m_leftEnter = false;
        m_rightEnter = false;
        m_jumpEnter = false;        
        m_buttonFight.SetActive(true);
    }
    private void OnApplicationPause(bool pauseStatus) 
    {
        m_leftEnter = false;
        m_rightEnter = false;
        m_jumpEnter = false;
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    void OnDestroy()
    {
        GameController.stopPlayerEvent -= Stop;
        GameController.resumePlayerEvent -= Resume;
        GameController.readyPlayEvent -= OnReadyPlay;
        GameController.activeInputEvent -= OnActive;
        GameController.activateInputTutorialEvent -= OnActivateTutorial;
    }
    
    public void OnActivateTutorial(InputTutorialType tutorialType, InputTutorialTarget inputTutorialTarget, bool toActive)
    {
        if (!gameObject.activeSelf)
            return;
        m_tutorialHand.gameObject.SetActive(toActive);
        if(m_tutorialHand.gameObject.activeSelf)
        {
            switch(tutorialType)
            {
                case InputTutorialType.Hold:
                    m_tutorialHand.AnimationState.SetAnimation(0, "click3", true);
                    break;
                case InputTutorialType.Click:
                    m_tutorialHand.AnimationState.SetAnimation(0, "click1", true);
                    break;
            }
            switch (inputTutorialTarget)
            {
                case InputTutorialTarget.Fight:
                    m_tutorialHand.transform.position = m_buttonFight.transform.position;
                    break;
                default:
                    m_tutorialHand.gameObject.SetActive(false);
                    break;
            }
        }
    }

    private void OnActive(bool active)
    {
        m_stop = !active;
        gameObject.SetActive(active);
    }

    private void OnReadyPlay()
    {
        m_stop = false;
        gameObject.SetActive(true);
    }

    public void OnLeftPointerEnter(BaseEventData eventData)
    {
        m_leftEnter = true;
    }

    public void OnRightPointerEnter(BaseEventData eventData)
    {
        m_rightEnter = true;
    }

    public void OnJumpPointerEnter(BaseEventData eventData)
    {
        m_jumpEnter = true;
    }

    public void OnLeftPointerExit(BaseEventData eventData)
    {
        m_leftEnter = false;
    }

    public void OnRightPointerExit(BaseEventData eventData)
    {
        m_rightEnter = false;
    }

    public void OnJumpPointerExit(BaseEventData eventData)
    {
        m_jumpEnter = false;
    }

    public void FightOnclick()
    {
        if(m_stop)
            return;
        fightAction?.Invoke();
    }

    public void FightEnter()
    {
        if(m_stop || m_fightEnter)
            return;
        m_fightEnter = true;
        fightAction?.Invoke();
    }

    public void FightExit()
    {
        if(m_stop || !m_fightEnter)
            return;
        m_fightEnter = false;
        fightExitAction?.Invoke();
    }

    public void DashOnClick()
    {
        if(m_stop)
            return;
        dashAction?.Invoke();
    }

    void Update()
    {
        if (m_stop)
            return;
        if(Input.GetKeyDown(KeyCode.LeftArrow))  
            m_leftEnter = true;
        if(Input.GetKeyDown(KeyCode.RightArrow))  
            m_rightEnter = true;
        if(Input.GetKeyUp(KeyCode.LeftArrow))  
            m_leftEnter = false;
        if(Input.GetKeyUp(KeyCode.RightArrow))  
            m_rightEnter = false;
        bool isTouch = Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer || Input.touchCount > 0;   
        if (isTouch)
        {
            if (m_leftEnter)
                leftAction?.Invoke();
            else if (m_rightEnter)
                rightAction?.Invoke();
        }
        
        if ((isTouch && m_jumpEnter) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpAction?.Invoke();
            m_jumpEnter = false;
        }

        if(!m_rightEnter && !m_leftEnter && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            releaseMove?.Invoke();
        
        //cheat
        if (Input.GetKeyDown(KeyCode.O))
            GameController.DoTrigger("key-test", true);
        if (Input.GetKeyDown(KeyCode.P))
            GameController.DoTrigger( "key-test", false);
        if(Input.GetKeyDown(KeyCode.S))
            fightAction?.Invoke();
        if(Input.GetKeyUp(KeyCode.S))
            fightExitAction?.Invoke();
        if(Input.GetKeyDown(KeyCode.D))
            dashAction?.Invoke();

#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Alpha0))
            STGameController.UpdatePlayerHp(100);
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            DamageDealerInfo damageDealerInfo = new DamageDealerInfo();
            damageDealerInfo.damage = 10;
            damageDealerInfo.attacker = transform.parent;
            STGameController.HitPlayer(damageDealerInfo);
        }
        if(Input.GetKeyDown(KeyCode.Alpha8))
            MainController.EquipWeapon(WeaponName.BattleAxe);
        if(Input.GetKeyDown(KeyCode.Alpha7))
            MainController.EquipWeapon(WeaponName.Sword);
        if(Input.GetKeyDown(KeyCode.Alpha6))
            MainController.EquipWeapon(WeaponName.Bow);
        if(Input.GetKeyDown(KeyCode.Alpha5))
            MainController.EquipWeapon(WeaponName.WindySword);
        if(Input.GetKeyDown(KeyCode.Alpha4))
            MainController.EquipWeapon(WeaponName.KratosAxe);
        if(Input.GetKeyDown(KeyCode.Alpha3))
            MainController.EquipWeapon(WeaponName.DanteSword);
        if(Input.GetKeyDown(KeyCode.Keypad1))
            MainController.EquipWeapon(WeaponName.DeathScythe);
        if(Input.GetKeyDown(KeyCode.Keypad2))
            MainController.EquipWeapon(WeaponName.DeathWindySword);
        if(Input.GetKeyDown(KeyCode.Keypad3))
            MainController.EquipWeapon(WeaponName.DeathComboSword);
        if(Input.GetKeyDown(KeyCode.Keypad4))
            MainController.EquipWeapon(WeaponName.DeathBattleAxe);
        if(Input.GetKeyDown(KeyCode.Keypad5))
            MainController.EquipWeapon(WeaponName.DeathBow);
        if(Input.GetKeyDown(KeyCode.Keypad6))
            MainController.EquipWeapon(WeaponName.DeathHammer);
#endif
    }

    public void Stop(bool isFinish)
    {
        m_stop = true;
        m_leftEnter = false;
        m_rightEnter = false;
        m_jumpEnter = false;   
    }

    public void Resume()
    {
        m_stop = false;
        m_leftEnter = false;
        m_rightEnter = false;
        m_jumpEnter = false;   
    }
}
