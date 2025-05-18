using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMoveableGround : MonoBehaviour
{
    [SerializeField] private string m_keyTrigger = "null";
    [SerializeField] private GameObject m_backupTrigger;

    [HideInInspector]
    public bool canReset = false;

    private MovingPlatform m_movingPlatform;
    private bool m_began;
    private Transform m_playerParent;
    private bool m_state;
    private GameObject m_triggerSource;

    private void Awake() 
    {
        m_movingPlatform = GetComponent<MovingPlatform>();    

        m_began = false;
        m_state = false;

        GameController.triggerEvent += OnTrigger;
        GameController.buffHeartEvent += OnBuffHeart;
    }
    private void OnDestroy() 
    {
        GameController.triggerEvent -= OnTrigger;
        GameController.buffHeartEvent -= OnBuffHeart;
    }

    void OnBuffHeart(Vector2 savePoint)
    {
        if(!canReset)
            return;
        if(m_triggerSource != null)
            m_triggerSource.gameObject.SendMessage("ResetState", SendMessageOptions.DontRequireReceiver);
        m_began = false;
        m_state = false;
        m_movingPlatform.ResetPlatform();
        if(m_backupTrigger != null)
            m_backupTrigger.SetActive(true);
    }
    
    void OnTrigger(string key, bool state, GameObject triggerSource)
    {
        if(key != m_keyTrigger || state == m_state)
            return;
        if(triggerSource != null)
            m_triggerSource = triggerSource;
        m_state = state;
        m_began = true;
        if(m_state)
            m_movingPlatform.MoveForward();
        else 
            m_movingPlatform.MoveBackward();
    }
}
