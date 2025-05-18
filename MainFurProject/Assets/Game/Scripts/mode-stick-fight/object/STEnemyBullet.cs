using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STEnemyBullet : MonoBehaviour
{
    [SerializeField] private GameObject m_fxExplosion;
    [SerializeField] private bool m_autoDestroy;
    [SerializeField] private float m_timeDestroy = 10f;
    [SerializeField] private bool m_fromMonster;
    [SerializeField] private bool m_noNeedInit;
    [SerializeField] private bool m_canRotate;
    [SerializeField] private bool m_rotateClockwise;
    [SerializeField] private float m_rotateSpeed;
    [SerializeField] private Transform m_bulletUI;

    [SerializeField] private AudioClip m_audioExplode;

    private Vector2 m_direction;
    private float m_speed;
    private float m_startTime;
    private Vector2 m_startPos;
    private Rigidbody2D m_rigidbody;

    private bool m_start;

    private void Awake() 
    {
        m_rigidbody = GetComponent<Rigidbody2D>();    
    }

    public void Init(Vector2 direction, float speed, bool isClockWise = false)
    {
        m_direction = direction.normalized;
        m_speed = speed;
        m_startTime = Time.time;
        m_startPos = transform.position;
        m_rotateClockwise = isClockWise;

        m_start = true;

        if(m_autoDestroy)
            Destroy(gameObject, m_timeDestroy);
    }

    private void Update()
    {
        if (!m_start)
            return;
        if(m_canRotate)
        {
            Vector3 axisDir = Vector3.forward;
            if (m_rotateClockwise)
                axisDir = Vector3.back;
            if (m_rotateSpeed > 0 && m_bulletUI != null)
                m_bulletUI.Rotate(axisDir * m_rotateSpeed, Space.World);
            else
                RotateByVelocity();
        }
        if(m_speed == 0)
            return;
        Vector2 pos = m_startPos + (Time.time - m_startTime) * m_speed * m_direction;
        transform.position = pos;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!m_start && !m_noNeedInit)
            return;
        if(collision.gameObject.tag == "monster" && m_fromMonster)
            return;
        m_start = false;
        ExplosionEffect();
    }

    public void ExplosionEffect()
    {
        GameObject go = Instantiate(m_fxExplosion);
        go.transform.SetParent(transform.parent, false);
        go.transform.position = transform.position;
        go.SetActive(true);
        //
        if (m_audioExplode)
            SoundManager.PlaySound3D(m_audioExplode, 15, false, transform.position);
        if(m_bulletUI)
            m_bulletUI.gameObject.SetActive(false);
        //
        Destroy(gameObject);
    }

    void RotateByVelocity()
    {
        if(!m_rigidbody)
            return;
        float angle = Mathf.Atan2(m_rigidbody.velocity.y, m_rigidbody.velocity.x) * Mathf.Rad2Deg;
        Vector3 axisDir = Vector3.forward;
        if (m_rotateClockwise)
            axisDir = Vector3.back;
        transform.rotation = Quaternion.AngleAxis(angle, axisDir); 
    }
}
