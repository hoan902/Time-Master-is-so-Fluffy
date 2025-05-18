using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectMonsterFireBall : MonoBehaviour
{
    [SerializeField] float m_jumpDuration = 3f;
    [SerializeField] float m_jumpHeight = 5f;
    [SerializeField] float m_jumpTime = 2f;
    [SerializeField] float m_firstJumpDelay = 0;

    IEnumerator Start() 
    {
        // StartCoroutine(ScheduleJump());    
        yield return new WaitForSeconds(m_firstJumpDelay);
        transform.DOJump(transform.position, m_jumpHeight, 1, m_jumpTime).SetDelay(m_jumpDuration).SetLoops(-1).SetEase(Ease.Linear);
    }

    //private void OnTriggerEnter2D(Collider2D other) 
    //{
    //    if(other.gameObject.tag != GameTag.PLAYER)
    //        return;
    //    GameController.UpdateHealth(-1);    
    //}
}
