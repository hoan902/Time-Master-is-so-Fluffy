
using DG.Tweening;
using UnityEngine;

public class ObjectElevatorPath : MonoBehaviour
{
    [SerializeField] private string m_key = "";

    private bool m_state;  
    private MovingPlatform m_movingPlatform;
    private GameObject m_triggerSource;

    [HideInInspector]
    public bool canReset = false;

    void Awake()
    {
        GameController.triggerEvent += OnTrigger;
        GameController.buffHeartEvent += OnBuffHeart;

        m_state = false;
        //
        m_movingPlatform = GetComponent<MovingPlatform>();
    }


    void OnDestroy()
    {
        transform.DOKill();   
        GameController.triggerEvent -= OnTrigger;     
        GameController.buffHeartEvent -= OnBuffHeart;
    }

    void OnBuffHeart(Vector2 savePoint)
    {
        if(!canReset)
            return;
        m_state = false;
        if(m_triggerSource != null)
            m_triggerSource.gameObject.SendMessage("ResetState", SendMessageOptions.DontRequireReceiver);
    }

    private void OnTrigger(string key, bool state, GameObject triggerSource)
    {
        if (m_key != key || m_state == state)
            return;
        if(triggerSource != null)
            m_triggerSource = triggerSource;
        m_state = state;
        if (m_state)
            m_movingPlatform.MoveForward();
        else
            m_movingPlatform.MoveBackward();
    }
}
