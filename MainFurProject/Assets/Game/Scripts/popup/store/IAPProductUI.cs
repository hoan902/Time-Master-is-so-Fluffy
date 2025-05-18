
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPProductUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_text;

    public void Init(Product product)
    {
        if(product == null)
        {
            m_text.text = "0";
            return;
        }
        m_text.text = product.metadata.localizedPriceString;
    }
}
