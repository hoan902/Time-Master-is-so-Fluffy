
using UnityEngine;

public class ObjectZoomCamera : MonoBehaviour
{
    [SerializeField] private float m_ratio = 1.5f;
    [SerializeField] private float m_time = 2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {      
        if (collision.tag == GameTag.PLAYER)
        {
            GameController.ZoomCamera(m_ratio, m_time);
        }
    } 
    private void OnTriggerExit2D(Collider2D collision)
    {      
        if (collision.tag == GameTag.PLAYER)
        {
            GameController.ZoomCamera(1, m_time);
        }
    } 
}
