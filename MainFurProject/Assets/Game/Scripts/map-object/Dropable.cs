using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dropable : MonoBehaviour
{
    public string keyTrigger = "";
    public string secondKeyTrigger = "";
    public bool hasAds = true;
    public GameObject? collector;

    public void SetKey(string key, string secondKey)
    {
        keyTrigger = key;
        secondKeyTrigger = secondKey;
    }
}
