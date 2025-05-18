using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STObjectCoinPhysicsSpawner : MonoBehaviour
{
    [SerializeField] private string m_key = "";
    [Range(0, 100)]
    [SerializeField] private int m_valueCoins = 100;
    [SerializeField] private int m_maxDrop = 1;
    [SerializeField] private int m_numberCoin = 10;
    [SerializeField] private bool m_autoCollect = false;
    [SerializeField] private GameObject m_coinObject;
    [SerializeField] private BoxCollider2D m_collider;
    [SerializeField] private AudioClip m_audioCollect;

    private List<GameObject> m_coins;
    private Transform m_player;
    private bool m_activeCollect;
    private bool m_dropped = false;
    private int m_dropCounter;

    private void Awake() 
    {
        m_collider.enabled = false;
        GameController.triggerEvent += OnTrigger;
    }
    private void OnDestroy() 
    {
        GameController.triggerEvent -= OnTrigger;
    }

    void OnTrigger(string key, bool toActive, GameObject triggerSource)
    {
        if(m_key != key || !toActive || m_dropped)
            return;
        m_dropCounter++;
        m_dropped = m_dropCounter >= m_maxDrop;
        m_collider.enabled = true;
        StartCoroutine(ICreate());
        m_player = FindObjectOfType<STPlayerController>().transform;
    }

    private void Update()
    {
        if (!m_autoCollect || m_player == null || !m_activeCollect)
            return;
        foreach (GameObject go in m_coins)
        {
            if (go == null)
                continue;
            go.transform.position = Vector3.Lerp(go.transform.position, m_player.position, 0.2f);
        }
    }

    IEnumerator ICreate()
    {
        m_coins = new List<GameObject>();
        for(int i = 0; i < m_numberCoin; i++)
        {
            GameObject go = Instantiate(m_coinObject);
            go.transform.SetParent(transform.parent, false);
            Vector2 pos = Vector2.zero;
            float halfX = m_collider.size.x / 2;
            float halfY = m_collider.size.y / 2;
            pos.x = transform.position.x + Random.Range(-halfX, halfX);
            pos.y = transform.position.y + Random.Range(0, halfY);
            go.transform.position = pos;
            go.SetActive(true);
            m_coins.Add(go);
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        
        m_collider.enabled = false;
        if(m_autoCollect)
        {
            yield return new WaitForSeconds(2f);
            m_activeCollect = true;
            foreach (GameObject go in m_coins)
            {
                if (go == null)
                    continue;
                Destroy(go.GetComponent<Rigidbody2D>());
                //go.GetComponent<Collider2D>().isTrigger = true;
            }
        }
    }

    private void OnCoinTriggerEnter(CollisionData data)
    {
        Collider2D collision = data.data as Collider2D;
        if (collision.gameObject.tag != GameTag.PLAYER)
            return;
        SoundManager.PlaySound(m_audioCollect, false);
        GameObject coinObject = data.sender;
        Transform effect = coinObject.transform.Find("effect-collect-coin");
        if(effect == null)
            return;
        effect.gameObject.SetActive(true);
        effect.SetParent(transform.parent, true);
        Destroy(effect.gameObject, 0.5f);
        CoinSingleEffect text = coinObject.transform.Find("text-coins-game").GetComponent<CoinSingleEffect>();
        text.Init(m_valueCoins/MapConstant.COIN_RATIO, coinObject.transform.position, transform.parent);
        Destroy(coinObject);
        m_coins.Remove(coinObject);
    }

    public void UpdateCoin(int coin)
    {
        m_numberCoin = coin / m_valueCoins;
    }
    public void UpdateKey(string key)
    {
        m_key = key;
    }
}
