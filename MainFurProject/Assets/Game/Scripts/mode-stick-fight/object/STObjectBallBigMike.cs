using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectBallBigMike : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float m_force = 20;
    [SerializeField] private bool m_moveLeft;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private float m_groundCheckDistance = 0.2f;

    [Header("Reference")]
    [SerializeField] private GameObject m_ball;
    [SerializeField] private Rigidbody2D m_rigidbody;
    [SerializeField] private CircleCollider2D m_collider;

    private DamageDealerInfo m_damageDealerInfor;
    private int m_direction;
    private bool m_canMove;

    private void Awake() 
    {
        STPlayerController player = FindObjectOfType<STPlayerController>();
        Collider2D[] playerColliders = player.GetComponents<Collider2D>();
        foreach(Collider2D collider in playerColliders)
        {
            Physics2D.IgnoreCollision(m_collider, collider);
        }

        m_damageDealerInfor = new DamageDealerInfo();
        m_damageDealerInfor.damage = 1000;
        m_damageDealerInfor.attacker = this.transform;

        m_direction = m_moveLeft ? -1 : 1;
    }
    private void Start() 
    {
        Move(true);    
    }

    private void Update() 
    {
        m_ball.transform.eulerAngles -= new Vector3(0, 0, m_rigidbody.velocity.x);    
    }
    private void FixedUpdate() 
    {
        if(!m_canMove)
            return;
        
        // RaycastHit2D raycastHit2D = Physics2D.Raycast(m_collider.bounds.center - new Vector3(0, 1, 0),Vector2.right * m_direction ,m_collider.radius + m_groundCheckDistance);
        // Debug.DrawRay(m_collider.bounds.center - new Vector3(0, 1, 0), (Vector2.right * m_direction) * (m_collider.radius + m_groundCheckDistance), Color.yellow);
        // if(raycastHit2D.collider != null)
        //     m_canMove = false;

        m_rigidbody.AddForce(Vector2.right * m_direction * m_force);
    }

    void OnHitMonster(Collider2D other)
    {
        if(m_damageDealerInfor == null)
            return;

        STObjectMonster monster = other.gameObject.GetComponent<STObjectMonster>();
        if(monster)
        {
            monster.gameObject.SendMessage("OnHit", m_damageDealerInfor, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void Move(bool toMove)
    {
        m_canMove = toMove;
    }
}
