using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class STMovingPlatformPreview
{
    static public STMovingPlatformPreview s_Preview = null;
    static public GameObject preview;

    static protected STMovingPlatform movingPlatform;

    static STMovingPlatformPreview()
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

    static public void CreateNewPreview(STMovingPlatform origin)
    {
        if(preview != null)
        {
            Object.DestroyImmediate(preview);
        }

        movingPlatform = origin; 

        preview = Object.Instantiate(origin.gameObject);
        StageUtility.PlaceGameObjectInCurrentStage(preview);
        preview.hideFlags = HideFlags.DontSave;
        STMovingPlatform plt = preview.GetComponentInChildren<STMovingPlatform>();
        Object.DestroyImmediate(plt);


        // Color c = new Color(0.2f, 0.2f, 0.2f, 0.4f);
        // SpriteRenderer[] rends = preview.GetComponentsInChildren<SpriteRenderer>();
        // for (int i = 0; i < rends.Length; ++i)
        //     rends[i].color = c;
    }
}
