
using Spine;
using Spine.Unity;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ObjectHome : MonoBehaviour
{

    [SerializeField] private Transform m_finishPoint;
    [SerializeField] private Transform m_startFinishPont;
    [SerializeField] private SkeletonAnimation m_spine;
    [SerializeField] private AudioClip m_audioOpen;//home_open
    [SerializeField] private AudioClip m_audioWheel;//home-wheel

    private bool m_stop;
    private GameObject m_soundWheel;

    void Awake()
    {
        // GameController.updatePointEvent += OnStarUpdate;
        m_stop = false;

    }

    void OnDestroy()
    {
        // GameController.updatePointEvent -= OnStarUpdate;
        StopAllCoroutines();
        if (m_soundWheel != null)
            Destroy(m_soundWheel);
    }

    public void EnterArea()
    {
        if (m_stop)
            return;
        Open();
    }

    public void ExitArea()
    {
        if (m_stop)
            return;
        Close();
    }

    private void OnStarUpdate(int value, Vector3? itemPos, int direction)
    {
        //if (value <= 0)
        //{
        //    m_complete = true;
        //    bool extraCondition = LevelLoader.instance.IsBossLevel(MainModel.gameInfo.level, MainModel.gameInfo.world) ? MainModel.gameInfo.levelCoin == MainModel.maxLevelCoin : true;
        //    if(m_area && extraCondition)
        //        Open();
        //}
    }

    private void OnOpenComplete(TrackEntry trackEntry)
    {
        if (trackEntry.Animation.Name == "open")
        {
            if (m_soundWheel != null)
                Destroy(m_soundWheel);
            m_soundWheel = SoundManager.PlaySound3D(m_audioWheel, 10, true, transform.position);
            m_spine.state.SetAnimation(0, "open2", true);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_stop)
            return;
        if (collision.tag == GameTag.PLAYER)
        {
            if (m_soundWheel != null)
                Destroy(m_soundWheel);
            m_stop = true;
            GameController.Finish(m_startFinishPont.position, m_finishPoint.position);
        }
    }

    void Open()
    {
        SoundManager.PlaySound3D(m_audioOpen, 10, false, transform.position);
        TrackEntry entry = m_spine.state.SetAnimation(0, "open", false);
        entry.Complete += OnOpenComplete;
    }

    void Close()
    {
        m_spine.state.SetAnimation(0, "close", false);
    }
}
