using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectBulletMushroom : MonoBehaviour
{
    [SerializeField] private GameObject m_bulletIcon;
    [SerializeField] private GameObject m_fxExplode;
    [SerializeField] private float m_minXRange = 6f;
    [SerializeField] private float m_maxRange = 10f;
    [SerializeField] private float m_flyTime = 1.5f;
    [SerializeField] private AudioClip m_audioExplode;
    
    private bool m_start;
    private GameObject m_soundExplode;

    public void Init(Transform shotPoint, Transform parent)
    {
        m_start = true;
        transform.position = shotPoint.position;

        int hDirection = shotPoint.position.x > parent.position.x ? 1 : -1;
        float targetXRange = shotPoint.localPosition.y > 1 ? m_maxRange : m_minXRange;
        Vector3 target = new Vector3(shotPoint.position.x + hDirection * targetXRange, shotPoint.parent.position.y - 2f);
        transform.DOJump(target, 5, 1, m_flyTime).SetEase(Ease.Linear);
    }
    private void OnDestroy() 
    {
        if(m_soundExplode != null)
            Destroy(m_soundExplode);    
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(!m_start)
            return;
        m_start = false;
        if(other.tag == GameTag.PLAYER)
        {
            GameController.UpdateHealth(-1);
        }
        m_bulletIcon.SetActive(false);
        m_fxExplode.SetActive(true);

        if(m_soundExplode != null)
            Destroy(m_soundExplode);
        m_soundExplode = SoundManager.PlaySound(m_audioExplode, false);
        Destroy(gameObject, 2f);
    }
}
