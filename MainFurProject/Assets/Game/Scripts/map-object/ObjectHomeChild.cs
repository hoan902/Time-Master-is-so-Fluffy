
using UnityEngine;

public class ObjectHomeChild : MonoBehaviour
{
    [SerializeField] private ObjectHome m_home;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == GameTag.PLAYER)
            m_home.EnterArea();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == GameTag.PLAYER)
            m_home.ExitArea();
    }
}
