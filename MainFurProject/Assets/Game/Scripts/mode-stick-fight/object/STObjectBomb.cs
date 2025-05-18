using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectBomb : MonoBehaviour
{
    [SerializeField] private GameObject m_effectExplosion; 
    [SerializeField] private ContactFilter2D m_contactFilter2D;

    private DamageDealerInfo m_damageDealerInfor;
    private Collider2D m_impactCollider;

    private void Awake() 
    {
        m_damageDealerInfor = new DamageDealerInfo();
        m_damageDealerInfor.attacker = this.transform;    
    }

    public void Init(int damage, int scale = 1)
    {
        m_damageDealerInfor.damage = damage;
        GameObject go = Instantiate(m_effectExplosion);
        go.transform.SetParent(transform.parent, false);
        go.transform.position = transform.position;
        go.transform.localScale = new Vector3(scale, scale, scale);
        go.transform.eulerAngles = Vector3.zero;

        HitTargets();
        Destroy(gameObject, 0.2f); ;
    }

    void HitTargets()
    {
        List<Collider2D> result = new List<Collider2D>();
        m_impactCollider = GetComponent<Collider2D>();
        m_impactCollider.OverlapCollider(m_contactFilter2D, result);
        bool hitPlayer = false;
        for(int i = 0; i < result.Count; i++)
        {
            if(result[i].tag == GameTag.PLAYER && !hitPlayer)
            {
                hitPlayer = true;
                STGameController.HitPlayer(m_damageDealerInfor);
            }
            else
                result[i].gameObject.SendMessage("OnHit", m_damageDealerInfor, SendMessageOptions.DontRequireReceiver);
        }
    }

    // public void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if(m_damageDealerInfor == null)
    //         return;
    //     if (collision.tag == GameTag.PLAYER)
    //     {
    //         STGameController.HitPlayer(m_damageDealerInfor);
    //     }
    //     else
    //     {
    //         collision.gameObject.SendMessage("OnHit", m_damageDealerInfor, SendMessageOptions.DontRequireReceiver);
    //     }
    // }

    public float GetSize()
    {
        return GetComponent<CircleCollider2D>().radius;
    }

    public void SetSize(float size)
    {
        GetComponent<CircleCollider2D>().radius = size;
    }
}
