
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float m_straightFlyTime = 3;
    [SerializeField] private float m_fallGravity = 3f;
    [SerializeField] private GameObject m_fxMonsterBeingHit;
    [SerializeField] private Transform m_effPoint;
    [SerializeField] private AudioClip m_hitAudio;

    private DamageDealerInfo m_damageDealerInfo;
    private bool m_ready;
    private Vector2 m_direction;
    private float m_speed;
    private Rigidbody2D m_rigidbody2D;
    private float m_straightTimer;

    private Vector3 startPos;

    public float StraightFlyTime
    {
        get => m_straightFlyTime;
    }

    public void Init(int damage, Vector2 direction, float speed)
    {
        startPos = transform.position;

        m_damageDealerInfo = new DamageDealerInfo();
        m_damageDealerInfo.damage = damage;
        m_damageDealerInfo.attacker = transform;
        m_direction = direction;
        m_speed = speed;
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        //m_rigidbody2D.AddForce(new Vector2(m_speed * m_direction, 0), ForceMode2D.Impulse);
        m_rigidbody2D.AddForce(m_speed * direction, ForceMode2D.Impulse);

        //transform.rotation = Quaternion.Euler(0, 0, direction > 0 ? 0 : -180);

        m_ready = true;
    }

    private void FixedUpdate()
    {
        if(!m_ready)
            return;
        float angle = Mathf.Atan2(m_rigidbody2D.velocity.y, m_rigidbody2D.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        m_straightTimer += Time.deltaTime;
        if (m_straightTimer >= m_straightFlyTime && m_rigidbody2D.gravityScale < 1)
        {
            m_rigidbody2D.gravityScale = m_fallGravity;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!m_ready || other.CompareTag(GameTag.PLAYER))
            return;
        STObjectInteractive otherComp = other.GetComponent<STObjectInteractive>();
        if (otherComp)
        {
            otherComp.gameObject.SendMessage("OnHit", m_damageDealerInfo, SendMessageOptions.DontRequireReceiver);
        }
        GameObject effectHit = Instantiate(m_fxMonsterBeingHit, m_effPoint.transform.position, Quaternion.identity, transform.parent);
        SoundManager.PlaySound3D(m_hitAudio, 10, false, m_effPoint.transform.position);
        Destroy(gameObject);
    }
}
