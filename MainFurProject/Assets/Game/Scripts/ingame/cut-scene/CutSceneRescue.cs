using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneRescue : MonoBehaviour
{
    [Range(1, 150)]
    [SerializeField] private int m_skin;

    public void OnRescue()
    {
        // GameController.SaveBall(m_skin);
    }
}
