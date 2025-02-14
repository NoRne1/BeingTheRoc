using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MultiSelectToggleGroup : MonoBehaviour
{
    public List<Toggle> toggles;
    public int maxSelections = 3;

    void Start()
    {
        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.AddListener(delegate { OnToggleValueChanged(toggle); });
        }
    }

    void OnToggleValueChanged(Toggle changedToggle)
    {
        var activeToggles = GetActiveToggles();
        if (activeToggles.Count > maxSelections)
        {
            // Deselect the last toggle that was selected
            changedToggle.isOn = false;
        }
    }

    public List<Toggle> GetActiveToggles()
    {
        return toggles.Where(toggle => toggle.isOn).ToList();
    }
}