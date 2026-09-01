using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class MusicManagerWwise : MonoBehaviour
{
    // Declare Wwise states
    public AK.Wwise.State Level101;
    public AK.Wwise.State Level102;

    // Method to set Level101 state
    public void SetLevel101State()
    {
        Level101.SetValue();
    }

    // Method to set Level102 state (if needed)
    public void SetLevel102State()
    {
       Level102.SetValue();
    }
}