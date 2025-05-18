using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMaceChild : MonoBehaviour
{
    [SerializeField] private ObjectMace m_parent;
    [SerializeField] private int m_damage = 1;

    private void Start() 
    {
        if(m_parent.AlwaysShake)
            GetComponent<PointEffector2D>().forceMagnitude = 0;
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.tag == GameTag.PLAYER)
            GameController.UpdateHealth(-m_damage);
        else
        {
            MonsterWeakPoint monsterWeakPoint = collider.GetComponent<MonsterWeakPoint>();
            if(monsterWeakPoint != null)
                monsterWeakPoint.OnHit();
        }
    }
}
