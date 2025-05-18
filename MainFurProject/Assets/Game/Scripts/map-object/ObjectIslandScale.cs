using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectIslandScale : MonoBehaviour
{
    [SerializeField] private float m_leftSize = 3;
    [SerializeField] private float m_rightSize = 3;
    [SerializeField] private float m_moveSpeed = 5f;
    [SerializeField] private float m_revertSpeed = 2f;

    [SerializeField] private ObjectIslandScaleFloor m_leftFloor;
    [SerializeField] private ObjectIslandScaleFloor m_rightFloor;
    [SerializeField] private SpriteRenderer m_leftRope;
    [SerializeField] private SpriteRenderer m_rightRope;
    [SerializeField] private SpriteRenderer m_centerRope;
    [SerializeField] private Transform m_leftCircle;
    [SerializeField] private Transform m_rightCircle;
    [SerializeField] private BoxCollider2D m_leftCollider;
    [SerializeField] private BoxCollider2D m_rightCollider;

    private Rigidbody2D m_leftRigidbody;
    private Rigidbody2D m_rightRigidbody;
    private Vector3 m_leftStartPos;
    private Vector3 m_rightStartPos;
    private bool m_canMove;
    private Tween m_revertTween;

    public float m_weightRate;

    private void Awake() 
    {
        m_leftRigidbody = m_leftFloor.GetComponent<Rigidbody2D>();
        m_rightRigidbody = m_rightFloor.GetComponent<Rigidbody2D>();
    }
    private void Start() 
    {
        m_leftStartPos = m_leftFloor.transform.position;    
        m_rightStartPos = m_rightFloor.transform.position;

        m_weightRate = 0;

        UpdateRopePosition();
        UpdateRopeLenght();
        UpdatePulleyPosition();
    }

    private void FixedUpdate() 
    {
        m_weightRate = m_leftFloor.childCount - m_rightFloor.childCount;
        if(m_weightRate == 0)
        {
            Revert();
            return;
        }
        if(m_weightRate > 0 && !RightMax())
            LeftMoveDown();
        else if(m_weightRate < 0 && !LeftMax())
            RightMoveDown();
        
        UpdateRopeLenght();
    }

    void Revert()
    {
        int moveDir = 0;
        if(Vector3.Distance(m_leftFloor.transform.position, m_leftStartPos) >= 0.01f)
        {
            Vector3 leftVelocity = (m_leftFloor.transform.position.y < m_leftStartPos.y ? Vector3.up : Vector3.down) * m_revertSpeed * Time.deltaTime;
            Vector3 leftPosition = Vector3.Lerp(m_leftRigidbody.position, (Vector3)m_leftRigidbody.position + leftVelocity, 0.1f);
            m_leftRigidbody.MovePosition(leftPosition);
            UpdateRopeLenght();
            moveDir = leftVelocity.y > 0 ? 1 : -1;
        }
        if(Vector3.Distance(m_rightFloor.transform.position, m_rightStartPos) >= 0.01f)
        {
            Vector3 rightVelocity = (m_rightFloor.transform.position.y < m_rightStartPos.y ? Vector3.up : Vector3.down) * m_revertSpeed * Time.deltaTime;
            Vector3 rightPosition = Vector3.Lerp(m_rightRigidbody.position, (Vector3)m_rightRigidbody.position + rightVelocity, 0.1f);
            m_rightRigidbody.MovePosition(rightPosition);
            UpdateRopeLenght();
        }
        if(moveDir != 0)
            RotatePulley(moveDir < 0, m_revertSpeed);
    }
    void LeftMoveDown()
    {
        Vector3 velocityDown = Vector3.down * m_moveSpeed * Time.deltaTime;
        Vector3 downPosition = Vector3.Lerp(m_leftRigidbody.position, (Vector3)m_leftRigidbody.position + velocityDown, 0.1f);
        m_leftRigidbody.MovePosition(downPosition);

        Vector3 velocityUp = Vector3.up * m_moveSpeed * Time.deltaTime;
        Vector3 upPosition = Vector3.Lerp(m_rightRigidbody.position, (Vector3)m_rightRigidbody.position + velocityUp, 0.1f);
        m_rightRigidbody.MovePosition(upPosition);

        RotatePulley(true, m_moveSpeed);
    }
    void RightMoveDown()
    {
        Vector3 velocityDown = Vector3.down * m_moveSpeed * Time.deltaTime;
        Vector3 downPosition = Vector3.Lerp(m_rightRigidbody.position, (Vector3)m_rightRigidbody.position + velocityDown, 0.1f);
        m_rightRigidbody.MovePosition(downPosition);

        Vector3 velocityUp = Vector3.up * m_moveSpeed * Time.deltaTime;
        Vector3 upPosition = Vector3.Lerp(m_leftRigidbody.position, (Vector3)m_leftRigidbody.position + velocityUp, 0.1f);
        m_leftRigidbody.MovePosition(upPosition);

        RotatePulley(false, m_moveSpeed);
    }
    void RotatePulley(bool isLeft, float rotateSpeed)
    {
        int dir = isLeft ? 1 : -1;
        m_leftCircle.Rotate(new Vector3(0, 0, dir * rotateSpeed * 10 * Time.deltaTime));
        m_rightCircle.Rotate(new Vector3(0, 0, dir * rotateSpeed * 10 * Time.deltaTime));
    }

    bool LeftMax()
    {
        return m_leftFloor.transform.localPosition.y >= -0.5f;
    }
    bool RightMax()
    {
        return m_rightFloor.transform.localPosition.y >= -0.5f;
    }

    public void UpdateRopePosition()
    {
        m_leftRope.transform.localPosition = new Vector2(m_leftFloor.transform.localPosition.x, 0);
        m_rightRope.transform.localPosition = new Vector2(m_rightFloor.transform.localPosition.x, 0);
        m_centerRope.transform.localPosition = new Vector2(m_leftRope.transform.localPosition.x, 0);
    }
    public void UpdateRopeLenght()
    {
        m_leftRope.size = new Vector2(0.109375f, Mathf.Abs(m_leftFloor.transform.localPosition.y));
        m_rightRope.size = new Vector2(0.109375f, Mathf.Abs(m_rightFloor.transform.localPosition.y));
        m_centerRope.size = new Vector2(0.109375f, Mathf.Abs(m_leftFloor.transform.position.x - m_rightFloor.transform.position.x));
    }
    public void UpdatePulleyPosition()
    {
        m_leftCircle.localPosition = new Vector2(m_leftFloor.transform.localPosition.x + 0.15f, -0.15f);
        m_rightCircle.localPosition = new Vector2(m_rightFloor.transform.localPosition.x - 0.15f, -0.15f);
    }
    public void UpdateLeftSize()
    {
        m_leftFloor.GetComponent<SpriteRenderer>().size = new Vector2(m_leftSize, 0.546875f);
        m_leftCollider.size = new Vector2(m_leftSize + 2, m_leftCollider.size.y);
        m_leftFloor.transform.Find("left").localPosition = new Vector3(-m_leftSize / 2f, 0, 0);
        m_leftFloor.transform.Find("right").localPosition = new Vector3(m_leftSize / 2f, 0, 0);
    }
    public void UpdateRightSize()
    {
        m_rightFloor.GetComponent<SpriteRenderer>().size = new Vector2(m_rightSize, 0.546875f);
        m_rightCollider.size = new Vector2(m_rightSize + 2, m_rightCollider.size.y);
        m_rightFloor.transform.Find("left").localPosition = new Vector3(-m_rightSize / 2f, 0, 0);
        m_rightFloor.transform.Find("right").localPosition = new Vector3(m_rightSize / 2f, 0, 0);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
