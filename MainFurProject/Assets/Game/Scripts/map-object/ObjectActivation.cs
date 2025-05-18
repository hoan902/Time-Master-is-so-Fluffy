using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectActivation : MonoBehaviour
{
    private enum ActivationType{Immediate, Continuous}
    private enum ChildAppearance{ScaleUp, MoveUp}
    [SerializeField] private string m_key = "";
    [SerializeField] private ActivationType m_activationType;
    [SerializeField] private ChildAppearance m_childAppearance;
    [SerializeField] private List<GameObject> m_objects;
    [SerializeField] private Ease m_ease = Ease.Unset;
    [SerializeField] private float m_delaySpawnTime = 0.05f;
    [SerializeField] private float m_childAppearanceTime = 0.25f;

    [Header("MoveUp height")]
    [SerializeField] private float m_childJumpHeight = 2f;

    private bool m_actived = false;

    private void Awake() 
    {
        if(m_objects.Count == 0)    
            return;
        foreach(GameObject child in m_objects)
        {
            child.SetActive(false);
        }
        
        GameController.triggerEvent += OnTrigger;
    }
    private void OnDestroy()
    {
        GameController.triggerEvent -= OnTrigger;
    }

    void OnTrigger(string key, bool state, GameObject triggerSource)
    {
        if(key != m_key || state == false || m_actived)
            return;
        m_actived = true;

        switch(m_activationType)
        {
            case ActivationType.Immediate:
                ActiveChildImmediate();
                break;
            case ActivationType.Continuous:
                StartCoroutine(ActiveChild(m_delaySpawnTime));
                break;
        }
    }

    IEnumerator ActiveChild(float delaySpawnTime)
    {
        for(int i = 0; i < m_objects.Count; i++)
        {
            m_objects[i].SetActive(true);
            switch(m_childAppearance)
            {
                case ChildAppearance.MoveUp:
                    Transform go = m_objects[i].transform;
                    go.DOJump(go.position, m_childJumpHeight, 1, m_childAppearanceTime).SetEase(m_ease);
                    break;
                case ChildAppearance.ScaleUp:
                    Transform go1 = m_objects[i].transform;
                    Vector2 childBaseScale = go1.transform.localScale;
                    m_objects[i].transform.localScale = Vector3.zero;
                    go1.transform.DOScale(childBaseScale, m_childAppearanceTime).SetEase(m_ease);
                    break;
            }
            yield return new WaitForSeconds(delaySpawnTime);
        }
    }

    void ActiveChildImmediate()
    {
        for(int i = 0; i < m_objects.Count; i++)
        {
            m_objects[i].SetActive(true);
            switch(m_childAppearance)
            {
                case ChildAppearance.MoveUp:
                    Transform go = m_objects[i].transform;
                    go.DOJump(go.position, m_childJumpHeight, 1, m_childAppearanceTime).SetEase(m_ease);
                    break;
                case ChildAppearance.ScaleUp:
                    Transform go1 = m_objects[i].transform;
                    Vector2 childBaseScale = go1.transform.localScale;
                    m_objects[i].transform.localScale = Vector3.zero;
                    go1.transform.DOScale(childBaseScale, m_childAppearanceTime).SetEase(m_ease);
                    break;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + Vector3.up, m_key);
    }
#endif
}
