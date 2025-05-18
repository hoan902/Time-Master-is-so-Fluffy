using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    public enum SpritePivot
    {
        Top,
        Bot,
        Center
    }

    [SerializeField] private float m_parallaxEffect = 1;
    [SerializeField] private SpritePivot m_pivot = SpritePivot.Center;
    public bool fitHeight = false;
    
    [HideInInspector]
    public SpriteRenderer[] renderers;

    private float m_length;
    private float m_startPosX;
    private float m_axisZ;
    private Vector2 m_baseScale;
    private SpriteRenderer m_renderer;
    private float m_lastCameraX;

    void Awake()
    {
        m_baseScale = transform.localScale;
    }

    public void Init(Vector2 cameraPosition)
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        m_startPosX = 0;
        m_axisZ = transform.position.z;
        m_length = renderers[0].bounds.size.x * 3f; 
        m_lastCameraX = cameraPosition.x - 2;
        //init first
    }
  
    public bool Execute(float cameraSize, float screenWidth, Vector3 cameraPosition)
    {
        float xCamera = cameraPosition.x;
        float temp = xCamera*(1-m_parallaxEffect);
        float dist = (xCamera*m_parallaxEffect);
        float x = m_startPosX + dist;
        float y = 0;
        switch (m_pivot)
        {
            case SpritePivot.Center:
                y = cameraPosition.y;
                break;
            case SpritePivot.Bot:
                y = cameraPosition.y - cameraSize;
                break;
            case SpritePivot.Top:
                y = cameraPosition.y + cameraSize;
                break;
        }
        Vector3 target = new Vector3(x, y, cameraPosition.z + 1);
        transform.position = target;
        if (temp > (m_startPosX + screenWidth) && (Mathf.Abs(xCamera - m_lastCameraX) > 1))
        {
            m_startPosX += m_length;
            if(temp <= (m_startPosX + screenWidth))
                m_lastCameraX = xCamera;
            return true;
        }
        else if(temp < (m_startPosX - screenWidth) && (Mathf.Abs(xCamera - m_lastCameraX) > 1))
        {
            m_startPosX -= m_length;
            if(temp >= (m_startPosX - screenWidth))
                m_lastCameraX = xCamera;
            return true;
        }

        return false;
    }

    public void UpdateFit(float ratio)
    {
        transform.localScale = new Vector3(m_baseScale.x, ratio*m_baseScale.y, 1);
    }

    public void SetAlpha(float targetAlpha)
    {
        Color tempColor = Color.white;
        tempColor.a = targetAlpha;
        foreach (SpriteRenderer spriteRenderer in renderers)
        {
            spriteRenderer.color = tempColor;
        }
    }

    public void SetColor(Color c)
    {
        foreach (SpriteRenderer spriteRenderer in renderers)
        {
            spriteRenderer.color = c;
        }
    }
}
