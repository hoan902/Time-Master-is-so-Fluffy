using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "button-style-config", menuName = "Mgif/ButtonStyleConfig", order = 1)]
public class CutsceneButtonStyleConfig : ScriptableObject
{
    public ButtonStyle[] styles;
}
[System.Serializable]
public class ButtonStyle
{
    public Sprite buttonOkSprite;
    public Sprite processSprite;
    public Color color;
}
