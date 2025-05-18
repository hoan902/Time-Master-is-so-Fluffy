using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class STEnemyPhysicBoulder : MonoBehaviour
{
    [SerializeField] private GameObject m_fxExplosion;
    [SerializeField] private bool m_autoDestroy;
    [SerializeField] private float m_timeDestroy = 10f;
    [SerializeField] private bool m_fromMonster;
    [SerializeField] private bool m_noNeedInit;
    [SerializeField] private bool m_canRotate;
    [SerializeField] private float m_rotateSpeed;
    [SerializeField] private Transform m_bulletUI;
    [SerializeField] private Transform m_bottomPos;
    [SerializeField] private Collider2D m_collider;
    [SerializeField] private STMonsterBoulderTrail m_trailFX;
    [SerializeField] private LayerMask m_wallLayer;
    [SerializeField] private LayerMask m_groundLayer;

    [SerializeField] private AudioClip m_audioExplode;
    [SerializeField] private AudioClip m_audioHitGround;

    public bool isGrounded {
        get
        {
            return m_isGrounded;
        }
    }
    public Transform bottomPos
    {
        get
        {
            return m_bottomPos;
        }
    }
    private Vector2 m_direction;
    private float m_speed;
    private bool m_isFromBoss = false;
    private Rigidbody2D m_rigidbody;
    private bool m_isGrounded;
    private bool m_landed;

    private bool m_start;

    private void Awake() 
    {
        m_rigidbody = GetComponent<Rigidbody2D>();    
    }

    public void Init(Vector2 direction, float speed)
    {
        m_direction = direction.normalized;
        m_speed = speed;
        m_trailFX.Init(this);

        m_start = true;

        if(m_autoDestroy)
            Destroy(gameObject, m_timeDestroy);
    }

    private void Update()
    {
        if (!m_start)
            return;
        if(m_canRotate && m_isGrounded)
        {
            Vector3 axisDir = m_direction.x < 0 ? Vector3.forward : Vector3.back;
            if (m_rotateSpeed > 0 && m_bulletUI != null)
                m_bulletUI.Rotate(axisDir * m_rotateSpeed, Space.World);
            else
                RotateByVelocity();
        }
        CheckGrounded();
        if(!m_landed && m_isGrounded)
        {
            m_landed = true;
            SoundManager.PlaySound3D(m_audioHitGround, 15, false, transform.position);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!m_start && !m_noNeedInit)
            return;
        if (collision.gameObject.tag == "monster" && m_fromMonster)
            return;
        if (m_isGrounded)
            return;
        m_start = false;
        ExplosionEffect();
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (m_isGrounded)
            m_rigidbody.velocity = (m_direction * m_speed);
        if (CheckWall())
        {
            m_start = false;
            ExplosionEffect();
        }
        if (collision.collider.CompareTag(GameTag.PLAYER))
        {
            m_start = false;
            ExplosionEffect();
        }
    }


    public void ExplosionEffect()
    {
        GameObject go = Instantiate(m_fxExplosion);
        go.transform.SetParent(transform.parent, false);
        go.transform.position = transform.position;
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
        Vector3 axisDir = m_direction.x > 0 ? Vector3.forward : Vector3.back;
        transform.rotation = Quaternion.AngleAxis(angle, axisDir); 
    }

    //------------------ Physic check ----------------
    bool CheckWall()
    {
        bool rightCheck = m_direction.x > 0;
        RaycastHit2D raycast = Physics2D.Raycast(m_collider.bounds.center, rightCheck ? Vector2.right : Vector2.left, m_collider.bounds.extents.x + 0.2f, m_wallLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center, (rightCheck ? Vector2.right : Vector2.left) * (m_collider.bounds.extents.x + 0.2f), Color.black);
#endif
        return raycast.collider != null;
    }
    void CheckGrounded()
    {
        RaycastHit2D leftCast = Physics2D.Raycast(m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, 0), Vector2.down, m_collider.bounds.extents.y + 1f, m_groundLayer);
        RaycastHit2D rightCast = Physics2D.Raycast(m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, 0), Vector2.down, m_collider.bounds.extents.y + 1f, m_groundLayer);
#if UNITY_EDITOR
        Debug.DrawRay(m_collider.bounds.center + new Vector3(-m_collider.bounds.extents.x, 0), Vector3.down * (m_collider.bounds.extents.y + 1f), Color.yellow);
        Debug.DrawRay(m_collider.bounds.center + new Vector3(m_collider.bounds.extents.x, 0), Vector3.down * (m_collider.bounds.extents.y + 1f), Color.yellow);
#endif
        m_isGrounded = leftCast.collider != null || rightCast.collider != null;
    }
}
