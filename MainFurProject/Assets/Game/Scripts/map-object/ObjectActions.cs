using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ObjectActions : MonoBehaviour
{
    private enum State
    {
        None,
        Begin,
        BeginFinish,
        Finish
    }

    public string triggerKey = "";
    public float transitionDuration = 2f;
    public bool activeBeginTransition = true;
    public bool activeFinishTransition = true;
    public bool stopMusic = true;
    public bool m_canSkip = false;

    private PlayableDirector m_playable;
    private State m_state = State.None;
    private bool m_ballDead = false;
    private bool m_ending = false;
    private bool m_actived;

    void Start()
    {
        m_playable = GetComponent<PlayableDirector>();

        GameController.triggerEvent += OnTrigger;

        CutSceneController.nextActionEvent += OnNextAction;
        CutSceneController.pauseEvent += OnPause;
        CutSceneController.activeGroupEvent += OnActiveGroups;
        CutSceneController.beginFinishEvent += OnBeginFinish;
        GameController.ballHurtEvent += OnBallHurt;
        GameController.maskSkipCutsceneClosedEvent += OnSkip;
        CutSceneController.updateSpeedEvent += OnUpdateSpeed;
    }

    void OnDestroy()
    {
        GameController.triggerEvent -= OnTrigger;

        CutSceneController.nextActionEvent -= OnNextAction;
        CutSceneController.pauseEvent -= OnPause;
        CutSceneController.activeGroupEvent -= OnActiveGroups;
        CutSceneController.beginFinishEvent -= OnBeginFinish;
        GameController.ballHurtEvent -= OnBallHurt;
        GameController.maskSkipCutsceneClosedEvent -= OnSkip;
        CutSceneController.updateSpeedEvent -= OnUpdateSpeed;
    }

    void OnSkip()
    {
        if(m_ending)
            return;
        float finishTime = GetCutsceneFinishTime();
        if(finishTime < 0)
            return;
        m_playable.Pause();
        m_playable.time = finishTime - 0.1f;
        m_playable.Play();
    }
    void OnBallHurt(bool isDead)
    {
        m_ballDead = isDead;
        if(MainModel.inCutscene)
        {
            m_playable.Stop();
        }
    }
    private void OnTrigger(string key, bool state, GameObject triggerSource)
    {
        if (triggerKey != key || m_ballDead)
            return;
        StartCoroutine(DelayStartCutScene());
    }

    private void OnActiveGroups(bool active, List<string> groups)
    {
        TimelineAsset timeline = m_playable.playableAsset as TimelineAsset;
        if (timeline == null)
            return;        
        
        // foreach (TrackAsset track in timeline.GetOutputTracks())
        // {
        //     if (track.GetGroup() == null)
        //         continue;
        //     if (groups.Contains(track.GetGroup().name))
        //         track.muted = !active;         
        // }

        foreach(TrackAsset track in timeline.GetRootTracks())
        {
            if (groups.Contains(track.name))
            {
                foreach(TrackAsset childTrack in track.GetChildTracks())
                {
                    childTrack.muted = !active;
                }
            }
        }

        double t = m_playable.time;
        m_playable.RebuildGraph();
        m_playable.time = t;
    }

    private void OnPause(bool pause)
    {
        if(m_state == State.None)
            return;
        if (pause)
            m_playable.playableGraph.GetRootPlayable(0).Pause();
        else
            m_playable.playableGraph.GetRootPlayable(0).Play();
    }

    private void OnUpdateSpeed(float value)
    {
        m_playable.playableGraph.GetRootPlayable(0).SetSpeed(value);
    }

    private void OnNextAction()
    {
        switch (m_state)
        {
            case State.Begin:
                m_playable.Play();
                break;
            case State.BeginFinish:
                CutSceneController.PauseCutScene(false);
                break;
            case State.Finish:
                Destroy(gameObject);
                GameController.ActiveInput(true);
                GameController.ActivateUI(true);
                if (stopMusic)
                    SoundManager.MuteMusic(false, false);
                break;
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.tag != GameTag.PLAYER || m_actived)
            return;
        m_actived = true;
        StartCoroutine(DelayStartCutScene());
    }
    IEnumerator DelayStartCutScene()
    {
        yield return null;
        MainModel.inCutscene = true;
        m_ending = false;
        
        List<string> playedCutscenes = PlayerPrefController.GetAllPlayedCutscene();
        bool played = playedCutscenes.Contains(m_playable.playableAsset.name);
        CutSceneController.ActivateFastForward(m_canSkip && played);
        
        m_state = State.Begin;
        GetComponent<Collider2D>().enabled = false;
        GameController.ActiveInput(false);
        CutSceneController.DoCutSceneTransition(activeBeginTransition, transitionDuration, false);
        if(stopMusic)
            SoundManager.MuteMusic(true, false);
    }

    public void OnFinish()
    {
        m_state = State.Finish;        
        GameController.ActiveInput(true);
        CutSceneController.Finish();
        CutSceneController.ActivateFastForward(false);
        PlayerPrefController.SaveAllPlayedCutscene(m_playable.playableAsset.name);
    }

    public void OnBeginFinish()
    {
        m_ending = true;
        m_playable.playableGraph.GetRootPlayable(0).SetSpeed(1);
        m_state = State.BeginFinish;
        CutSceneController.PauseCutScene(true);
        CutSceneController.DoCutSceneTransition(activeFinishTransition, transitionDuration, true);
    }

    float GetCutsceneFinishTime()
    {
        TimelineAsset timeline = m_playable.playableAsset as TimelineAsset;
        TrackAsset firstTrack = timeline.GetOutputTrack(0);
        TrackAsset secondTrack = timeline.GetOutputTrack(1);

        int markerCount = firstTrack.GetMarkerCount();
        for(int i = 0; i < markerCount; i++)
        {
            IMarker marker = firstTrack.GetMarker(i);
            var signal = marker as SignalEmitter;
            if(signal.asset.name == "cutscene-finish")
            {
                return (float)marker.time;
            }
        }

        markerCount = secondTrack.GetMarkerCount();
        for(int i = 0; i < markerCount; i++)
        {
            IMarker marker = secondTrack.GetMarker(i);
            var signal = marker as SignalEmitter;
            if(signal.asset.name == "cutscene-finish")
            {
                return (float)marker.time;
            }
        }

        return -1;
    }
}


