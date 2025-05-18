using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectTriggerVirtualCamera : MonoBehaviour
{
    [SerializeField] private string m_key = "";
    [SerializeField] private float m_blendTime = 3f;
    [SerializeField] private float m_showTime = 3f;
    [SerializeField] private CinemachineBlendController m_blendController;
    [SerializeField] private GameObject m_virtualCamera;
    [SerializeField] Vector3 m_virtualCameraPosition;

    private bool m_actived;

    private void Start() 
    {
        m_blendController.BlendTime = m_blendTime;

        GameController.triggerEvent += OnTrigger;
    }
    private void OnDestroy() 
    {
        GameController.triggerEvent -= OnTrigger;
    }

    void OnTrigger(string key, bool state, GameObject triggerSource)
    {
        if(key != m_key || !state || m_actived)
            return;
        m_actived = true;   
        StartCoroutine(IShow());
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.tag != GameTag.PLAYER || m_actived || m_key != "")
            return;
        m_actived = true;   
        StartCoroutine(IShow());
    }

    IEnumerator IShow()
    {
        GameController.ActiveInput(false);            
        m_virtualCamera.SetActive(true);
        yield return new WaitForSeconds(m_blendTime + m_showTime);
        if(!MainModel.inCutscene)   
            GameController.ActiveInput(true);
        m_virtualCamera.SetActive(false);
        GameController.UpdateBlendTime(1);
    }

    public void UpdateCamPos(Vector3 des)
    {
        m_virtualCameraPosition = des;
        m_virtualCamera.transform.position = new Vector3(m_virtualCameraPosition.x, m_virtualCameraPosition.y, -5);
    }
    public Vector3 GetCamPos()
    {
        return m_virtualCameraPosition;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(m_virtualCamera.transform.position, 0.5f);    
    }
}
