using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectIslandInfinityChild : MonoBehaviour
{
    private float m_speed = 3f;
    private Vector3[] m_path;
    private int m_passedIndex;
    private int m_targetIndex;
    private bool m_moving = false;
    private Vector3 m_moveDirection;
    private Rigidbody2D m_rigidbody;
    private ObjectInfinityIsland m_parent;
    private Transform m_gear;
    private Animation m_gearAnimation;

    private void Awake() 
    {
        m_rigidbody = GetComponent<Rigidbody2D>();    
        m_gearAnimation = transform.Find("child").GetComponent<Animation>();
    }

    public void Init(ObjectInfinityIsland parent, int startIndex)
    {
        m_parent = parent;
        m_speed = m_parent.islandSpeed;
        m_path = m_parent.worldNode;
        m_moving = true;
        m_passedIndex = startIndex;
        m_targetIndex = m_passedIndex + 1;
        UpdateMoveDirection(m_passedIndex, m_targetIndex);
        m_gearAnimation.Play();
    }

    private void LateUpdate() 
    {
        if(!m_moving)
            return;
        transform.Translate(m_moveDirection * m_speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, m_path[m_targetIndex]) <= 0.1f)
        {
            m_moving = false;
            transform.position = m_path[m_targetIndex];
            TargetNextPoint();
        }   
    }

    void TargetNextPoint()
    {
        m_passedIndex = m_targetIndex;
        m_targetIndex++;
        if(m_targetIndex >= m_path.Length)
        {
            if(m_parent.loop)
            {
                m_targetIndex = 0;
            }
            else
            {
                m_passedIndex = 0;
                m_targetIndex = 1;
                m_gearAnimation.Stop();
                transform.DOShakePosition(0.5f, 0.1f, 50).OnComplete(() => {
                    StartCoroutine(VibrateComplete());
                });
            }
            return;
        }
        UpdateMoveDirection(m_passedIndex, m_targetIndex);
        m_moving = true;
    }
    void UpdateMoveDirection(int startIndex, int targetIndex)
    {
        m_moveDirection = (m_path[targetIndex] - m_path[startIndex]).normalized;
    }

    public IEnumerator VibrateComplete()
    {
        GetComponent<STPlatformCatcher>().RemovePlayer();
        //m_rigidbody.position = m_path[0];
        transform.position = m_path[0];
        yield return null;
        UpdateMoveDirection(m_passedIndex, m_targetIndex);
        m_gearAnimation.Play();
        m_moving = true;
    }
}
