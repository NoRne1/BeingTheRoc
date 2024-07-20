using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum TogglePanelType
{
    Panel0,
    Panel1,
    Panel2,
    Panel3,
    Panel4,
}

public class NorneToggle: Toggle
{
    public TogglePanelType toggleType;
}
