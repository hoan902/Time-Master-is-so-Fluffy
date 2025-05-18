using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFlaxFallChild : MonoBehaviour
{
    [SerializeField] private ObjectFlaxFall m_flax;
    [SerializeField] private GameObject m_effectDead;
    [SerializeField] private AudioClip m_audio;

    private bool m_stop;

    void Start()
    {
        m_stop = false;
    }
    
    public void OnTriggerEnter2D(Collider2D collider)
    {
        if(m_stop || !m_flax.IsFall())
            return;
        m_stop = true;
        switch(collider.tag)
        {
            case GameTag.PLAYER:
                GameController.UpdateHealth(-1);
                break;
            default:
                MonsterWeakPoint monsterWeakPoint = collider.GetComponent<MonsterWeakPoint>();
                if(monsterWeakPoint != null)
                    monsterWeakPoint.OnHit();
                break;
        }   
        SoundManager.PlaySound3D(m_audio, 10, false, transform.position);             
        GameObject eff = Instantiate(m_effectDead);
        eff.transform.SetParent(m_flax.transform.parent, false);
        eff.transform.position = transform.position;
        Destroy(m_flax.gameObject);
    }
}
