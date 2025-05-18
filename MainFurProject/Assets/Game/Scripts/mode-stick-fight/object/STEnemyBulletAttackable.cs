using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STEnemyBulletAttackable : STObjectInteractive
{
    [SerializeField] private GameObject m_fxExplosion;
    [SerializeField] private float m_slowSpeedRatio = 0.5f;
    [SerializeField] private float m_slowTime = 1f;
    [SerializeField] private int m_maxQuantityDust;
    [SerializeField] private int m_waveSpreadOutRange = 10;
    [SerializeField] private bool m_canInvertMovement;
    [SerializeField] private bool m_autoDestroy;
    [SerializeField] private float m_timeDestroy = 10f;
    [SerializeField] private bool m_fromMonster;
    [SerializeField] private bool m_noNeedInit;
    [SerializeField] private bool m_canRotate;
    [SerializeField] private float m_rotateSpeed;
    [SerializeField] private Transform m_bulletUI;
    [SerializeField] private GameObject m_wave;

    [SerializeField] private AudioClip m_audioExplode;

    private Vector2 m_direction;
    private float m_speed;
    private float m_startTime;
    private Vector2 m_startPos;
    private Rigidbody2D m_rigidbody;
    private bool m_inverted;
    private DamageDealerInfo m_damageDealerInfor;
    private Coroutine m_slowdownRoutine;
    private float m_baseSpeed;
    private CircleCollider2D m_collider;

    private bool m_start;

    public override void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();   
        m_collider = GetComponent<CircleCollider2D>(); 
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override void OnHit(DamageDealerInfo attackerInfor)
    {
        DamageDealerInfo fakeDamageDealerInfor = attackerInfor;
        fakeDamageDealerInfor.damage = 0;
        base.OnHit(fakeDamageDealerInfor);

        if(!m_inverted && m_canInvertMovement)
            InvertMovement();
        else
            Slowdown();
    }

    public void Init(Vector2 direction, float speed, DamageDealerInfo damageDealerInfo)
    {
        m_direction = direction.normalized;
        m_speed = speed;
        m_baseSpeed = speed;
        m_startTime = Time.time;
        m_startPos = transform.position;
        m_damageDealerInfor = damageDealerInfo;
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
            if(m_rotateSpeed > 0 && m_bulletUI != null)
                m_bulletUI.Rotate(Vector3.forward * m_rotateSpeed, Space.World);
            else
                RotateByVelocity();
        }
        if(m_speed == 0)
            return;
        transform.Translate(m_direction * m_speed * Time.deltaTime);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!m_start && !m_noNeedInit)
            return;
        if(collision.gameObject.tag == "monster" && m_fromMonster && !m_inverted)
            return;
        m_start = false;
        HitTarget(collision);
        ExplosionEffect();
    }
    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (!m_start && !m_noNeedInit)
            return;
        if(other.gameObject.tag == "monster" && m_fromMonster && !m_inverted)
            return;
        m_start = false;
        HitTarget(other.collider);
        ExplosionEffect();
        if((other.gameObject.tag == GameTag.GROUND || other.gameObject.tag == GameTag.WALL) && m_wave != null)
        {
            ActiveWave(other.GetContact(0));
        }
    }

    void InvertMovement()
    {
        m_inverted = true;
        m_start = false;
        m_direction = Quaternion.AngleAxis(180, Vector3.forward) * m_direction;
        m_speed *= 1.5f;
        Init(m_direction, m_speed, m_damageDealerInfor);
        StartCoroutine(DelayActiveCollider());
    }
    IEnumerator DelayActiveCollider()
    {
        m_collider.enabled = false;
        yield return null;
        m_collider.enabled = true;
    }
    void Slowdown()
    {
        if(m_slowdownRoutine != null)
            StopCoroutine(m_slowdownRoutine);
        m_slowdownRoutine = StartCoroutine(ISlowdown());
    }

    void HitTarget(Collider2D other)
    {
        if(other.GetComponent<STPlayerController>())
            STGameController.HitPlayer(m_damageDealerInfor);
        else
            other.gameObject.SendMessage("OnHit", m_damageDealerInfor, SendMessageOptions.DontRequireReceiver);           
    }
    public void ExplosionEffect()
    {
        GameObject go = Instantiate(m_fxExplosion);
        go.transform.SetParent(transform.parent, false);
        go.transform.position = transform.position;
        //
        if (m_audioExplode)
            SoundManager.PlaySound(m_audioExplode, false);
        if(m_bulletUI)
            m_bulletUI.gameObject.SetActive(false);
        //
        Destroy(gameObject, 0.2f);
    }

    void RotateByVelocity()
    {
        if(!m_rigidbody)
            return;
        float angle = Mathf.Atan2(m_rigidbody.velocity.y, m_rigidbody.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); 
    }
    void ActiveWave(ContactPoint2D contactPoint)
    {
        float angle = Vector2.SignedAngle(contactPoint.normal, Vector2.up);
        Quaternion targetRotation = Quaternion.Euler(0, 0, -angle);

        GameObject wave = Instantiate(m_wave, contactPoint.point, targetRotation, transform.parent);
        wave.SetActive(true);
        wave.GetComponent<STObjectBulletWave>().Init(m_waveSpreadOutRange);
    }

    IEnumerator ISlowdown()
    {
        m_speed = m_baseSpeed * m_slowSpeedRatio;
        yield return new WaitForSeconds(m_slowTime);
        m_speed = m_baseSpeed;
    }
}
