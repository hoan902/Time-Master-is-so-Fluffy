using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectFlax : MonoBehaviour
{
    [SerializeField] private int m_playerDamage = 20;
    [SerializeField] private int m_monsterDamage = 10;
    [SerializeField] private float m_width = 5;
    [SerializeField] private int m_forceMagnitude = 10000;
    [SerializeField] private BoxCollider2D m_left;
    [SerializeField] private BoxCollider2D m_right;
    [SerializeField] private SpriteRenderer m_item;
    [SerializeField] private bool m_instantKill = false;

    public bool InstantKill{get => m_instantKill;}

    public int GetDamage(bool toPlayer)
    {
        return toPlayer ? m_playerDamage : m_monsterDamage;
    }

    public void ResizeWidth()
    {
        float half = m_width / 2;
        Vector2 size = new Vector2(half, m_left.size.y);
        m_left.size = size;
        m_right.size = size;
        m_left.transform.localPosition = new Vector3(-1*half/2, m_left.transform.localPosition.y, 0);
        m_right.transform.localPosition = new Vector3(half / 2, m_left.transform.localPosition.y, 0);
        m_item.size = new Vector2(m_width, m_item.size.y);
    }

    public void UpdateForce()
    {
        m_left.GetComponent<AreaEffector2D>().forceMagnitude = m_forceMagnitude;
        m_right.GetComponent<AreaEffector2D>().forceMagnitude = m_forceMagnitude;
    }
}
