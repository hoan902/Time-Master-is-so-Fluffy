using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class ObjectNote : MonoBehaviour
{
    private void Start() 
    {
        gameObject.SetActive(false);
    }

    public void UpdateText(string text)
    {
        GetComponent<TextMeshPro>().text = text;
    }
    public void UpdateText(string[] texts)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach(string text in texts)
        {
            stringBuilder.Append(text);
            stringBuilder.Append("\n");
        }
        GetComponent<TextMeshPro>().text = stringBuilder.ToString();
    }
}
