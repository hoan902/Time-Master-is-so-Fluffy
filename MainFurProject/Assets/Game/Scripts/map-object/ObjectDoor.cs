using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ObjectDoor : MonoBehaviour
{
    [SerializeField] private string m_key = "";
    [SerializeField] private float m_time = 1;
    [SerializeField] private float m_distance = 4;
    [SerializeField] private bool m_activeCamera;
    [SerializeField] private SpriteMaskInteraction m_maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    [SerializeField] private Transform m_door;
    [SerializeField] private BoxCollider2D m_collider;
    [SerializeField] private SpriteRenderer m_sprite;
    [SerializeField] private GameObject m_virtualCamera;
    [SerializeField] private bool m_openFirst = false;
    [SerializeField] private AudioClip m_audioOpen;//object-door-switch

    // private Vector2 m_starPos;
    private bool m_state;
    private GameObject m_sound;
    private bool m_first;

    void Awake()
    {
        m_state = m_openFirst;
        GameController.triggerEvent += OnTrigger;
        // m_starPos = m_door.localPosition;
        m_door.localPosition = new Vector2(m_door.localPosition.x, m_openFirst ? m_distance : 0);
        m_sprite.maskInteraction = m_maskInteraction;
    }

    void OnDestroy()
    {
        GameController.triggerEvent -= OnTrigger;
        m_door.DOKill();
    }

    private void OnTrigger(string key, bool state, GameObject triggerSource)
    {
        if(m_key != key || !gameObject.activeSelf)
            return;        
        m_state = !m_state;
        if(m_state)
            Open();
        else
            Close();

        if(!m_first && m_activeCamera)
            StartCoroutine(IShow());
    }

    private void Open()
    {
        if(m_sound == null)
            m_sound = SoundManager.PlaySound3D(m_audioOpen, 10, true, transform.position);
        StartCoroutine(IOpen());
    }

    private void Close()
    {
        if(m_sound == null)
            m_sound = SoundManager.PlaySound3D(m_audioOpen, 10, true, transform.position);
        m_collider.enabled = true;
        m_door.DOKill();
        m_door.DOLocalMoveY(0, m_time).OnComplete(() =>
        {
            Destroy(m_sound);
        });
    }

    IEnumerator IShow()
    {
        m_first = true;
        GameController.ActiveInput(false);            
        m_virtualCamera.SetActive(true);
        yield return new WaitForSeconds(m_time + 0.5f);
        if(!MainModel.inCutscene)   
            GameController.ActiveInput(true);
        m_virtualCamera.SetActive(false);
    }
    IEnumerator IOpen()
    {
        // if (!m_first && m_activeCamera)
        // {
        //     m_first = true;
        //     GameController.ActiveInput(false);            
        //     m_virtualCamera.SetActive(true);
        //     yield return new WaitForSeconds(0.5f);
        // }
        yield return null;
        m_door.DOKill();
        m_door.DOLocalMoveY(m_distance, m_time).OnComplete(() =>
        {         
            // if(!MainModel.inCutscene)   
            //     GameController.ActiveInput(true);            
            // m_virtualCamera.SetActive(false);
            m_collider.enabled = false;
            Destroy(m_sound);
        });
    }
}
