
using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using TMPro;

public class ObjectSavePoint : MonoBehaviour
{
    [SerializeField] private List<GameObject> m_resetObjects = new List<GameObject>();

    [HideInInspector]
    [SerializeField] private SkeletonAnimation m_spine;
    [HideInInspector]
    [SerializeField] private GameObject m_effect;
    [HideInInspector]
    [SerializeField] private GameObject m_text;
    [HideInInspector]
    [SerializeField] private AudioClip m_audio;//object-check-point
    [SerializeField] private int m_healValue = 0;
    [SerializeField] private GameObject m_objectHeal;
    [SerializeField] private bool m_invisible;

    private bool m_saved;
    private bool m_active;//only active check point can do reset player, object...
    private List<ObjectCache> m_cache;


    void Awake()
    {
        GameController.checkedPointEvent += OnChecked;
        GameController.loadSavePointEvent += OnLoadCheckPoint;
    }

    IEnumerator Start()
    {
        yield return null;
        if (m_spine != null)
            m_spine.AnimationState.SetAnimation(0, "idle", true);
        m_saved = false;
        m_active = false;
        //
        m_cache = new List<ObjectCache>();
        for (int i = 0; i < m_resetObjects.Count; i++)
        {
            if(m_resetObjects[i] == null)
                continue;
            if(m_resetObjects[i].GetComponent<ObjectMoveableGround>())
                m_resetObjects[i].GetComponent<ObjectMoveableGround>().canReset = true;
            else if(m_resetObjects[i].GetComponent<ObjectElevatorPath>())
                m_resetObjects[i].GetComponent<ObjectElevatorPath>().canReset = true;
            Transform obj = m_resetObjects[i].transform;
            ObjectCache c = new ObjectCache();
            c.position = obj.transform.position;
            c.rotation = obj.transform.rotation;
            c.scale = obj.transform.localScale;
            m_cache.Add(c);
        }

        if(m_invisible)
        {
            Color tempColor = Color.white;
            tempColor.a = 0;
            m_spine.Skeleton.SetColor(tempColor);
            m_effect.GetComponent<ParticleSystem>().startColor = tempColor;
            m_text.GetComponent<TextMeshPro>().color = tempColor;
        }
    }

    void OnDestroy()
    {
        GameController.checkedPointEvent -= OnChecked;
        GameController.loadSavePointEvent -= OnLoadCheckPoint;
    }

    void OnChecked()
    {
        if (m_saved)
            m_active = false;
    }

    void OnLoadCheckPoint(Vector2? pos)
    {
        if (!m_active)
            return;
        for (int i = 0; i < m_resetObjects.Count; i++)
        {
            if (m_resetObjects[i] == null)
                continue;
            Transform obj = m_resetObjects[i].transform;
            ObjectCache c = m_cache[i];
            obj.SetLocalPositionAndRotation(c.position, c.rotation);
            obj.localScale = c.scale;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_saved || collision.gameObject == null)
            return;
        if (collision.tag == GameTag.PLAYER)
        {
            if(!m_invisible)
                SoundManager.PlaySound3D(m_audio, 5, false, transform.position);
            if (m_spine != null)
            {
                StartCoroutine(IDelayAnim());
            }
            m_effect.SetActive(true);
            m_text.SetActive(true);
            GameController.SavePoint(transform.position + new Vector3(0, 1));
            m_saved = true;
            m_active = true;

            if(m_healValue > 0)
                Heal(m_healValue, collision.transform);
        }
    }

    IEnumerator IDelayAnim()
    {
        yield return null;
        yield return null;
        yield return null;
        TrackEntry entry = m_spine.AnimationState.SetAnimation(0, "on", false);
        entry.Complete += (t) =>
        {
            m_spine.AnimationState.SetAnimation(0, "on_loop", true);
        };
    }

    void Heal(int value, Transform target)
    {
        GameObject go = Instantiate(m_objectHeal, transform.position + new Vector3(0, 1, 0), Quaternion.identity, transform.parent);
        go.GetComponent<ObjectHeal>().Init(m_healValue, transform.position, target);
    }

    private class ObjectCache
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
}
