using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MovingPlatformByKeyPreview 
{
    static public MovingPlatformByKeyPreview s_Preview = null;
    static public GameObject preview;

    static protected MovingPlatformByKey movingPlatform;

    static MovingPlatformByKeyPreview()
    {
        Selection.selectionChanged += SelectionChanged;
    }

    static void SelectionChanged()
    {
        if (movingPlatform != null && Selection.activeGameObject != movingPlatform.gameObject)
        {
            DestroyPreview();
        }
    }

    static public void DestroyPreview()
    {
        if (preview == null)
            return;

        Object.DestroyImmediate(preview);
        preview = null;
        movingPlatform = null;
    }

    static public void CreateNewPreview(MovingPlatformByKey origin)
    {
        if(preview != null)
        {
            Object.DestroyImmediate(preview);
        }

        movingPlatform = origin; 

        preview = Object.Instantiate(origin.gameObject);
        StageUtility.PlaceGameObjectInCurrentStage(preview);
        preview.hideFlags = HideFlags.DontSave;
        MovingPlatformByKey plt = preview.GetComponentInChildren<MovingPlatformByKey>();
        Object.DestroyImmediate(plt);
    }
}
