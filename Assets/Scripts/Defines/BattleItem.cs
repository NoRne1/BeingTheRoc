using System;
using UniRx;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public enum BattleItemType
{
    player = 0,
    enemy = 1,
    time = 2,
    sceneItem = 3
}

public class BattleItem: IStorable
{
    public string uuid { get; set; }
    public string Name { get; set; }
    public string Resource { get; set; }
    public BattleItemType battleItemType { get; set; }
    public float remainActingDistance { get; set; }
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
    public int currentHP;
    public int currentEnergy;
    public int shield;
    public int level;
    public int remainExp { get { return exp - GlobalAccess.levelUpExp * level; } }
    public int exp;
    public Backpack backpack;
    public BuffCenter buffCenter;

    public int BornSkill { get; set; }
    public int Skill1 { get; set; }
    public int Skill2 { get; set; }
    public int Skill3 { get; set; }

    public string StorableCategory => "BattleItem";

    public string Identifier => this.uuid.ToString();

    public EnemyAI enemyAI;

    public BattleItem() {}

    public BattleItem(BattleItemType type) {
        switch (type)
        {
            case BattleItemType.player:
            case BattleItemType.enemy:
            case BattleItemType.sceneItem:
                battleItemType = type;
                break;
            case BattleItemType.time:
                uuid = GameUtil.Instance.GenerateUniqueId();
                battleItemType = type;
                Speed = 100;
                break;
        }
    }

    public BattleItem(string uuid)
    {
        this.uuid = uuid;
    }
}

