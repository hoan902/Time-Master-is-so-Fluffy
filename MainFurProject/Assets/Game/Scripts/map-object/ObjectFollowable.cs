using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectFollowable : MonoBehaviour
{
    public int index;
    public bool collected;
    public static int followableObjectsCollectedCount;
    public static Action<GameObject> followableObjectCollectEvent;
}
