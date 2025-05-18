
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class ObjectMonsterFireBullet : MonoBehaviour
{
    [SerializeField] private Transform m_shootPoint;
    [SerializeField] private SkeletonAnimation m_spine;
    [SerializeField] private AudioClip m_audioBullet;//object-bullet
    [SerializeField] private AudioClip m_audioShoot;//object-enemi-shoot

    private bool m_dead = false;
    private float m_startTime;
    private int m_damage;
    private float m_bulletSpeed;
    private int m_direction;
    private Vector3 m_startPos;
    private Vector3 m_scale;
    private Vector3 m_lastPos;
    private Vector3 m_baseScale;

    void Start()
    {
        m_spine.AnimationState.Complete += OnAnimComplete;   
    }

    public void Init(int damage, float bulletSpeed, int direction, string bulletType, bool soundDefault = true)//direction 0 = x, 1 = y
    {
        m_dead = false;
        m_startTime = Time.time;
        m_damage = damage;
        m_bulletSpeed = bulletSpeed;
        m_direction = direction;
        m_startPos = m_shootPoint.position;
        transform.position = m_startPos;       
        m_lastPos = m_startPos; 
        m_baseScale = transform.localScale;
        m_scale = new Vector3(m_shootPoint.parent.localScale.x*m_shootPoint.localScale.x, m_shootPoint.parent.localScale.y*m_shootPoint.localScale.y, 1);
        if(soundDefault)
            SoundManager.PlaySound3D(m_audioShoot, 10, false, transform.position);
        //
        //StartCoroutine(DelayAnim(bulletType));
    }

    IEnumerator DelayAnim(string bulletType)
    {
        yield return null;
        m_spine.AnimationState.SetAnimation(0, bulletType, true);
    }

    void OnDestroy()
    {
        m_spine.AnimationState.Complete -= OnAnimComplete;
    }

    private void OnAnimComplete(TrackEntry trackentry)
    {
        if(trackentry.Animation.Name != "fx_enemi")
            return;
        Destroy(gameObject);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == null || m_dead)
            return;
        switch (collision.gameObject.tag)
        {
            case GameTag.PLAYER:
                StartDestroy();
                GameController.UpdateHealth(-1);
                break;
            case GameTag.GROUND:
            case GameTag.WALL:
            case GameTag.OBJECT_BOX:
                StartDestroy();
                break;
        }
    }

    void StartDestroy()
    {
        SoundManager.PlaySound3D(m_audioBullet, 10, false, transform.position);
        StopAllCoroutines();
        m_dead = true;
        m_spine.AnimationState.SetAnimation(0, "fx_enemi", false);
        GetComponent<Collider2D>().isTrigger = true;
        GetComponent<Collider2D>().enabled = false;
    }

    void LateUpdate()
    {
        if(m_dead)
            return;
        if (m_direction == 0) //move x
        {
            float x = m_startPos.x - (Time.time - m_startTime) * m_bulletSpeed *m_scale.x;
            transform.position = new Vector2(x, m_startPos.y);
            int direction = m_lastPos.x < x ? -1 : 1;
            transform.localScale = new Vector3(-m_baseScale.x*direction, m_baseScale.y, 1);
        }
        else
        {
            float y = m_startPos.y - (Time.time - m_startTime) * m_bulletSpeed * m_scale.y;
            transform.position = new Vector2(m_startPos.x, y);
            int direction = m_lastPos.y < y ? -1 : 1;
            transform.localScale = new Vector3(m_baseScale.x, m_baseScale.y*direction, 1);
        }
        m_lastPos = transform.position;
    }
}
