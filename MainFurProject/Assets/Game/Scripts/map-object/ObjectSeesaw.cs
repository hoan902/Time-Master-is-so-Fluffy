using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSeesaw : MonoBehaviour
{
    [SerializeField] private float m_size = 7f;
    [SerializeField] private SpriteRenderer m_floor;
    [SerializeField] private BoxCollider2D m_box;

    public void UpdateSize()
    {
        Vector2 size = m_floor.size;
        size.x = m_size;
        m_floor.size = size;
        size = m_box.size;
        size.x = m_size - 0.18f;
        m_box.size = size;
    }

}
