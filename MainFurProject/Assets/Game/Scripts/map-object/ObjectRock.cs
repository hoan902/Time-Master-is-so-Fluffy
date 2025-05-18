using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRock : MonoBehaviour
{
    [SerializeField] private Rigidbody2D m_rigidbody;
    [SerializeField] private GameObject m_rock;
    [SerializeField] private LayerMask m_groundMask;
    [SerializeField] private AudioClip m_audioRoll;
    [SerializeField] private AudioClip m_audioImpact;

    private CircleCollider2D m_collider;
    private bool m_isOnGround;
    private GameObject m_soundRoll;
    private GameObject m_soundImpact;
    private bool m_isFalling;

    private void Awake() 
    {
        m_collider = GetComponent<CircleCollider2D>();    
    }
    private void OnDestroy() 
    {
        if(m_soundRoll != null)
            Destroy(m_soundRoll);
        if(m_soundImpact != null)
            Destroy(m_soundImpact);
    }

    void Update()
    {
        m_rock.transform.eulerAngles -= new Vector3(0, 0, m_rigidbody.velocity.x);
        CheckGround();

        if(m_isOnGround)
        {
            if(Mathf.Abs(m_rigidbody.velocity.x) < 0.5f && m_soundRoll != null)
                Destroy(m_soundRoll);
            else if(Mathf.Abs(m_rigidbody.velocity.x) > 0.5f && m_soundRoll == null)
                m_soundRoll = SoundManager.PlaySound(m_audioRoll, true, false);
        }
        else
        {
            m_isFalling = m_rigidbody.velocity.y < 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if(other.gameObject.tag != GameTag.GROUND || !m_isFalling)
            return;
        if(m_soundImpact != null)
            Destroy(m_soundImpact);
        m_soundImpact = SoundManager.PlaySound(m_audioImpact, false);
    }

    void CheckGround()
    {
        RaycastHit2D raycastHit2D = Physics2D.Raycast(m_collider.bounds.center, Vector2.down, m_collider.radius + 0.5f, m_groundMask);
        Debug.DrawRay(m_collider.bounds.center, Vector2.down * (m_collider.radius + 0.5f), Color.red);
        m_isOnGround = raycastHit2D.collider != null;
        m_isFalling = !m_isOnGround;
    }
}
