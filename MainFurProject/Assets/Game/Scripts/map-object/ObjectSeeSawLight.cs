using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ObjectSeeSawLight : MonoBehaviour
{
    [SerializeField] private float m_size = 5;
    [SerializeField] private SpriteRenderer m_center;
    [SerializeField] private SpriteRenderer m_floor;
    [SerializeField] private BoxCollider2D m_floorCollider;
    [SerializeField] private float m_lightInner = 1;
    [SerializeField] private float m_lightOutter = 3;
    [SerializeField] private Light2D m_leftLight;
    [SerializeField] private Light2D m_rightLight;

    private void Start() 
    {
        UpdateSize();    
    }

    public void UpdateSize()
    {
        Vector2 size = m_floor.size;
        size.x = m_size;
        m_floor.size = size;

        size = m_floorCollider.size;
        size.x = m_size - 0.18f;
        m_floorCollider.size = new Vector2(size.x, m_floor.size.y - 0.25f);

        m_leftLight.transform.localPosition = new Vector3(-m_floorCollider.bounds.extents.x + 0.5f, 0, 0);
        m_rightLight.transform.localPosition = new Vector3(m_floorCollider.bounds.extents.x - 0.5f, 0, 0);
    }

    public void UpdateLightSize()
    {
        m_leftLight.pointLightOuterRadius = m_lightOutter;
        m_leftLight.pointLightInnerRadius = m_lightInner;
        m_rightLight.pointLightOuterRadius = m_lightOutter;
        m_rightLight.pointLightInnerRadius = m_lightInner;
    }
}
