using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public enum FiveElementsType
{
    None = -1,
    Metal = 0,
    Wood = 1,
    Water = 2,
    Fire = 3,
    Earth = 4,
    Multiple = 5,//混元
}

public class FiveElements
{
    public Dictionary<FiveElementsType, int> baseFiveElements = new Dictionary<FiveElementsType, int>();
    public int multipleValue {
        get
        {
            // 检查所有五个元素是否存在
            if (!baseFiveElements.ContainsKey(FiveElementsType.Metal) ||
                !baseFiveElements.ContainsKey(FiveElementsType.Wood) ||
                !baseFiveElements.ContainsKey(FiveElementsType.Water) ||
                !baseFiveElements.ContainsKey(FiveElementsType.Fire) ||
                !baseFiveElements.ContainsKey(FiveElementsType.Earth))
            {
                return -1; // 如果有任何一个不存在，返回 -1
            }

            // 如果全部存在，计算最小值
            int min = baseFiveElements[FiveElementsType.Metal];
            min = Math.Min(min, baseFiveElements[FiveElementsType.Wood]);
            min = Math.Min(min, baseFiveElements[FiveElementsType.Water]);
            min = Math.Min(min, baseFiveElements[FiveElementsType.Fire]);
            min = Math.Min(min, baseFiveElements[FiveElementsType.Earth]);

            return min;
        }
    }
    public FiveElements(){}
    public FiveElements(int metal, int wood, int water, int fire, int earth)
    {
        baseFiveElements.Add(FiveElementsType.Metal, metal);
        baseFiveElements.Add(FiveElementsType.Wood, wood);
        baseFiveElements.Add(FiveElementsType.Water, water);
        baseFiveElements.Add(FiveElementsType.Fire, fire);
        baseFiveElements.Add(FiveElementsType.Earth, earth);
    }
}

public class CharacterModel: IStorable
{
    public Backpack backpack;
    public FiveElements fiveElements;
    public Attributes attributes;
    public Subject<Unit> characterUpdate = new Subject<Unit>();
    private System.IDisposable equipDisposable;
    private System.IDisposable characterDisposable;
    public CharacterDefine define;

    public List<FeatureDefine> features;

    //BattleItem
    public string uuid;

    //Define
    public int ID { get; set; }
    public JobType Job { get; set; }
    public GeneralLevel Level { get; set; }
    public NameData nameData;
    public string Name 
    { 
        get 
        { 
            return Config.Language == 0 ? nameData.chineseName: nameData.englishName; 
        } 
    }
    public string Race { get; set; }
    public string Resource { get; set; }
    public string Desc { get; set; }
    public int BornSkill { get; set; }
    public int Skill1 { get; set; }
    public int Skill2 { get; set; }
    public int Skill3 { get; set; }

    public CharacterModel()
    {}

    public CharacterModel(string uuid)
    {
        this.uuid = uuid;
    }

    public CharacterModel(CharacterDefine define)
    {
        uuid = GameUtil.Instance.GenerateUniqueId();
        this.define = define;
        ID = define.ID;
        if (GameUtil.Instance.IsMainCharacter(ID)) 
        {
            // nameData = new NameData(DataManager.Instance.LanguagesDic[0][define.Name], DataManager.Instance.LanguagesDic[1][define.Name], Gender.None);
            nameData = new NameData(-1, define.Name, define.Name, Gender.None);
        } else 
        {
            nameData = DataManager.Instance.nameGenerator.GetRandomNameByGender(Gender.Random);
        }
        Race = define.Race;
        Job = define.Job;
        Level = define.Level;
        fiveElements = GameUtil.Instance.GetFiveElements();
        attributes = new Attributes(characterUpdate);
        attributes.Init(define);
        Resource = define.Resource;
        Desc = define.Desc;
        backpack = new Backpack(uuid, 3, 3, characterUpdate);
        //只有被动技能，其他技能升级选择后再赋值
        BornSkill = define.BornSkill;
        Skill1 = -1;
        Skill2 = -1;
        Skill3 = -1;
        characterDisposable = characterUpdate.AsObservable().Subscribe(_ =>
        {
            NorneStore.Instance.Update<CharacterModel>(this, isFull: true);
        });
        equipDisposable = backpack.equipUpdate.AsObservable().Subscribe(_ =>
        {
            ReloadEquipAttr();
        });

        features = new List<FeatureDefine>();
        var randomFeatureList = DataManager.Instance.GetRandomLevelDefine<FeatureDefine>(DataManager.Instance.levelFeatures, 0.5f, 0.35f, 0.15f, 3, false);
        foreach(var feature in randomFeatureList)
        {
            if (feature.Item1)
            {
                features.Add(feature.Item2);
            }
        }
    }

