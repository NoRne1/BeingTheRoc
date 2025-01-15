using System.Collections;
using System.Collections.Generic;
using UniRx;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class UIEventPage : UIWindow
{
    public RectTransform dialogueScrollRect;
    public Transform dialogueScrollContent;
    public GameObject dialoguePrefab;

    public RectTransform buttonPanel;
    public Transform buttonScrollContent;
    public GameObject buttonPrefab;

    public GameObject separation;
    public Button nextButton;

    public EventDefine eventDefine;
    public int nextNum = 0;
    public EventReward eventReward;
    // Start is called before the first frame update
    void Start()
    {
        nextButton.OnClickAsObservable().Subscribe(_=>
        {
            NextButtonClick();
        }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(EventDefine eventDefine)
    {
        this.eventDefine = eventDefine;
        dialogueScrollRect.gameObject.SetActive(true);
        buttonPanel.gameObject.SetActive(false);
        separation.SetActive(true);
        if (eventDefine.dialogue1 != null)
        {
            AddDialogue(eventDefine.dialogue1);
        } else {
            Debug.LogError("UIEventPage Setup eventDefine.dialogue1 == null");
        }
    }

    private void NextButtonClick()
    {
        nextNum += 1;
        if (nextNum == 1)
        {
            if (eventDefine.dialogue2 != null)
            {
                AddDialogue(eventDefine.dialogue2);
            } else 
            {
                ShowButtonPanel();
            }
        } else if (nextNum == 2)
        {
            if (eventDefine.dialogue3 != null)
            {
                AddDialogue(eventDefine.dialogue3);
            } else 
            {
                ShowButtonPanel();
            }
        } else if (nextNum == 3)
        {
            ShowButtonPanel();
        } else if (nextNum == 1000)
        {
            //result时，点击next
            if (eventReward != null)
            {
                ProcessEventReward(eventReward);
            }
        } else
        {
            Debug.LogError("NextButtonClick nextNum should not be 4");
        }
    }

    private void ShowButtonPanel()
    {
        dialogueScrollRect.gameObject.SetActive(false);
        buttonPanel.gameObject.SetActive(true);
        separation.SetActive(false);
        if (eventDefine.button1 != null) 
        {
            AddButton(eventDefine.button1);
        }
        if (eventDefine.button2 != null) 
        {
            AddButton(eventDefine.button2);
        }
        if (eventDefine.button3 != null) 
        {
            AddButton(eventDefine.button3);
        }
        nextNum = 999;
    }

    private void AddDialogue(EventDialogue dialogue)
    {
        var dialogueObject = Instantiate(dialoguePrefab, dialogueScrollContent);
        dialogueObject.GetComponent<UIEventDialogue>().Setup(dialogue);
        SetupDialogueRectSize();
    }

    private void AddButton(EventButton buttonInfo)
    {
        var button = Instantiate(buttonPrefab, buttonScrollContent);
        button.GetComponent<UIEventButton>().Setup(buttonInfo);
        button.GetComponent<Button>().OnClickAsObservable().Subscribe(_ =>
        {
            ShowResult(buttonInfo);
        }).AddTo(this);
    }

    private void ShowResult(EventButton buttonInfo)
    {
        GameUtil.Instance.DetachChildren(dialogueScrollContent);
        dialogueScrollRect.gameObject.SetActive(true);
        buttonPanel.gameObject.SetActive(false);
        separation.SetActive(true);
        if (buttonInfo.dialogue != null)
        {
            AddDialogue(buttonInfo.dialogue);
        }
        eventReward = buttonInfo.reward;
    }

    private void ProcessEventReward(EventReward eventReward)
    {
        BlackBarManager.Instance.AddMessage("EventReward: " + eventReward.type.ToString() + " " + eventReward.id.ToString() + " " + eventReward.num.ToString());
        UIManager.Instance.Close<UIEventPage>();
    } 

    private IEnumerator SetupDialogueRectSize()
    {
        // 强制更新布局
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
        // 等待布局调整完成
        yield return new WaitForEndOfFrame();
        // 获取最新尺寸
        var dialogueScrollContentSize = dialogueScrollContent.gameObject.GetComponent<RectTransform>().sizeDelta;
        dialogueScrollRect.sizeDelta = new Vector2(dialogueScrollContentSize.x, Mathf.Min(700, dialogueScrollContentSize.y));
    }
}
