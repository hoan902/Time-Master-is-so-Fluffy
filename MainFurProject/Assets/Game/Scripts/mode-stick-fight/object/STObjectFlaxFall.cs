using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectFlaxFall : MonoBehaviour
{
    [SerializeField] private int m_playerDamage = 20;
    [SerializeField] private int m_monsterDamage = 10;
    [SerializeField] private float m_timeDelay = 0f;
    [SerializeField] private Rigidbody2D m_bodyRigid;
    [SerializeField] private BoxCollider2D m_bodyBox;
    [SerializeField] private SpriteRenderer m_bodyRender;
    [SerializeField] private LayerMask m_layerGround;
    [SerializeField] private AudioClip m_audio;

    private bool m_fall;

    void Start()
    {
        m_fall = false;
        RaycastHit2D ray = Physics2D.Raycast(transform.position, -transform.up, Mathf.Infinity, m_layerGround);
        if(ray.collider != null)
        {
            float size = (ray.point - (Vector2)transform.position).magnitude;
            m_bodyBox.size = new Vector2(m_bodyBox.size.x, size);
            m_bodyBox.offset = new Vector2(0, -size/2);
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if(m_fall)
            return;
        if(collider.tag != GameTag.PLAYER)
            return;
        m_bodyBox.enabled = false;
        m_fall = true;
        StartCoroutine(DelayFall());
    }

    IEnumerator DelayFall()
    {
        yield return new WaitForSeconds(m_timeDelay);
        SoundManager.PlaySound3D(m_audio, 10, false, m_bodyRigid.transform.position);
        m_bodyRigid.bodyType = RigidbodyType2D.Dynamic;
    }

    public int GetDamage(bool toPlayer)
    {
        return toPlayer ? m_playerDamage : m_monsterDamage;
    }

    public bool IsFall()
    {
        return m_fall;
    }
}
