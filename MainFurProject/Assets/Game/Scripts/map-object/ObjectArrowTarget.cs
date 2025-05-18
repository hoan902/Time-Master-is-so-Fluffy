using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectArrowTarget : MonoBehaviour
{
    private enum Target
    {
        BossNeo = 0,
        BossNeoHurt,
        SaveBall
    }
    [SerializeField] private float m_angle = 0f;
    [SerializeField] private Target m_target;
    [SerializeField] private Transform m_arrow;
    [SerializeField] private SpriteRenderer m_rendererTarget;
    [SerializeField] private Sprite[] m_spriteTargets;

    public void UpdateAngle()
    {
        m_arrow.eulerAngles = new Vector3(0, 0, m_angle);
    }

    public void UpdateTarget ()
    {
        m_rendererTarget.sprite = m_spriteTargets[(int)m_target];
    }
}
