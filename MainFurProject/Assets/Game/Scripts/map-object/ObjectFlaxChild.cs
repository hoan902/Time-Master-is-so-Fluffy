
using UnityEngine;

public class ObjectFlaxChild : MonoBehaviour
{
    private ObjectFlax m_flax;

    void Awake()
    {
        m_flax = gameObject.transform.parent.GetComponent<ObjectFlax>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == null || m_flax == null)
            return;
        if (collision.tag == GameTag.PLAYER)
        {
            int damage = m_flax.InstantKill ? 10 : 1;
            GameController.UpdateHealth(-damage);
        }
        else
        {
            MonsterWeakPoint monsterWeakPoint = collision.GetComponent<MonsterWeakPoint>();
            if(monsterWeakPoint != null)
                monsterWeakPoint.OnHit();
        }
    }
}
