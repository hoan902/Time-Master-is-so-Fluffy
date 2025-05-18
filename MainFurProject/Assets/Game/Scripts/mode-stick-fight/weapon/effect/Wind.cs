using DG.Tweening;
using UnityEngine;

public class Wind : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_renderer;
    
    private DamageDealerInfo m_damageDealerInfo;
    private bool m_ready;
    private int m_direction;
    private float m_speed;
    private Rigidbody2D m_rigidbody2D;
    private float m_lifeTime;
    private float m_lifeTimer;
    private float m_currentScale = 1;
    private float m_delayHitTime;
    private float m_delayHitTimer;

    public void Init(int damage, int direction, float speed, float lifeTime, float delayHitTime = 0)
    {
        m_damageDealerInfo = new DamageDealerInfo();
        m_damageDealerInfo.damage = damage;
        m_damageDealerInfo.attacker = transform;
        m_direction = direction;
        m_speed = speed;
        m_lifeTime = lifeTime;
        m_delayHitTime = delayHitTime;
        transform.localScale = new Vector2(-m_direction, 1);
        transform.DOScale(new Vector3(-m_direction * 1.2f, 1.2f, 1), m_lifeTime);
        m_renderer.DOFade(0f, 0.2f).SetDelay(0.2f);
        
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_ready = true;
    }

    private void FixedUpdate()
    {
        if(!m_ready)
            return;
        m_lifeTimer += Time.deltaTime;
        m_delayHitTimer += Time.deltaTime;
        if (m_lifeTimer >= m_lifeTime)
        {
            Explode();
            return;
        }

        m_rigidbody2D.velocity = new Vector2(m_direction * m_speed, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<STObjectInteractive>() && m_delayHitTimer >= m_delayHitTime)
        {
            other.gameObject.SendMessage("OnHit", m_damageDealerInfo, SendMessageOptions.DontRequireReceiver);
        }
    }

    void Explode()
    {
        Destroy(gameObject);
    }
}
