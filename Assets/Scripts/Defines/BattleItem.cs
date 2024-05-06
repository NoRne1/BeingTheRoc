using System;
using UnityEngine;

public enum BattleItemType
{
    player = 0,
    enemy = 1,
    time = 2,
    sceneItem = 3
}
public interface BattleItem
{
    public string uuID { get; }
    public string Name { get; set; }
    public string Resource { get; set; }
    public BattleItemType battleItemType { get; set; }
    public int remainActingTime { get; set; }
    public int Mobility { get; set; }
    public int Speed { get; set; }
    public int MaxHP { get; set; }
    public int Strength { get; set; }
    public int Defense { get; set; }
    public int Dodge { get; set; }
    public int Accuracy { get; set; }
    public int Energy { get; set; }
    public int Lucky { get; set; }
    public string Desc { get; set; }
}

