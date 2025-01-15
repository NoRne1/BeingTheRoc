using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDialogue
{
    public string name;
    public string content;

    public EventDialogue(){}
}

public enum EventRewardType
{
    wheat = 0,
    hungry = 1,
}

public class EventReward
{
    public EventRewardType type;
    public int id;
    public int num;

    public EventReward(){}
}

public class EventButton
{
    public string title;
    public string desc;
    public EventDialogue dialogue;
    public EventReward reward;

    public EventButton(){}
}

public class EventDefine
{
    public int ID { get; set; }
    public string title { get; set; }
    public EventDialogue dialogue1 { get; set; }
    public EventDialogue dialogue2 { get; set; }
    public EventDialogue dialogue3 { get; set; }
    public EventButton button1 { get; set; }
    public EventButton button2 { get; set; }
    public EventButton button3 { get; set; }
}
