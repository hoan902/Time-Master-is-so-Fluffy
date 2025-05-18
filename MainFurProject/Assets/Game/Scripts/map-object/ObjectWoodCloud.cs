using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectWoodCloud : MonoBehaviour
{
    [SerializeField] SpriteRenderer m_leftPart;
    [SerializeField] SpriteRenderer m_rightPart;
    [SerializeField] SpriteRenderer m_refSizeRenderer;
    [SerializeField] BoxCollider2D m_collider;
    [SerializeField] float m_colliderOffset = 2f;

    void Start()
    {
        
    }

    public void UpdateSize()
    {
        
        if(m_refSizeRenderer == null)
        {
            Debug.LogError("Chua co sprite ref kia anh An");
            return;
        }
        int clampedValue = (int)m_refSizeRenderer.size.x;
        m_refSizeRenderer.size = new Vector2(clampedValue, 1);
        UpdateBorderPos();
        UpdateColliderSize();
    }

    public void UpdateBorderPos()
    {
        float xPos = m_refSizeRenderer.size.x / 2;
        m_leftPart.transform.localPosition = new Vector3(-xPos - 0.5f, 0, 0);
        m_rightPart.transform.localPosition = new Vector3(xPos + 0.5f, 0, 0);
    }
    public void UpdateColliderSize()
    {
        m_collider.size = new Vector2(m_refSizeRenderer.size.x + m_colliderOffset, 0.8f);
    }
}
