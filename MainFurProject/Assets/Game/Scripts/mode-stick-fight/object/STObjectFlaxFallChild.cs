using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectFlaxFallChild : MonoBehaviour
{
    [SerializeField] private STObjectFlaxFall m_flax;
    [SerializeField] private GameObject m_effectDead;
    [SerializeField] private AudioClip m_audio;

    private bool m_stop;
    private DamageDealerInfo m_damageDealerInfor;

    void Start()
    {
        m_stop = false;
        m_damageDealerInfor = new DamageDealerInfo();
        m_damageDealerInfor.damage = m_flax.GetDamage(true);
        m_damageDealerInfor.attacker = this.transform;
    }
    
    public void OnTriggerEnter2D(Collider2D collider)
    {
        if(m_stop || !m_flax.IsFall() || m_damageDealerInfor == null)
            return;
        m_stop = true;
        switch(collider.tag)
        {
            case GameTag.PLAYER:
                m_damageDealerInfor.damage = m_flax.GetDamage(true);
                STGameController.HitPlayer(m_damageDealerInfor);
                Explode();
                break;
            case GameTag.GROUND:
            case GameTag.ONE_WAY:
            case GameTag.ELEVATOR:
                Explode();
                break;
            default:
                STObjectMonster monster = collider.GetComponent<STObjectMonster>();
                if(monster)
                {
                    m_damageDealerInfor.damage = m_flax.GetDamage(false);
                    monster.gameObject.SendMessage("OnHit", m_damageDealerInfor, SendMessageOptions.DontRequireReceiver);
                    Explode();
                }
                break;
        }   
        
    }

    void Explode()
    {
        SoundManager.PlaySound3D(m_audio, 10, false, transform.position);             
        GameObject eff = Instantiate(m_effectDead);
        eff.transform.SetParent(m_flax.transform.parent, false);
        eff.transform.position = transform.position;
        Destroy(m_flax.gameObject);
    }
}
