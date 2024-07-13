using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System.Linq;

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

    public List<int> skills = new List<int>();
    public int BornSkill { get; set; }
    public int Skill1 { get; set; }
    public int Skill2 { get; set; }
    public int Skill3 { get; set; }

    public int moveAdvancedDistance;
    //public bool isInExtraRound = false;

    public string StorableCategory => "BattleItem";

    public string Identifier => this.uuid.ToString();

    public EnemyAI enemyAI;

    // other attribute
    public bool isInvisible = false;
    public bool canActing = true;
    public bool haveAttackedInRound = false;
    public bool isInvincible = false;
    public bool avoidDeath = false;
    public bool reinforceDefense = false;
    public bool isSilent = false;
    public bool isConfine = false;
    public Action<string> avoidDeathFunc;
    // Subjects
    public Subject<Unit> defeatSubject = new Subject<Unit>();
    public Subject<Vector2> moveSubject = new Subject<Vector2>();
    public Subject<Unit> lastEnergyAttackSubject = new Subject<Unit>();

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

    public void LoadSkills()
    {
        skills.Clear();
        skills.Add(BornSkill);
        skills.Add(Skill1);
        skills.Add(Skill2);
        skills.Add(Skill3);
        skills = skills.Where(id => id != -1).ToList();
    }

    public BattleItem(string uuid)
    {
        this.uuid = uuid;
    }

    public void BattleInit()
    {
        buffCenter = new BuffCenter(this.uuid);
        this.attributes.BattleInit();
    }

    public void RoundBegin()
    {
        if (!BattleManager.Instance.roundManager.isInExtraRound)
        {
            buffCenter.RoundBegin();
        }
        if (reinforceDefense)
        {
            attributes.currentShield = Mathf.Max(0, attributes.currentShield - 10);
        } else
        {
            attributes.currentShield = (int)(attributes.currentShield / 2.0f);
        }
        attributes.currentEnergy = attributes.Energy;
        haveAttackedInRound = false;
    }

    public void RoundEnd()
    {
        if (!BattleManager.Instance.roundManager.isInExtraRound)
        {
            buffCenter.RoundEnd();
        }
    }

    public void BattleEnd()
    {
        this.attributes.Buff.Reset();
        this.attributes.LoadFinalAttributes();
    }
}

