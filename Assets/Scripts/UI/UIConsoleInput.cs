using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIConsoleInput : MonoBehaviour
{
    public Image bg;
    public TextMeshProUGUI message;
    public void Setup(string text, ConsoleResult result)
    {
        message.text = text;
        switch (result)
        {
            case ConsoleResult.normal:
                bg.color = Color.white;
                message.color = Color.white;
                break;
            case ConsoleResult.error:
                bg.color = Color.black;
                message.color = Color.black;
                break;
        }
    }
}
