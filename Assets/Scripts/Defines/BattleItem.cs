using System;

public enum BattleItemType
{
    player = 0,
    enemy = 1,
    time = 2,
    sceneItem = 3
}
public interface BattleItem
{
    public string Resource { get; set; }
    public BattleItemType battleItemType { get; set; }
    public int remainActingTime { get; set; }
}

