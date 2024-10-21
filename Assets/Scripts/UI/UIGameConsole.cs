using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public enum ConsoleResult
{
    normal = 0,
    error = 1
}

public class UIGameConsole : UIWindow
{
    public TMP_InputField inputField;
    public ScrollRect scrollRect;
    public Transform consoleDisplay;
    private RectTransform displayContent;
    public GameObject inputTemplate;
    public GameObject outputTemplate;

    void Start()
    {
        //选择输入字段
        inputField.Select();
        //激活输入字段，使其准备好接受输入
        inputField.ActivateInputField();

        displayContent = consoleDisplay.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendCommand();
        }
    }

    void AddInput(string input, ConsoleResult result = ConsoleResult.normal)
    {
        UIConsoleInput newInput = Instantiate(inputTemplate, consoleDisplay).GetComponent<UIConsoleInput>();
        newInput.Setup(input, result);
        LayoutRebuilder.ForceRebuildLayoutImmediate(displayContent);
    }

    void AddOutput(string output, ConsoleResult result = ConsoleResult.normal)
    {
        UIConsoleOutput newOutput = Instantiate(outputTemplate, consoleDisplay).GetComponent<UIConsoleOutput>();
        newOutput.Setup(output, result);
        LayoutRebuilder.ForceRebuildLayoutImmediate(displayContent);
    }

    IEnumerator ForceScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        // Wait for end of frame AND force update all canvases before setting to bottom.
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    public void SendCommand()
    {
        if (inputField.text.Trim() != "")
        {
            string command = inputField.text.Trim();
            AddInput(command);
            ExecuteCommand(command);
            inputField.text = "";

            // 再次激活输入字段，使其准备好接受输入
            inputField.ActivateInputField();
        }
    }

    void ExecuteCommand(string command)
    {
        string[] args = command.Split(' ');

        switch (args[0].ToLower())
        {
            case "get":
                if (args.Length >= 3)
                {
                    string item = args[1].ToLower();
                    int amount;
                    if (int.TryParse(args[2], out amount))
                    {
                        GetItem(item, amount);
                    }
                    else
                    {
                        AddOutput("Invalid amount.", ConsoleResult.error);
                    }
                }
                else
                {
                    AddOutput("Invalid command format.", ConsoleResult.error);
                }
                break;
            case "help":
                if (args.Length >= 2)
                {
                    string commandString = args[1].ToLower();
                    switch (commandString)
                    {
                        case "-get":
                            AddOutput("command supported: get money 'amount', get item 'id'");
                            break;
                        default:
                            AddOutput("Unknown command.Can not help.", ConsoleResult.error);
                            break;
                    }
                } else
                {
                    AddOutput("command supported: get, enter help -'command' to get more info.");
                }
                break;
            default:
                AddOutput("Unknown command.", ConsoleResult.error);
                break;
        }
        StartCoroutine(ForceScrollToBottom());
    }

    void GetItem(string item, int value)
    {
        // 这里你可以添加实际游戏中获得道具的逻辑
        if (item == "money")
        {
            GameManager.Instance.FeatherCoinChanged(value);
            AddOutput("Added " + value + " money.");
        }
        else if (item == "item")
        {
            if (DataManager.Instance.StoreItems.ContainsKey(value))
            {
                GameManager.Instance.repository.AddItem(new StoreItemModel(DataManager.Instance.StoreItems[value]));
                AddOutput("Added " + value + " items.");
            } else
            {
                AddOutput("item " + value + " not found.", ConsoleResult.error);
            }
        }
        else
        {
            AddOutput("Unknown item.", ConsoleResult.error);
        }
    }
}
