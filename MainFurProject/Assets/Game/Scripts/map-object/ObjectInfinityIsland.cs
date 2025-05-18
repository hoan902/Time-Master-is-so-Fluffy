using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class ObjectInfinityIsland : MonoBehaviour
{
    [SerializeField] private GameObject m_child;
    [SerializeField] private Transform m_islandContainer;
    [SerializeField] private GameObject m_rope;
    [SerializeField] private Transform m_ropeContainer;
    public Transform startPoint;
    public Transform endPoint;
    public float islandSpeed = 1f;
    public string key = "";
    public bool loop;

    private Vector3[] m_worldNodes;
    public GameObject[] islands = new GameObject[1];
    public int[] childStartIndexs = new int[1];

    public Vector3[] localNodes = new Vector3[1];
    public Vector3[] worldNode {get => m_worldNodes;}
        

    private void Awake() 
    {
        m_worldNodes = new Vector3[localNodes.Length];
        for(int i = 0; i < m_worldNodes.Length; ++i)
            m_worldNodes[i] = transform.TransformPoint(localNodes[i]);
    }

    void Start()
    {
        for(int i = 0; i < islands.Length; i++)
        {
            GameObject island = islands[i];
            island.GetComponent<ObjectIslandInfinityChild>().Init(this, childStartIndexs[i]);
        }
    }
////////////////////// Editor Only //////////////////////
    public void UpdateAllIslandPos()
    {
        if(localNodes.Length < 1 || islands.Length == 0)
            return;
        int islandAmount = islands.Length;
        float islandPosOffset = 0;
        float pathLength = 0;
        List<float> distances = new List<float>();
        for(int i = 1; i < localNodes.Length; i++)
        {
            float distance = Vector3.Distance(transform.TransformPoint(localNodes[i]), transform.TransformPoint(localNodes[i - 1]));
            distances.Add(distance);
            pathLength += distance;
        }
        float distanceBetweenIslands = pathLength / islandAmount;
        islandPosOffset = islandAmount == 1 ? (pathLength / 2) : ((pathLength - ((islandAmount - 1) * distanceBetweenIslands)) / 2);

        for(int i = 0; i < islandAmount; i++)
        {
            int islandIndex = i;
            int destinationLineIndex = 0;
            float tempDistance = 0;
            float destinationDistance = distanceBetweenIslands * (islandIndex + 1) - islandPosOffset;

            Vector3 direction = Vector3.zero;
            float distanceFromStartPos = 0;
            Vector3 startPosByLineIndex = Vector3.zero;
            
            for(int j = 0; j < distances.Count; j++)
            {
                tempDistance += distances[j];
                if(tempDistance >= destinationDistance)
                {
                    destinationLineIndex = j;
                    int startPosIndex = destinationLineIndex;
                    int endPosIndex = startPosIndex + 1;
                    distanceFromStartPos = destinationDistance - (tempDistance - distances[destinationLineIndex]);
                    startPosByLineIndex = transform.TransformPoint(localNodes[startPosIndex]);
                    direction = (transform.TransformPoint(localNodes[endPosIndex]) - startPosByLineIndex).normalized;
                    childStartIndexs[islandIndex] = startPosIndex;
                    break;
                }
            }
            islands[islandIndex].transform.position = startPosByLineIndex + direction * distanceFromStartPos;
        }
        
        // Create all ropes
        UpdateRopes();
    }
    public void UpdateRopes()
    {
        int ropeCount = m_ropeContainer.childCount;
        for(int i = 0; i < ropeCount; i++)
        {
            DestroyImmediate(m_ropeContainer.GetChild(0).gameObject);
        }
        for(int i = 0; i < localNodes.Length - 1; i++)
        {
            GameObject newRope = Instantiate(m_rope, m_ropeContainer);
            newRope.SetActive(true);

            int startRopePosIndex = i;
            int targetRopePosIndex = i + 1;

            Vector2 startPos = transform.TransformPoint(localNodes[startRopePosIndex]);
            Vector2 endPos = transform.TransformPoint(localNodes[targetRopePosIndex]);
            Vector3 dir = (endPos - startPos);
            newRope.GetComponent<SpriteRenderer>().size = new Vector2(dir.magnitude, 0.21f);
            float angle = Vector3.Angle(Vector3.right, dir);
            newRope.transform.eulerAngles = new Vector3(0, 0, dir.y > 0 ? angle : -angle);
            newRope.transform.position = startPos;
            newRope.transform.localScale = Vector3.one;
        }
    }
    public void AddIsland()
    {
        GameObject newIsland = Instantiate(m_child, m_islandContainer);
        newIsland.SetActive(true);
        newIsland.name = "island-" + (islands.Length - 1);
        int index = islands.Length - 1;
        islands[index] = newIsland;
    }
    public void RemoveIsland()
    {
        if(islands.Length > 0)
        {
            int toDestroyIndex = islands.Length - 1;
            DestroyImmediate(islands[toDestroyIndex]);            
        }
    }
    public void UpdateStartEndPos()
    {
        startPoint.localPosition = localNodes[0];
        endPoint.localPosition = localNodes[localNodes.Length - 1];
    }
    public void RemoveAllIslands()
    {
        if(islands.Length > 0)
            Array.Clear(islands, 0, islands.Length);
        if(childStartIndexs.Length > 0)
            Array.Clear(childStartIndexs, 0, childStartIndexs.Length);
        int totalChild = m_islandContainer.childCount;
        for(int i = 0; i < totalChild; i++) 
        {
            DestroyImmediate(m_islandContainer.GetChild(0).gameObject);
        }
    }
    public int GetIslandAmount()
    {
        return m_islandContainer.childCount;
    }
}
