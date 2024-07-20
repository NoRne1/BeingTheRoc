using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIPanelSelector: MonoBehaviour
{ 
    // 声明一个事件，用于通知父脚本选中的NorneToggle
    public event Action<NorneToggle> OnToggleValueChanged;

    private List<NorneToggle> toggles = new List<NorneToggle>();

    // Start is called before the first frame update
    void Awake()
    {
        foreach (var toggle in GetComponentsInChildren<NorneToggle>())
        {
            toggles.Add(toggle);
            SetAlpha(toggle);
            // 监听Toggle的onValueChanged事件
            toggle.onValueChanged.AddListener(delegate {
                ToggleValueChanged(toggle);
            });
        }
    }

    void ToggleValueChanged(NorneToggle toggle)
    {
        // 触发事件，通知父脚本
        OnToggleValueChanged?.Invoke(toggle);
        SetAlpha(toggle);
    }

    void SetAlpha(NorneToggle toggle)
    {
        Image image = toggle.GetComponent<Image>();

        if (image != null)
        {
            // 根据Toggle的选中状态设置alpha值
            Color color = image.color;
            color.a = toggle.isOn ? 1f : 0.3f;
            image.color = color;
        }
    }

    public void PanelInit(TogglePanelType type)
    {
        toggles.Where(toggle => toggle.toggleType == type).FirstOrDefault().isOn = true;
    }
}
