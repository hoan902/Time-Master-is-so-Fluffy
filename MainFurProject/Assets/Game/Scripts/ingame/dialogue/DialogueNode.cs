using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
[System.Serializable]
public class DialogueNode  : ScriptableObject
{
    [SerializeField] bool isPlayerSpeaking = false;
    [SerializeField] string text;
    [SerializeField] List<string> children = new List<string>();
    [SerializeField] Rect rect = new Rect(0, 0, 200, 100);
    [SerializeField] string onEnterAction;
    [SerializeField] string onExitAction;
    [SerializeField] Condition condition;

    public Rect GetRect()
    {
        return rect;
    }
    public string GetText()
    {
        return text;
    }
    public List<string> GetChildren()
    {
        return children;
    }
    public bool IsPlayerSpeaking()
    {
        return isPlayerSpeaking;
    }
    public string GetOnEnterAction()
    {
        return onEnterAction;
    } 
    public string GetOnExitAction()
    {
        return onExitAction;
    }
    public bool CheckCondition(IEnumerable<IPredicateEvaluator> evaluators)
    {
        return condition.Check(evaluators);
    }
#if UNITY_EDITOR
    public void SetPosition(Vector2 newPosition)
    {
        Undo.RecordObject(this, "Move Dialogue");
        rect.position = newPosition;
        EditorUtility.SetDirty(this);
    }
    public void SetText(string newText)
    {
        if(text != newText)
        {
            Undo.RecordObject(this, "Update Dialogue Text");
            text = newText;
            EditorUtility.SetDirty(this);
        }
    }
    public void AddChild(string childID)
    {
        Undo.RecordObject(this, "Add Dialogue Link");
        children.Add(childID);
        EditorUtility.SetDirty(this);
    }
    public void RemoveChild(string childID)
    {
        Undo.RecordObject(this, "Remove Dialogue Link");
        children.Remove(childID);
        EditorUtility.SetDirty(this);
    }

    internal void SetPlayerSpeaking(bool newIsPlayerSpeaking)
    {
        Undo.RecordObject(this, "Change Dialogue Speaker");
        isPlayerSpeaking = newIsPlayerSpeaking;
        EditorUtility.SetDirty(this);
    }
#endif
}
