using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupUpdate : MonoBehaviour
{
    public void UpdateOnclick()
    {
        MainController.ClosePopup(PopupType.Update);
        MainController.ActiveLoading(true, 0);
        SystemController.DoUpdate();
    }

    public void CloseOnclick()
    {
        SystemController.CancelUpdate();
        MainController.ClosePopup(PopupType.Update);        
    }
}
