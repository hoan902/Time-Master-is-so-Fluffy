using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLava : MonoBehaviour
{
    [SerializeField] private float m_delayKillTime = 2f;
    [SerializeField] AudioClip m_audioSank;

    private bool m_ready = true;
    private Transform m_player;
    private float m_playerMass;
    private bool m_killing = false;
    private Coroutine m_sankRoutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null || rb.bodyType == RigidbodyType2D.Static)
            return;
        
        SoundManager.PlaySound3D(m_audioSank, 10, false, other.transform.position);
        switch(other.tag)
        {
            case GameTag.PLAYER:
                if(!m_killing)
                {
                    m_killing = true;
                    m_player = other.transform;
                    m_playerMass = rb.mass;
                    StartCoroutine(IPlayerDead());
                    rb.gravityScale = 0.5f;
                    rb.velocity = new Vector3(0, -1, 0);
                }
                break;
            case GameTag.OBJECT_BOX:
                Rigidbody2D rigid = other.GetComponent<Rigidbody2D>();
                if (rigid != null)
                    rigid.mass = 40;
                break;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null || rb.bodyType == RigidbodyType2D.Static)
            return;
        if (other.tag == GameTag.PLAYER && !m_killing)
        {
            StopAllCoroutines();
            rb.mass = m_playerMass;
            m_player = null;
            GameController.ActiveInput(true);
            rb.gravityScale = 3;
        }
        else
        {
            SoundManager.PlaySound3D(m_audioSank, 10, false, other.transform.position);
        }
    }

    IEnumerator IPlayerDead()
    {
        if(m_sankRoutine != null)
            StopCoroutine(m_sankRoutine);
        m_sankRoutine =  StartCoroutine(IPlayerSank());
        GameController.ActiveInput(false);
        yield return new WaitForSeconds(m_delayKillTime);
        m_player.GetComponent<Rigidbody2D>().gravityScale = 3;
        GameController.UpdateHealth((-MainModel.gameInfo.health));
        m_killing = false;
        if(m_sankRoutine != null)
            StopCoroutine(m_sankRoutine);
    }

    IEnumerator IPlayerSank()
    {
        yield return new WaitForSeconds(0.5f);
        Rigidbody2D rigid = m_player.GetComponent<Rigidbody2D>();
        while(m_player != null)
        {
            if (rigid != null)
                rigid.mass += 0.5f;
            yield return null;
        }
    }
}