    public string StorableCategory => "CharacterModel";

    public string Identifier => this.uuid.ToString();

    ~CharacterModel()
    {
        if (characterDisposable != null)
        {
            characterDisposable.Dispose();
            characterDisposable = null;
        }
        if (equipDisposable != null)
        {
            equipDisposable.Dispose();
            equipDisposable = null;
        }
    }

    public void InitInvokeSkill() 
    {
        if (DataManager.Instance.Skills.ContainsKey(BornSkill)) 
        {
            var skill = DataManager.Instance.Skills[BornSkill];
            if (skill.InvokeType == SkillInvokeType.instant) 
            {
                SkillManager.Instance.InvokeSkill(uuid, skill.MethodName, skill.PropertyType, skill.Value);
            }
        } else 
        {
            Debug.Log("skill: " + BornSkill + " not found!");
        }
    }

    public void LevelUp() 
    {   
        attributes.level += 1;
        AddMaxPropertyPoints(20);
    }

    public void AddMaxPropertyPoints(int value) 
    {   
        attributes.maxPropertyPoints += value;
        attributes.RemainPropertyPoints += value;
    }

    public void HungryChange(int change)
    {
        if (change < 0)
        {
            //减饱腹度
            var wheatCoin = GameManager.Instance.wheatCoin.Value;
            if (attributes.currentHungry >= Mathf.Abs(change))
            {
                attributes.currentHungry += change;
            } else {
                var remainConsume = Mathf.Abs(change) - attributes.currentHungry;
                attributes.currentHungry = 0;
                if (wheatCoin >= remainConsume)
                {
                    GameManager.Instance.WheatCoinChanged(-remainConsume);
                } else {
                    var remainConsume2 = remainConsume - wheatCoin;
                    GameManager.Instance.WheatCoinChanged(-wheatCoin);
                    GameManager.Instance.CharacterHPChange(uuid, -remainConsume2 * GlobalAccess.hurtPerRemainConsume);
                }
            }
        } else 
        {
            //超出最大值（由前置条件去拦截，战斗效果允许），也不报错
            attributes.currentHungry = Mathf.Min(attributes.MaxHungry, attributes.currentHungry + change);
        }
        GlobalAccess.SaveCharacterModel(this, false);
    }

    public void ReloadEquipAttr()
    {
        attributes.LoadEquipAttributes(backpack.equips);
    }

    public BattleItem ToBattleItem()
    {
        BattleItem item = new BattleItem();
        item.uuid = this.uuid;
        item.type = BattleItemType.player;
        item.nameData = this.nameData;
        item.Race = this.Race;
        item.Job = this.Job;
        item.Level = this.Level;
        item.attributes = GameUtil.Instance.DeepCopy(this.attributes);
        item.attributes.SetUpdateSubject(item.battleItemUpdate);
        item.Resource = this.Resource;
        item.Desc = this.Desc;
        item.backpack = GameUtil.Instance.DeepCopy(this.backpack);
        item.backpack.fatherUpdate = item.battleItemUpdate;
        item.features = this.features;
        item.BornSkill = this.BornSkill;
        item.Skill1 = this.Skill1;
        item.Skill2 = this.Skill2;
        item.Skill3 = this.Skill3;
        item.LoadSkills();
        return item;
    }
}
