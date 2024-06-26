using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UniRx;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.RuleTile.TilingRuleOutput;

[Flags]
public enum BattleItemType
{
    player = 1,
    enemy = 2,
    time = 4,
    sceneItem = 8
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
    //public bool isInExtraRound = false;

    public string StorableCategory => "BattleItem";

    public string Identifier => this.uuid.ToString();

    public EnemyAI enemyAI;

    // Subjects
    public Subject<Unit> defeatSubject = new Subject<Unit>();
    public Subject<Vector2> moveSubject = new Subject<Vector2>();
    public Subject<List<string>> hurtSubject = new Subject<List<string>>();

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

    public void RoundBegin()
    {
        if (!BattleManager.Instance.isInExtraRound)
        {
            buffCenter.RoundBegin();
        }
        attributes.currentShield = (int)(attributes.currentShield / 2.0f);
        attributes.currentEnergy = attributes.Energy;
    }

    public void BattleEnd()
    {
        this.attributes.Buff.Reset();
        this.attributes.LoadFinalAttributes();
    }
}

