using System.Collections;
using UnityEngine;

public class STObjectDealDamageTrail : MonoBehaviour
{
    [SerializeField] private Transform m_right;
    [SerializeField] private GameObject m_effect;
    [SerializeField] private float m_effectDistance = 1f;
    [SerializeField] private AudioClip m_audioExplode;

    private int m_childCount;
    private ContactFilter2D m_enemyContactFilter;
    private DamageDealerInfo m_damageDealerInfor;
    private int m_direction;

    public void Init(ContactFilter2D enemyContactFilter, DamageDealerInfo damageDealerInfor, int maxQuantity, int direction)
    {
        m_enemyContactFilter = enemyContactFilter;
        m_damageDealerInfor = damageDealerInfor;
        m_direction = direction;
        m_childCount = maxQuantity;

        CreateEffect();
        StartCoroutine(ISpreadOut());
        Destroy(gameObject, 3f);
    }
    void CreateEffect()
    {
        // right
        for (int i = 0; i < m_childCount; i++)
        {
            GameObject rightEffect = Instantiate(m_effect, m_right);
            rightEffect.SetActive(false);
            rightEffect.transform.localPosition = new Vector2(m_direction * m_effectDistance * (i + 1), 0);
            rightEffect.GetComponent<STObjectDealDamageTrailChild>().Init(m_damageDealerInfor.damage);
        }
    }
    IEnumerator ISpreadOut()
    {
        for (int i = 0; i < m_right.childCount; i++)
        {
            Collider2D rChild = m_right.GetChild(i).GetComponent<Collider2D>();
            rChild.gameObject.SetActive(true);
            SoundManager.PlaySound3D(m_audioExplode, 10, false, rChild.transform.position);

            rChild.enabled = true;
            yield return new WaitForSeconds(0.02f);
            rChild.enabled = false;
            yield return new WaitForSeconds(0.02f);
        }
    }
}
