
using UnityEngine;

public class ObjectSawChild : MonoBehaviour
{
    [SerializeField] private ObjectSaw m_object;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == GameTag.PLAYER)
        {
            GameController.UpdateHealth(-1 * m_object.GetDamage());
        }
        else if(collision.transform.parent.tag == GameTag.PLAYER)
        {
            GameController.UpdateHealth(-1);
        }
        else
        {
            MonsterWeakPoint monsterWeakPoint = collision.GetComponent<MonsterWeakPoint>();
            if(monsterWeakPoint != null)
                monsterWeakPoint.OnHit();
        }
    }
}
