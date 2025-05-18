using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class STCameraMovement : MonoBehaviour
{
    [SerializeField] private float m_scale = 1;
    [SerializeField] private float m_smoothTime = 0.2f;
    [SerializeField] private float m_maxForceUpdateY = 4f;

    [HideInInspector]
    [SerializeField] private Transform m_cameraFollowTarget;
    [SerializeField] private GameObject m_background;
    [HideInInspector]
    [SerializeField] private CinemachineVirtualCamera m_camera;
    [HideInInspector]
    [SerializeField] private CinemachineImpulseSource m_shaker;
    [HideInInspector]
    [SerializeField] private ContinuousImpulse m_shakerLoop;
    [SerializeField] private AnimationCurve m_shakeLoopCurve;
    [SerializeField] private List<Collider2D> m_borders;

    [HideInInspector]
    [SerializeField] private AudioClip m_audioRain;

    private Transform m_player;

    private Vector2 m_finishPoint;
    private bool m_stop;
    private bool m_firstTime;
    private Tweener m_tweener;
    private BackgroundParallax[] m_parallaxes;
    private BackgroundParallax[] m_fakeParallaxes;

    private Vector3 velocity = Vector3.zero;
    private float m_baseSize;//base camera size
    private float m_startSize;//start camera size    
    private float m_targetSize;//final size after zoom
    private float m_targetY;
    private float m_currentParallaxSize;//camera size using for parallax
    private CinemachineBrain m_brain;
    private CinemachineVirtualCamera m_virtualLiveCamera;
    private GameObject m_soundRain;
    private Tween m_fadeTween;
    private Camera m_realCamera;
    private Transform m_cameraTransform;
    private GameObject m_fakeBackground;
    private CinemachineConfiner2D m_playerCameraConfiner;
    private Collider2D m_baseBorder;
    private bool m_zoomed;//has been zoomed

    // change offset
    private List<ObjectChangeCameraOffset> m_offsetQueue;
    private Tweener m_changeOffsetTweener;
    private Vector2 m_baseOffset;
    private Cinemachine.CinemachineTransposer m_playerCameraTransposer;
    private ObjectChangeCameraOffset m_currentOffsetChanger;
    
    void Awake()
    {
        m_stop = false;
        m_baseSize = m_camera.m_Lens.OrthographicSize;
        m_startSize = m_camera.m_Lens.OrthographicSize * m_scale;
        m_brain = GetComponent<CinemachineBrain>();
        m_virtualLiveCamera = m_camera;
        m_brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Linear;
        m_parallaxes = m_background.GetComponentsInChildren<BackgroundParallax>();
        //
        m_brain.m_CameraActivatedEvent.AddListener(OnVirtualCameraActive);
        //
        m_playerCameraConfiner = m_camera.GetComponent<CinemachineConfiner2D>();
        m_baseBorder = m_playerCameraConfiner.m_BoundingShape2D;
        //
        m_playerCameraTransposer = m_camera.GetCinemachineComponent<CinemachineTransposer>();
        m_baseOffset = m_playerCameraTransposer.m_FollowOffset;
        m_offsetQueue = new List<ObjectChangeCameraOffset>();
        //
        GameController.finishEvent += OnFinish;
        GameController.updateRevivalCameraEvent += OnUpdateTarget;
        GameController.zoomCameraEvent += OnZoom;
        GameController.shakeCameraEvent += OnShake;
        GameController.shakeCameraWeakEvent += OnShakeWeak;
        GameController.shakeCameraLoopEvent += OnShakeLoop;
        GameController.vibrateCustomEvent += OnVibrateCustom;
        GameController.updateTargetYCameraEvent += OnUpdateTargetY;
        GameController.loadSavePointEvent += OnLoadSavePoint;
        GameController.vibrateCameraEvent += OnVibrate;
        GameController.vibrateCameraYEvent += OnVibrateY;
        GameController.updateWeatherEvent += OnUpdateWeather;
        GameController.updateBlendTimeEvent += OnUpdateBlendTime;
        GameController.changeBackgroundEvent += OnChangeBackground;
        GameController.maskTeleClosedEvent += OnMaskTeleClosed;
        GameController.changeCameraOffsetEvent += OnChangeOffSet;
    }

    IEnumerator Start()
    {
        m_realCamera = GetComponent<Camera>();
        m_cameraTransform = m_realCamera.transform;
        Vector2 camPos = m_realCamera.transform.position;
        for (int i = 0; i < m_parallaxes.Length; i++)
        {
            m_parallaxes[i].Init(camPos);
        }
        OnZoom(1, 0);
        //
        while (m_player == null)
        {
            STPlayerController sTPlayerController = FindObjectOfType<STPlayerController>();
            if(sTPlayerController != null)
                m_player = FindObjectOfType<STPlayerController>().transform;
            yield return null;
        }

        Vector3 position = m_player.transform.position;
        m_targetY = position.y;
        m_cameraFollowTarget.position = position;
        //
        CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
    }

    void OnDestroy()
    {
        if(m_brain != null)
            m_brain.m_CameraActivatedEvent.RemoveListener(OnVirtualCameraActive);
        GameController.finishEvent -= OnFinish;
        GameController.zoomCameraEvent -= OnZoom;
        GameController.shakeCameraEvent -= OnShake;
        GameController.shakeCameraWeakEvent -= OnShakeWeak;
        GameController.shakeCameraLoopEvent -= OnShakeLoop;
        GameController.vibrateCustomEvent -= OnVibrateCustom;
        GameController.updateRevivalCameraEvent -= OnUpdateTarget;
        GameController.updateTargetYCameraEvent -= OnUpdateTargetY;
        GameController.loadSavePointEvent -= OnLoadSavePoint;
        GameController.vibrateCameraEvent -= OnVibrate;
        GameController.vibrateCameraYEvent -= OnVibrateY;
        GameController.updateWeatherEvent -= OnUpdateWeather;
        GameController.updateBlendTimeEvent -= OnUpdateBlendTime;
        GameController.changeBackgroundEvent -= OnChangeBackground;
        GameController.maskTeleClosedEvent -= OnMaskTeleClosed;
        GameController.changeCameraOffsetEvent -= OnChangeOffSet;
        if (m_soundRain != null)
            Destroy(m_soundRain);

        m_tweener?.Kill(false);
        m_fadeTween?.Kill();
        m_changeOffsetTweener?.Kill();
        CinemachineImpulseManager.Instance.Clear();
    }

    void OnChangeOffSet(ObjectChangeCameraOffset objectChangeCameraOffset, bool change)
    {
        if(change && !m_offsetQueue.Contains(objectChangeCameraOffset))
        {
            m_offsetQueue.Add(objectChangeCameraOffset);
        }
        else if(!change && m_offsetQueue.Contains(objectChangeCameraOffset))
        {
            m_offsetQueue.Remove(objectChangeCameraOffset);
        }

        if(m_offsetQueue.Count == 0)
        {
            m_changeOffsetTweener?.Kill();
            m_currentOffsetChanger = null;
            Vector2 currentOffset = m_playerCameraTransposer.m_FollowOffset;
            DOTween.To(() => currentOffset, x => currentOffset = x, m_baseOffset, 2f).OnUpdate(() =>
            {
                m_playerCameraTransposer.m_FollowOffset = new Vector3(currentOffset.x, currentOffset.y, -10);
            });
        }
        else
        {
            if (m_currentOffsetChanger != null && m_currentOffsetChanger == m_offsetQueue[0])
                return;
            m_currentOffsetChanger = m_offsetQueue[0];
            Vector2 currentOffset = m_playerCameraTransposer.m_FollowOffset;
            m_changeOffsetTweener = DOTween.To(() => currentOffset, x => currentOffset = x, m_currentOffsetChanger.offset, 2f).OnUpdate(() =>
            {
                m_playerCameraTransposer.m_FollowOffset = new Vector3(currentOffset.x, currentOffset.y, -10);
            });
        }
    }
    void OnMaskTeleClosed(Vector3 destination)
    {
        UpdateBorder(destination);
    }
    void OnChangeBackground(Season targetSeason, float fadeTime)
    {
        if(m_fakeBackground != null)
        {
            Destroy(m_fakeBackground);
            m_fakeBackground = null;
        }

        m_fakeBackground = m_background.gameObject;
        m_fakeBackground.GetComponent<SortingGroup>().sortingOrder = -2;
        m_fakeParallaxes = m_parallaxes;
        //init new background
        GameObject bgSample = ConfigLoader.instance.config.GetBackground(targetSeason);
        if(bgSample == null)
            return;
        m_background = Instantiate(bgSample, m_fakeBackground.transform.parent, false);
        m_background.transform.position = m_fakeBackground.transform.position;
        m_parallaxes = m_background.GetComponentsInChildren<BackgroundParallax>();
        //
        Vector3 cameraPos = m_cameraTransform.position;
        for (int i = 0; i < m_parallaxes.Length; i++)
        {
            m_parallaxes[i].Init(cameraPos);
            m_parallaxes[i].SetAlpha(0);
            if(m_parallaxes[i].fitHeight)
                m_parallaxes[i].UpdateFit(m_currentParallaxSize / m_baseSize);
        }
        //force update position immediate
        float cameraSize = m_realCamera.orthographicSize;
        float width = cameraSize * Screen.width / Screen.height;
        bool update = true;
        while (update)
        {
            bool found = false;
            for (int i = 0; i < m_parallaxes.Length; i++)
            {
                bool result = m_parallaxes[i].Execute(cameraSize, width, cameraPos);
                if (result)
                    found = true;
            }
            update = found;
        }
        //
        Color c = m_parallaxes[0].renderers[0].color;
        m_fadeTween?.Kill();
        m_fadeTween = DOTween.ToAlpha(() => c, x => c = x, 1f, fadeTime).OnUpdate(() =>
        {
            for (int i = 0; i < m_parallaxes.Length; i++)
            {
                m_parallaxes[i].SetColor(c);
            }
        }).OnComplete(()=>
        {
            Destroy(m_fakeBackground);
            m_fakeBackground = null;
        });
    }

    void OnUpdateBlendTime(float time)
    {
        m_brain.m_DefaultBlend.m_Time = time;
    }
    void OnUpdateWeather(Weather weather)
    {
        switch (weather)
        {
            case Weather.Rain:
                if (m_soundRain == null)
                    m_soundRain = SoundManager.PlaySound(m_audioRain, true);
                SpriteRenderer[] renderers = m_background.GetComponentsInChildren<SpriteRenderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].color = new Color(0.62f, 0.62f, 0.62f);
                }
                break;
            default:
                break;
        }
    }
    private void OnLoadSavePoint(Vector2? obj)
    {
        CinemachineImpulseManager.Instance.Clear();
    }

    private void OnVirtualCameraActive(ICinemachineCamera to, ICinemachineCamera from)
    {
        if (m_brain == null || (m_virtualLiveCamera == null && to == null))
            return;
        if (!m_firstTime)
        {
            m_firstTime = true;
            return;
        }
        if (to != null && to is CinemachineVirtualCamera)
            m_virtualLiveCamera = to as CinemachineVirtualCamera;
        if (m_virtualLiveCamera == null)
            return;
        if (m_tweener != null)
            m_tweener.Kill(false);
        float size = m_currentParallaxSize;
        float targetSize = m_virtualLiveCamera.m_Lens.OrthographicSize;
        if (m_brain.ActiveBlend == null)
        {
            if (m_virtualLiveCamera == m_camera)
                m_brain.m_DefaultBlend.m_Time = 1f;
            UpdateParallax(m_virtualLiveCamera.m_Lens.OrthographicSize);
            return;
        }
        float time = m_brain.ActiveBlend.Duration;
        if (m_virtualLiveCamera == m_camera)
        {
            m_tweener = DOTween.To(() => size, x => size = x, targetSize, time).OnUpdate(() =>
            {
                UpdateParallax(size);
            }).OnComplete(() =>
            {
                if (!Mathf.Approximately(m_targetSize, targetSize) && !m_zoomed)
                    OnZoom(m_targetSize / targetSize, 0.25f);
            }).SetEase(Ease.Linear);
        }
        else
        {
            m_tweener = DOTween.To(() => size, x => size = x, targetSize, time).OnUpdate(() =>
            {
                UpdateParallax(size);
            }).SetEase(Ease.Linear);
        }
    }

    private void OnUpdateTargetY(float y)
    {
        m_targetY = y;
    }

    private void OnUpdateTarget(Vector3 targetPos)
    {
        m_targetY = targetPos.y;
        StartCoroutine(DelayRevive());
    }

    IEnumerator DelayRevive()
    {
        yield return new WaitForSeconds(0.5f);
        if(MainModel.gameInfo.savePoint != null)
            UpdateBorder(MainModel.gameInfo.savePoint.Value);
        GameController.BuffHeart();
    }

    private void OnShake()
    {
        m_shaker.m_ImpulseDefinition.m_ImpulseDuration = 0.2f;
        m_shaker.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
        m_shaker.GenerateImpulse();
    }

    private void OnShakeWeak()
    {
        m_shaker.m_ImpulseDefinition.m_ImpulseDuration = 0.2f;
        m_shaker.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
        m_shaker.GenerateImpulse(0.3f);
    }

    private void OnShakeLoop(float time)
    {
        // m_shaker.m_ImpulseDefinition.m_ImpulseDuration = time;
        // m_shaker.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Custom;
        // m_shaker.m_ImpulseDefinition.m_CustomImpulseShape = m_shakeLoopCurve;
        // m_shaker.GenerateImpulse(3);
        m_shakerLoop.Active(true, time);
    }

    private void OnVibrate(float force)
    {
        m_shaker.m_ImpulseDefinition.m_ImpulseDuration = 0.1f;
        m_shaker.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Rumble;
        m_shaker.GenerateImpulse(force);
    }
    private void OnVibrateY()
    {
        m_shaker.m_ImpulseDefinition.m_ImpulseDuration = 0.5f;
        m_shaker.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
        m_shaker.GenerateImpulse(0.5f);
    }

    private void OnVibrateCustom(Vector3 vibration, float duration)
    {
        m_shaker.m_ImpulseDefinition.m_ImpulseDuration = duration;
        m_shaker.GenerateImpulse(vibration);
    }

    private void OnZoom(float ratio, float time)
    {
        m_zoomed = !Mathf.Approximately(1, ratio);
        if (m_tweener != null)
            m_tweener.Kill(false);
        float size = m_camera.m_Lens.OrthographicSize;
        m_targetSize = ratio * m_startSize;
        if (Mathf.Approximately(0, time))
        {
            m_camera.m_Lens.OrthographicSize = m_targetSize;
            UpdateParallax(m_targetSize);
        }
        else
            m_tweener = DOTween.To(() => size, x => size = x, m_targetSize, time).OnUpdate(() =>
            {
                m_camera.m_Lens.OrthographicSize = size;
                UpdateParallax(size);
            }).SetEase(Ease.Linear);
    }

    private void OnFinish(Vector2 startFinishPoint, Vector2 finishPoint)
    {
        m_finishPoint = finishPoint;
        m_stop = true;
    }

    private void LateUpdate()
    {
        if (m_player != null)
            UpdateCameraFollowTargetPosition();
        float cameraSize = m_realCamera.orthographicSize;
        float width = cameraSize * Screen.width / Screen.height;
        Vector3 cameraPos = m_cameraTransform.position;
        for (int i = 0; i < m_parallaxes.Length; i++)
        {
            m_parallaxes[i].Execute(cameraSize, width, cameraPos);
        }
        if (m_fakeBackground != null)
        {
            for (int i = 0; i < m_fakeParallaxes.Length; i++)
            {
                m_fakeParallaxes[i].Execute(cameraSize, width, cameraPos);
            }
        }
        if (m_virtualLiveCamera != m_camera)
            UpdateParallax(cameraSize);
    }

    void UpdateCameraFollowTargetPosition()
    {
        // if (m_brain.IsBlending || m_virtualLiveCamera.Follow != m_cameraFollowTarget)
        //     return;
        Vector2 target = m_player.transform.position;
        float baseY = target.y;
        if (baseY < m_targetY)
            m_targetY = baseY;
        else if ((baseY - m_targetY) >= m_maxForceUpdateY)
            m_targetY = baseY;
        target.y = m_targetY;
        float ratio = m_camera.m_Lens.OrthographicSize / m_baseSize;
        m_cameraFollowTarget.position = Vector3.SmoothDamp(m_cameraFollowTarget.position, target, ref velocity, m_smoothTime * ratio);
    }

    void UpdateParallax(float size)
    {
        m_playerCameraConfiner.InvalidateCache();
        m_currentParallaxSize = size;
        for (int i = 0; i < m_parallaxes.Length; i++)
        {
            if(m_parallaxes[i].fitHeight)
                m_parallaxes[i].UpdateFit(m_currentParallaxSize / m_baseSize);
        }

        if (m_fakeBackground != null)
        {
            for (int i = 0; i < m_fakeParallaxes.Length; i++)
            {
                if(m_fakeParallaxes[i].fitHeight)
                    m_fakeParallaxes[i].UpdateFit(m_currentParallaxSize / m_baseSize);
            }
        }
    }

    void UpdateBorder(Vector3 destination)
    {
        if(m_borders.Count <= 0)
            return;
        Collider2D targetBorder = m_baseBorder;
        foreach(Collider2D border in m_borders)
        {
            if(border.bounds.Contains(destination))
            {
                targetBorder = border;
                break;
            }
        }
        if(targetBorder != m_playerCameraConfiner.m_BoundingShape2D)
        {
            StartCoroutine(IDelayActiveConfiner(targetBorder));
        }
    }

    IEnumerator IDelayActiveConfiner(Collider2D targetBorder)
    {
        yield return null;
        yield return null;
        yield return null;
        m_playerCameraConfiner.enabled = false;
        m_playerCameraConfiner.m_BoundingShape2D = targetBorder;
        yield return null;
        if(m_playerCameraConfiner.m_BoundingShape2D != null)
            m_playerCameraConfiner.enabled = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if(m_baseBorder == null)
        {
            m_playerCameraConfiner = m_camera.GetComponent<CinemachineConfiner2D>();
            m_baseBorder = m_playerCameraConfiner.m_BoundingShape2D;
        }
            
        PolygonCollider2D basePoly = m_baseBorder as PolygonCollider2D;
        for(int i = 0; i < basePoly.points.Length; i++)
        {
            int start = i;
            int end = i + 1;
            if(end == basePoly.points.Length)
                end = 0;
            Gizmos.DrawLine(basePoly.points[start], basePoly.points[end]);
        }

        if(m_borders.Count == 0)
            return;
        foreach(Collider2D collider2D in m_borders)
        {
            PolygonCollider2D borderPoly = collider2D as PolygonCollider2D;
            for(int i = 0; i < borderPoly.points.Length; i++)
            {
                int start = i;
                int end = i + 1;
                if(end == borderPoly.points.Length)
                    end = 0;
                Vector2 offset = collider2D.transform.localPosition;
                Gizmos.DrawLine(borderPoly.points[start] + offset, borderPoly.points[end] + offset);
            }
        }
    }
}
