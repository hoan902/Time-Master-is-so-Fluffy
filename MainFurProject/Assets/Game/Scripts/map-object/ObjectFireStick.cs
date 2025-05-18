using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectFireStick : MonoBehaviour
{
    [SerializeField] bool m_clockwise;
    [SerializeField] float m_duration = 5f;
    [SerializeField] SpriteRenderer m_stick;
    [SerializeField] BoxCollider2D m_collider;
    
    private Tween m_tweener;

    // Start is called before the first frame update
    void Start()
    {
        float startAngle = transform.eulerAngles.z;
        if(m_tweener == null)
            m_tweener = transform.DORotate(new Vector3(0, 0, (m_clockwise ? -360 + startAngle : 360 + startAngle)), m_duration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
    }


    private void OnTrigger(Collider2D other) 
    {
        if(other.gameObject.tag != GameTag.PLAYER)
            return;
        GameController.UpdateHealth(-1);   
    }

    public void UpdateSize(float lenght)
    {
        m_stick.size = new Vector2(lenght, m_stick.size.y);

        Vector2 size = m_stick.bounds.size;
        m_collider.size = size;
        m_collider.offset = new Vector2(size.x / 2, 0);

    }
}
