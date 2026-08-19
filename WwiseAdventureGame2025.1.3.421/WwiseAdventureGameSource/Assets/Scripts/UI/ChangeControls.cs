////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2018 Audiokinetic Inc. / All Rights Reserved
//
////////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
 using UnityEditor;

public class ChangeControls : MonoBehaviour
{
    ToggleGroup toggleGroup;
    
    public void ChangeController()
    {
        if (toggleGroup == null)
        {
            toggleGroup = GetComponent<ToggleGroup>();
        }
        
        if (toggleGroup.AnyTogglesOn())
        {
            Toggle tog = toggleGroup.ActiveToggles().FirstOrDefault();

            InputManager.ChangeController((InputManager.ControllerMode)System.Enum.Parse(typeof(InputManager.ControllerMode), tog.gameObject.name));
        }
    }
}
