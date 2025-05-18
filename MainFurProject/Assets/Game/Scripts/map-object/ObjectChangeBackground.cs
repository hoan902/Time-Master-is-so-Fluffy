using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectChangeBackground : MonoBehaviour
{

    [SerializeField] private Season m_startSeason;
    [SerializeField] private Season m_endSeason;
    [SerializeField] private float m_fadeTime = 1f;

    [SerializeField] private bool m_useArea = false;

    private bool m_gameFinished;
    private bool m_ballDead = false;

    private void Awake() 
    {
        m_gameFinished = false;

        GameController.finishEvent += OnGameFinish;
        GameController.ballHurtEvent += OnBallHurt;
        GameController.updateRevivalCameraEvent += OnUpdateCameraRevival;
    }
    private void OnDestroy() 
    {
        GameController.finishEvent -= OnGameFinish;
        GameController.ballHurtEvent -= OnBallHurt;
        GameController.updateRevivalCameraEvent -= OnUpdateCameraRevival;
    }

    void OnBallHurt(bool isDead)
    {
        m_ballDead = isDead;
    }
    void OnUpdateCameraRevival(Vector3 savePoint)
    {
        m_ballDead = false;
    }
    void OnGameFinish(Vector2 startFinishPoint, Vector2 finishPoint)
    {
        m_gameFinished = true;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.tag != GameTag.PLAYER || m_gameFinished || m_ballDead)
            return;
        if(!m_useArea)
        {
            Season targetSeason = other.transform.position.x > transform.position.x ? m_startSeason : m_endSeason;
            GameController.ChangeBackground(targetSeason, m_fadeTime);
        }
        else
        {
            GameController.ChangeBackground(m_startSeason, m_fadeTime);
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if(other.gameObject.tag != GameTag.PLAYER || !m_useArea || m_gameFinished || m_ballDead)
            return;
        GameController.ChangeBackground(m_endSeason, m_fadeTime);
    }
}
