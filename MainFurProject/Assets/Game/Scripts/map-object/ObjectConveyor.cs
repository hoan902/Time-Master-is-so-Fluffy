using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectConveyor : MonoBehaviour
{
    [SerializeField] private float m_speed = 10f;
    [SerializeField] private Vector3 m_startPoint;
    [SerializeField] private Vector3 m_endPoint;
    [SerializeField] private GameObject m_startPointObj;
    [SerializeField] private GameObject m_endPointObj;
    [SerializeField] private Transform m_gearStart;
    [SerializeField] private Transform m_gearEnd;
    [SerializeField] private SpriteRenderer m_line;
    [SerializeField] private BoxCollider2D m_collider;
    [SerializeField] private SurfaceEffector2D m_effector;
    [SerializeField] private Shader m_shader;

    private Material m_material;
    private float m_offsetX;

    void Start()
    {
        if(m_shader == null)
            return;
        m_material = new Material(m_shader);
        m_line.material = m_material;
        m_effector.speed = m_speed;
        m_offsetX = 0;
    }

    void Update()
    {
        if(m_material == null)
            return;
        m_offsetX -= m_speed * Time.deltaTime / 24;
        if (m_offsetX > 360)
            m_offsetX -= 360;
        else if (m_offsetX < -360)
            m_offsetX += 360;
        m_gearStart.eulerAngles = new Vector3(0, 0, m_offsetX * 200);
        m_gearEnd.eulerAngles = new Vector3(0, 0, m_offsetX * 200);
        m_material.SetTextureOffset("_MainTex", new Vector2(m_offsetX, 0));
    }

    public void UpdateLine()
    {
        m_startPointObj.transform.position = m_startPoint;
        m_endPointObj.transform.position = m_endPoint;
        m_line.transform.position = m_startPoint;
        Vector2 direction = m_endPoint - m_startPoint;
        Vector2 size = m_line.size;
        size.x = direction.magnitude;
        m_line.size = size;
        m_collider.size = size;
        Vector2 offset = m_collider.offset;
        offset.x = size.x / 2;
        m_collider.offset = offset;
        m_line.transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

}
