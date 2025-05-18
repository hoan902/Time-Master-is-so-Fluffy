
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class ObjectSwitchPress : MonoBehaviour
{
    [SerializeField] private string m_key = "";
    [SerializeField] private SkeletonAnimation m_button;
    [SerializeField] private bool m_playerAcceptOnly = false;
    [SerializeField] private bool m_oneWayOnly = false;
    [SerializeField] private AudioClip m_audio;//object-switch-press

    private bool m_on;
    private List<Collider2D> m_colliders;

    void Start()
    {
        m_on = false;
        m_colliders = new List<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == null)
            return;
        if(m_oneWayOnly && collision.transform.position.y < transform.position.y)
            return;
        if(m_playerAcceptOnly && collision.tag != GameTag.PLAYER)
            return;
        if (m_playerAcceptOnly && collision.tag == GameTag.PLAYER && collision.offset.y < 0.5f)
            return;
        if (collision.tag == GameTag.PLAYER || collision.tag == GameTag.OBJECT_BOX || collision.tag == GameTag.MONSTER_MOVE_GROUND)
        {
            m_colliders.Add(collision);
            if(m_on)
                return;
            SoundManager.PlaySound3D(m_audio, 5, false, transform.position);
            m_on = true;
            m_button.AnimationState.SetAnimation(0,"down", false);
            GameController.DoTrigger(m_key, true, gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == null)
            return;
        if(m_oneWayOnly && collision.transform.position.y < transform.position.y)
            return;
        if(m_playerAcceptOnly && collision.tag != GameTag.PLAYER)
            return;
        if (m_playerAcceptOnly && collision.tag == GameTag.PLAYER && collision.offset.y < 0.5f)
            return;
        if (collision.tag == GameTag.PLAYER || collision.tag == GameTag.OBJECT_BOX || collision.tag == GameTag.MONSTER_MOVE_GROUND)
        {
            m_colliders.Remove(collision);
            if(!m_on || m_colliders.Count > 0)
                return;
            m_on = false;
            m_button.AnimationState.SetAnimation(0, "up", false);
            GameController.DoTrigger(m_key, false, gameObject);
        }
    }

    public void ResetState()
    {
        m_on = false;
        m_colliders = new List<Collider2D>();
    }

    public void SetKey(string key)
    {
        m_key = key;
    }
}
