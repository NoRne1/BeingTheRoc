using System;
using System.Xml.Linq;
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
    public string Desc { get; set; }
    public Attributes attributes;
    public Backpack backpack;
    public BuffCenter buffCenter;
    public bool isInvisible;
    public bool canActing = true;

    public int BornSkill { get; set; }
    public int Skill1 { get; set; }
    public int Skill2 { get; set; }
    public int Skill3 { get; set; }

    public int moveAdvancedDistance;
    public bool isInExtraRound = false;

    public string StorableCategory => "BattleItem";

    public string Identifier => this.uuid.ToString();

    public EnemyAI enemyAI;

    // Subject
    public Subject<Unit> defeatSubject = new Subject<Unit>();

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
                attributes = new Attributes();
                attributes.UpdateInitSpeed(100);
                break;
        }
    }

    public BattleItem(string uuid)
    {
        this.uuid = uuid;
    }

    public void BattleEnd()
    {
        this.attributes.Buff.Reset();
        this.attributes.LoadFinalAttributes();
    }
}

