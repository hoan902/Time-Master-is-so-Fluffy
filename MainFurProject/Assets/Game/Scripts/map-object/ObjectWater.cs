using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectWater : MonoBehaviour
{

    [SerializeField] private Vector2 m_size = new Vector2(6f, 2f);
    [SerializeField] private Transform m_renderer;
    [SerializeField] private BoxCollider2D m_boxCollider;
    [SerializeField] private BuoyancyEffector2D m_buoyancyEffector;
    [SerializeField] private AudioClip m_soundFall;
    [SerializeField] private GameObject m_splash;
    [SerializeField] private GameObject m_bubble;

    private float m_Width;
    private Transform m_player;
    private float m_playerMass;

    void Start()
    {
        AdjustComponentSizes();
    }

    public void AdjustComponentSizes()
    {
        float offsetY = -0.125f * m_size.y;
        m_boxCollider.size = m_size;
        m_boxCollider.offset = new Vector2(0, offsetY);
        m_buoyancyEffector.surfaceLevel = m_size.y / 2 - 0.5f + offsetY;
        //           
        m_renderer.localScale = new Vector3(m_size.x, m_size.y + 0.5f, 1);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null || rb.bodyType == RigidbodyType2D.Static)
            return;
        SoundManager.PlaySound3D(m_soundFall, 10, false, collision.transform.position);

        Bounds bounds = collision.bounds;
        //PlaySplash(new Vector3(bounds.min.x + bounds.extents.x, bounds.min.y, bounds.min.z));

        switch (collision.tag)
        {
            case GameTag.PLAYER:
                m_player = collision.transform;
                m_playerMass = rb.mass;
                StartCoroutine(IPlayerDead());
                break;
            case GameTag.OBJECT_BOX:
                Rigidbody2D rigid = collision.GetComponent<Rigidbody2D>();
                if (rigid != null)
                    rigid.mass = 40;
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null || rb.bodyType == RigidbodyType2D.Static)
            return;
        if (collision.tag == GameTag.PLAYER)
        {
            StopAllCoroutines();
            m_bubble.SetActive(false);
            rb.mass = m_playerMass;
            m_player = null;
            GameController.ActiveInput(true);
        }
        else
        {
            SoundManager.PlaySound3D(m_soundFall, 10, false, collision.transform.position);
            Bounds bounds = collision.bounds;
            //PlaySplash(new Vector3(bounds.min.x + bounds.extents.x, bounds.min.y, bounds.min.z));
        }
    }

    private void PlaySplash(Vector3 position)
    {
        GameObject go = Instantiate(m_splash);
        go.transform.SetParent(transform, false);
        go.transform.position = position;
        Destroy(go, 1f);
    }

    IEnumerator IPlayerDead()
    {
        StartCoroutine(IUpdateBubble());
        while (MainModel.gameInfo.health > 0)
        {
            yield return new WaitForSeconds(1f);
            GameController.ActiveInput(false);
            GameController.UpdateHealth(-1);
        }
        m_bubble.SetActive(false);
    }

    IEnumerator IUpdateBubble()
    {
        yield return new WaitForSeconds(0.5f);
        Rigidbody2D rigid = m_player.GetComponent<Rigidbody2D>();
        m_bubble.SetActive(true);
        while (m_player != null)
        {
            m_bubble.transform.position = m_player.position;
            if (rigid != null)
                rigid.mass += 0.5f;
            yield return null;
        }
        m_bubble.SetActive(false);
    }
}
