using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class STObjectBulletWave : MonoBehaviour
{
    [SerializeField] private Transform m_left;
    [SerializeField] private Transform m_right;
    [SerializeField] private GameObject m_effect;
    [SerializeField] private float m_effectDistance = 1f;
    [SerializeField] private AudioClip m_audioExplode;

    private int m_childCount;

    public void Init(int maxChild)
    {
        m_childCount = maxChild;

        CreateEffect();

        StartCoroutine(ISpreadOut());
    }
    void CreateEffect()
    {
        // left
        for(int i = 0; i < m_childCount / 2; i++)
        {
            GameObject leftEffect = Instantiate(m_effect, m_left);
            leftEffect.SetActive(false);
            leftEffect.transform.localPosition = new Vector2(-(m_effectDistance * (i + 1)), 0);
        }

        // right
        for(int i = 0; i < m_childCount / 2; i++)
        {
            GameObject rightEffect = Instantiate(m_effect, m_right);
            rightEffect.SetActive(false);
            rightEffect.transform.localPosition = new Vector2(m_effectDistance * (i + 1), 0);
        }
    }
    IEnumerator ISpreadOut()
    {
        for(int i = 0; i < m_left.childCount; i++)
        {
            Collider2D lChild = m_left.GetChild(i).GetComponent<Collider2D>();
            Collider2D rChild = m_right.GetChild(i).GetComponent<Collider2D>();
            lChild.gameObject.SetActive(true);
            rChild.gameObject.SetActive(true);
            SoundManager.PlaySound3D(m_audioExplode, 10, false, lChild.transform.position);
            SoundManager.PlaySound3D(m_audioExplode, 10, false, rChild.transform.position);

            GameController.ShakeCameraWeak();
            lChild.enabled = true;
            rChild.enabled = true;
            yield return new WaitForSeconds(0.1f);
            lChild.enabled = false;
            rChild.enabled = false;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
