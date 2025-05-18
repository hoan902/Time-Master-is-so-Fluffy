using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDashCollectible : MonoBehaviour
{
    [SerializeField] private float m_duration = 5f;
    [SerializeField] private GameObject m_ui;

    private bool m_ready;
    private GameObject m_player;

    private void Start()
    {
        m_ready = true;
        m_ui.SetActive(true);
        m_player = FindObjectOfType<STPlayerController>().gameObject;
        m_player.SendMessage("OnActivateDash", true, SendMessageOptions.DontRequireReceiver);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!m_ready)
            return;
        if (!collision.CompareTag(GameTag.PLAYER))
            return;
        if (collision.offset.y < 0.5f)
            return;
        m_player.SendMessage("RenewDash", SendMessageOptions.DontRequireReceiver);
        m_player.SendMessage("OnDashContinue", SendMessageOptions.DontRequireReceiver);
        GameController.ShakeCamera();
        InputController.dashAction?.Invoke();
        StartCoroutine(IDelayShow());
    }
    
    IEnumerator IDelayShow()
    {
        m_ready = false;
        m_ui.SetActive(false);
        yield return new WaitForSeconds(m_duration);
        m_ready = true;
        m_ui.SetActive(true);
    }
}
