using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectElevatorArrow : MonoBehaviour
{
    [SerializeField] private string m_key = "";
    [SerializeField] private ObjectSwitchPress m_press;
    [SerializeField] private float m_slowSpeedRatio = 0.2f;
    [SerializeField] private SpriteRenderer m_platformRenderer;
    [SerializeField] private SpriteRenderer m_circleRenderer;
    [SerializeField] private SpriteRenderer m_arrowRenderer;
    [SerializeField] private Sprite[] m_platformSprites;
    [SerializeField] private Sprite[] m_circleSprites;
    [SerializeField] private float m_sizeX = 5.515625f;
    [SerializeField] private BoxCollider2D m_pressCollider;

    private bool m_state;
    private STMovingPlatform m_movingPlatform;
    private GameObject m_triggerSource;
    private Transform m_arrow;
    private bool m_checkStop = false;
    private BoxCollider2D m_boxCollider;

    private void Awake()
    {
        m_press.SetKey(m_key);
        m_state = false;
        m_platformRenderer.sprite = m_platformSprites[0];
        m_circleRenderer.sprite = m_circleSprites[0];
        m_arrow = m_arrowRenderer.transform;

        m_movingPlatform = GetComponent<STMovingPlatform>();
        m_boxCollider = GetComponent<BoxCollider2D>();

        GameController.triggerEvent += OnTrigger;
    }
    private void Start()
    {
        UpdateArrow(true);
    }
    void OnDestroy()
    {
        transform.DOKill();
        GameController.triggerEvent -= OnTrigger;
    }
    private void Update()
    {
        if (!m_checkStop)
            return;
        if (!m_movingPlatform.Started)
        {
            UpdateArrow(true);
            m_checkStop = false;
        }
    }

    void OnTrigger(string key, bool state, GameObject triggerSource)
    {
        if (m_key != key || m_state == state)
            return;
        if (triggerSource != null)
            m_triggerSource = triggerSource;
        m_state = state;
        if (m_state)
            MoveForward();
        else
            MoveBackward();
    }

    void MoveForward()
    {
        m_checkStop = false;
        m_movingPlatform.MoveForward();
        m_movingPlatform.UpdateVelocity(1);
        m_platformRenderer.sprite = m_platformSprites[1];
        m_circleRenderer.sprite = m_circleSprites[1];
        UpdateArrow(true);
    }
    void MoveBackward()
    {
        m_checkStop = true;
        m_movingPlatform.MoveBackward();
        m_movingPlatform.UpdateVelocity(m_slowSpeedRatio);
        m_platformRenderer.sprite = m_platformSprites[0];
        m_circleRenderer.sprite = m_circleSprites[0];
        UpdateArrow(false);
    }
    void UpdateArrow(bool isForward)
    {
        int startIndex = isForward ? 0 : 1;
        int endIndex = isForward ? 1 : 0;
        Vector2 direction = m_movingPlatform.worldNodes[endIndex] - m_movingPlatform.worldNodes[startIndex];
        float angle = Vector3.SignedAngle(Vector3.left, direction.normalized, Vector3.forward);
        m_arrow.rotation = Quaternion.Euler(0, 0, angle + 180);
    }

    public void UpdateBox()
    {
        if (m_boxCollider == null)
        {
            m_boxCollider = GetComponent<BoxCollider2D>();
        }
        m_boxCollider.size = new Vector2(m_sizeX - 0.5f, m_boxCollider.size.y);
        m_platformRenderer.size = new Vector2(m_sizeX, m_platformRenderer.size.y);
        if (m_pressCollider)
        {
            m_pressCollider.size = new Vector2(m_sizeX - 0.5f, m_pressCollider.size.y);
        }
    }
}
