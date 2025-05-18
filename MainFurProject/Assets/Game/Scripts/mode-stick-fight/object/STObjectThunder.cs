using System.Collections;
using UnityEngine;

public class STObjectThunder : MonoBehaviour
{
    private const float M_TIME_EXPLOSTION = 0.1f;
    private const float M_TIME_DEFAULT = 0.5f;
    private const float M_TIME_DELAY = 1f;

    [SerializeField] private GameObject m_fxExplosion;
    [SerializeField] private BoxCollider2D m_boxCollider;
    [SerializeField] private AudioClip m_audioExplosion;
    [SerializeField] private float m_distanceObstacle = 6.5f;
    [SerializeField] private LayerMask m_layerObstacle;

    private bool m_isGrounded;
    private Vector2 m_pointHit;

    private void Awake()
    {
        m_boxCollider.enabled = false;
    }

    private IEnumerator Start()
    {
        CheckGrounded();
        float duration = M_TIME_DELAY + (m_isGrounded ? M_TIME_EXPLOSTION : M_TIME_DEFAULT);
        StartCoroutine(IDelayDealDamage(M_TIME_DEFAULT, M_TIME_DELAY));
        yield return new WaitForSeconds(duration);
        if (m_isGrounded)
        {
            ExplosionEffect();
            yield break;
        }
        Destroy(gameObject, 1f - duration);
    }

    private IEnumerator IDelayDealDamage(float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        m_boxCollider.enabled = true;
        yield return new WaitForSeconds(duration);
        m_boxCollider.enabled = false;
    }

    private void ExplosionEffect()
    {
        if (m_fxExplosion == null || !m_isGrounded)
            return;
        if (m_audioExplosion != null)
            SoundManager.PlaySound3D(m_audioExplosion, 15, false, m_pointHit);
        GameObject effect = Instantiate(m_fxExplosion, m_pointHit, Quaternion.Euler(0, 0, m_fxExplosion.transform.rotation.z), transform.parent);
        effect.SetActive(true);
        GameController.ShakeCameraWeak();
        Destroy(gameObject, 1f);
    }

    private void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, m_distanceObstacle, m_layerObstacle);
        m_isGrounded = hit.collider != null;
        if (m_isGrounded)
            m_pointHit = hit.point;
    }
}
