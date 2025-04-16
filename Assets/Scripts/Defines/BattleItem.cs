using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System.Linq;

public enum BattleItemSide
{
    none = 0,
    player = 1,
    enemy = 2,
    neutral = 3,
}

public enum BattleItemType
{
    character = 1,
    granary = 2,
    time = 3,
    sceneItem = 4,
    quitTime = 5,
}

public class BattleItem: IStorable
{
    public string uuid { get; set; }
    public NameData nameData;
    public string Name 
    { 
        get 
        { 
            return Config.Language == 0 ? nameData.chineseName: nameData.englishName; 
        } 
    }
    public string Race { get; set; }
    public JobType Job { get; set; }
    public GeneralLevel Level { get; set; }
    public string Resource { get; set; }
    public BattleItemSide side { get; set; }
    public BattleItemType type { get; set; }
    public float remainActingDistance { get; set; }
    public string Desc { get; set; }
    public Attributes attributes;
    public FiveElements fiveElements;
    public Backpack backpack;
    public BuffCenter buffCenter;

    public List<int> skills = new List<int>();
    public int BornSkill { get; set; }
    public int Skill1 { get; set; }
    public int Skill2 { get; set; }
    public int Skill3 { get; set; }

    public List<FeatureDefine> features;

    public int moveAdvancedDistance;
    //public bool isInExtraRound = false;

    public string StorableCategory => "BattleItem";

    public string Identifier => this.uuid.ToString();

    public EnemyAI enemyAI;

    public Subject<Unit> battleItemUpdate = new Subject<Unit>();
    private System.IDisposable updateDisposable;

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

    public bool isPlayer {
        get
        {
            return type == BattleItemType.character && side == BattleItemSide.player;
        }
    }
    public bool isEnemy {
        get
        {
            return type == BattleItemType.character && side == BattleItemSide.enemy;
        }
    }
    public bool isNeutral {
        get
        {
            return type == BattleItemType.character && side == BattleItemSide.neutral;
        }
    }

    public BattleItem()
    {
        updateDisposable = battleItemUpdate.AsObservable().Subscribe(_ =>
        {
            NorneStore.Instance.Update<BattleItem>(this, isFull: true);
        });
    }

    // just for NorneStore
    public BattleItem(string uuid)
    {
        this.uuid = uuid;
    }

    public BattleItem(BattleItemType type)
    {
        switch (type)
        {
            case BattleItemType.character:
                updateDisposable = battleItemUpdate.AsObservable().Subscribe(_ =>
                {
                    NorneStore.Instance.Update<BattleItem>(this, isFull: true);
                });
                this.side = BattleItemSide.none;
                this.type = type;
                break;
            case BattleItemType.sceneItem:
                this.side = BattleItemSide.neutral;
                this.type = type;
                break;
            case BattleItemType.time:
            case BattleItemType.quitTime:
                uuid = GameUtil.Instance.GenerateUniqueId();
                this.side = BattleItemSide.none;
                this.type = type;
                attributes = new Attributes(battleItemUpdate);
                attributes.UpdateInitSpeed(100);
                break;
            case BattleItemType.granary:
                uuid = GameUtil.Instance.GenerateUniqueId();
                this.nameData = new NameData(-1, "粮仓", "Granary", Gender.None);
                this.Resource = "granary_icon";
                this.side = BattleItemSide.none;
                this.type = type;
                attributes = new Attributes(battleItemUpdate);
                attributes.UpdateInitSpeed(0);
                break;
        }
    }

    ~BattleItem()
    {
        if (updateDisposable != null)
        {
            updateDisposable.Dispose();
            updateDisposable = null;
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

    public void HungryChange(int change)
    {   
        if(change < 0)
        {
            var wheatCoin = GameManager.Instance.wheatCoin.Value;
            if (attributes.currentHungry >= Mathf.Abs(change))
            {
                attributes.currentHungry += change;
            } else {
                var remainConsume = Mathf.Abs(change) - attributes.currentHungry;
                attributes.currentHungry = 0;
                switch (side)
                {
                    case BattleItemSide.player:
                        if (wheatCoin >= remainConsume)
                        {
                            GameManager.Instance.WheatCoinChanged(-remainConsume);
                        } else 
                        {
                            var remainConsume2 = remainConsume - wheatCoin;
                            GameManager.Instance.WheatCoinChanged(-wheatCoin);
                            BattleCommonMethods.ProcessDirectAttack("GOD", uuid, 
                                remainConsume2 * GlobalAccess.hurtPerRemainConsume);
                        }
                        break;
                    case BattleItemSide.enemy:
                        BattleCommonMethods.ProcessDirectAttack("GOD", BattleManager.Instance.battleItemManager.enemyGranaryID, 
                            remainConsume * GlobalAccess.hurtPerRemainConsume);
                        break;
                    default:
                        Debug.LogError(type + "type has no HungryChange");
                        break;
                }
            }
        } else 
        {
            //超出最大值（由前置条件去拦截，战斗效果允许），也不报错
            attributes.currentHungry = Mathf.Min(attributes.MaxHungry, attributes.currentHungry + change);
        }
        GlobalAccess.SaveBattleItem(this, false);
    }

    public void BattleInit()
    {
        switch (type)
        {
            case BattleItemType.character:
                buffCenter = new BuffCenter(this.uuid, battleItemUpdate);
                break;
            default:
                break;
        }
        this.attributes.BattleInit();
        // invoke battleStart skill
        foreach (var skillId in skills)
        {
            if (DataManager.Instance.Skills.ContainsKey(skillId))
            {
                var skill = DataManager.Instance.Skills[skillId];
                if (skill.InvokeType == SkillInvokeType.battleStart)
                {
                    SkillManager.Instance.InvokeSkill(uuid, skill.MethodName, skill.PropertyType, skill.Value);
                }
            } else
            {
                Debug.Log("skill: " + skillId + " not found!");
                continue;
            }
        }
    }

    public void RoundBegin()
    {
        if (!BattleManager.Instance.roundManager.isInExtraRound)
        {
            buffCenter?.RoundBegin();
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
            buffCenter?.RoundEnd();
        }
    }

    public void BattleEnd()
    {
        this.attributes.Buff.Reset();
        this.attributes.LoadFinalAttributes();
        if (isPlayer)
        {
            var cm = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(uuid)).Value;
            cm.attributes.currentHungry = this.attributes.currentHungry;
            cm.attributes.currentHP = this.attributes.currentHP;
            NorneStore.Instance.Update(cm, true);
        }
    }

    public int GetFiveElementValue(FiveElementsType type)
    {
        switch(type)
        {
            case FiveElementsType.Metal:
            case FiveElementsType.Wood:
            case FiveElementsType.Water:
            case FiveElementsType.Fire:
            case FiveElementsType.Earth:
                return fiveElements.baseFiveElements[type];
            case FiveElementsType.Multiple:
                return fiveElements.multipleValue;
            default:
                Debug.LogError("Five Element Type Error");
                return 0;
        }
    }
}

