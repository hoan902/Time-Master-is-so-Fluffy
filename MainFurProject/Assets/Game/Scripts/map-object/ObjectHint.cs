
using System.Collections;
using UnityEngine;

public class ObjectHint : MonoBehaviour
{
    [SerializeField] private float m_time;
    [SerializeField] private GameObject m_anim;

    private bool m_active;
    void Start()
    {
        m_active = false;
        m_anim.SetActive(false);
        StartCoroutine(Waiting());
    }

    IEnumerator Waiting()
    {
        yield return new WaitForSeconds(m_time);
        m_active = true;
    }

    public void OnTriggerStay2D(Collider2D collider)
    {
        if(collider.tag != GameTag.PLAYER || !m_active)
            return;
        if(!m_anim.activeSelf)
            m_anim.SetActive(true);
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        if(collider.tag != GameTag.PLAYER || !m_active)
            return;
        m_anim.SetActive(false);
    }
}
