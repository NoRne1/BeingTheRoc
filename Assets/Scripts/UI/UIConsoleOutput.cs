using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIConsoleOutput : MonoBehaviour
{
    public TextMeshProUGUI message;
    public void Setup(string text, ConsoleResult result)
    {
        message.text = text;
        switch (result)
        {
            case ConsoleResult.normal:
                message.color = Color.green;
                break;
            case ConsoleResult.error:
                message.color = Color.red;
                break;
        }
    }
}
