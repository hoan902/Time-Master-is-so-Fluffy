using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlatformScaleUpDown : MonoBehaviour
{
    [SerializeField] private int m_quantity = 3;
    [SerializeField] private bool m_firstScaleUp = false;
    [SerializeField] private float m_firstScaleDelay = 2f;
    [SerializeField] private float m_scaleDuration = 5f;
    [SerializeField] private float m_scaleTime = 1f;
    [SerializeField] private float m_moveTime = 1f;
    [SerializeField] private Transform m_blockContainer;
    [SerializeField] private GameObject m_blockPrefab;
    [SerializeField] Rigidbody2D m_rigibody;
    [SerializeField] BoxCollider2D m_box;

    public GameObject[] blocks = new GameObject[1];
    public Vector3 endPosition;

    private List<Vector3> m_worldNodes;
    private List<Vector3> m_blockOutPositions;
    private int m_targetIndex;
    private Coroutine m_routine;
    private bool m_firstMoved;
    private bool m_moving;
    private float m_distance;
    private float m_moveSpeed;

    private void Awake()
    {
        m_worldNodes = new List<Vector3>();
        m_worldNodes.Add(transform.position);
        m_worldNodes.Add(transform.TransformPoint(endPosition));
        m_targetIndex = 1;
        m_distance = Vector3.Distance(m_worldNodes[0], m_worldNodes[1]);
        m_moveSpeed = m_distance / m_moveTime;
    }
    private void Start()
    {
        m_blockOutPositions = new List<Vector3>();

        for (int i = 0; i < blocks.Length; i++)
        {
            m_blockOutPositions.Add(blocks[i].transform.localPosition);
            if (!m_firstScaleUp)
                blocks[i].transform.localPosition = Vector3.zero;
        }
        if (!m_firstScaleUp)
            m_box.size = Vector2.one;

        if(blocks.Length % 2 == 0)
        {
            m_box.offset = (m_firstScaleUp) ? new Vector2(0.5f, 0) : Vector2.zero;
        }

        StartCoroutine(DelayFirstScale());
    }
    private void OnDestroy()
    {
        if (m_routine != null)
            StopCoroutine(m_routine);
    }

    IEnumerator DelayFirstScale()
    {
        yield return new WaitForSeconds(m_firstScaleDelay);
        if (!m_firstScaleUp)
            SpreadOut(0);
        else
            SpreadIn(0);
        Move(0);
    }

    void SpreadOut(float delayTime)
    {
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].transform.DOLocalMove(m_blockOutPositions[i], m_scaleTime).SetDelay(delayTime).SetEase(Ease.Linear);
        }
        DOTween.To(() => m_box.size, x => m_box.size = x, new Vector2(blocks.Length, 1), m_scaleTime).SetDelay(delayTime).OnComplete(() => SpreadOutComplete()).SetEase(Ease.Linear);
        if(blocks.Length % 2 == 0)
        {
            DOTween.To(() => m_box.offset, x => m_box.offset = x, new Vector2(0.5f, 0), m_scaleTime).SetDelay(delayTime).SetEase(Ease.Linear);
        }
    }
    void SpreadIn(float delayTime)
    {
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].transform.DOLocalMove(Vector3.zero, m_scaleTime).SetDelay(delayTime).SetEase(Ease.Linear);
        }
        DOTween.To(() => m_box.size, x => m_box.size = x, new Vector2(1, 1), m_scaleTime).SetDelay(delayTime).OnComplete(() => SpreadInComplete()).SetEase(Ease.Linear);
        if (blocks.Length % 2 == 0)
        {
            DOTween.To(() => m_box.offset, x => m_box.offset = x, new Vector2(0, 0), m_scaleTime).SetDelay(delayTime).SetEase(Ease.Linear);
        }
    }
    void SpreadOutComplete()
    {
        SpreadIn(m_scaleDuration);
    }
    void SpreadInComplete()
    {
        SpreadOut(m_scaleDuration);
    }
    void Move(float delayTime)
    {
        if (m_targetIndex > 1)
            m_targetIndex = 0;
        Vector3 direction = (m_worldNodes[m_targetIndex] - transform.position).normalized;
        Vector3 currentPos = m_rigibody.position;
        DOTween.To(() => currentPos, x => currentPos = x, m_worldNodes[m_targetIndex], m_moveTime).SetDelay(delayTime).SetEase(Ease.Linear).OnComplete(() => {
            MoveComplete();
        }).OnUpdate(() => {
            m_rigibody.velocity = direction * m_moveSpeed;
        });
    }
    void MoveComplete()
    {
        m_targetIndex++;
        Move(m_scaleDuration);
        m_rigibody.velocity = Vector2.zero;
    }

    // Editor Methods
    public void AddBlock()
    {
        GameObject newBlock = Instantiate(m_blockPrefab, m_blockContainer);
        newBlock.SetActive(true);
        newBlock.name = "block-" + (blocks.Length - 1);
        int index = blocks.Length - 1;
        blocks[index] = newBlock;
    }
    public void RemoveBlock()
    {
        if (blocks.Length > 0)
        {
            int toDestroyIndex = blocks.Length - 1;
            DestroyImmediate(blocks[toDestroyIndex]);
        }
    }
    public void UpdateAllBlocksPosition()
    {
        if (blocks.Length == 0)
            return;
        Vector2 blockSize = m_blockPrefab.GetComponent<SpriteRenderer>().bounds.size;
        for (int i = 0; i < blocks.Length; i++)
        {
            if (i == 0)
                blocks[i].transform.localPosition = Vector3.zero;
            else
            {
                if (i % 2 == 0)
                {
                    blocks[i].transform.localPosition = new Vector3(-(i / 2), 0, 0);
                }
                else
                {
                    blocks[i].transform.localPosition = new Vector3((i / 2) + 1, 0, 0);
                }
            }
            blocks[i].GetComponent<SpriteRenderer>().sortingOrder = (blocks.Length - i);
        }
    }
    public void UpdateColliderSize()
    {
        m_box.offset = new Vector2(blocks.Length % 2 == 0 ? 0.5f : 0, 0);
        m_box.size = new Vector2(blocks.Length, 1);
    }
    public void RemoveAllBlocks()
    {
        if (blocks.Length > 0)
            Array.Clear(blocks, 0, blocks.Length);
        int totalChild = m_blockContainer.childCount;
        for (int i = 0; i < totalChild; i++)
        {
            DestroyImmediate(m_blockContainer.GetChild(0).gameObject);
        }
    }
}
