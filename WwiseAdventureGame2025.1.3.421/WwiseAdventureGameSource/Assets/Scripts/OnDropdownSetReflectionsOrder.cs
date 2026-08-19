using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnDropdownSetReflectionsOrder : MonoBehaviour
{
    public bool reflectionOrderHotkeysEnabled = false;
    [Header("UI Objects")]
    public Dropdown dropdown;

    private void Awake()
    {
        if (dropdown == null)
        {
            dropdown = GetComponent<Dropdown>();
        }

        int currentReflectionOrder = (int)AkWwiseInitializationSettings.Instance.AkSpatialAudioInitSettings.uMaxReflectionOrder;
        dropdown.value = currentReflectionOrder;
    }

    public void SetReflectionsOrder(int order) {
        AkUnitySoundEngine.SetReflectionsOrder((uint)order, true);
        print("Wwise: Early Reflections Order set to " + (uint)order);
    }

    public void SetDropdownValue(int element)
    {
        SetReflectionsOrder(element);
    }

    public void SetDropdownValueFromHotkey(int element)
    {
        if (reflectionOrderHotkeysEnabled)
        {
            dropdown.value = element;
            dropdown.RefreshShownValue();
        }
    }

    public void SetHotkeyMode(bool condition)
    {
        reflectionOrderHotkeysEnabled = condition;
    }
}
