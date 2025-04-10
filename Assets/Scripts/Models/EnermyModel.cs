using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class EnermyModel: IStorable
{
    public int level = 0;
    public int remainExp = 0;
    public Backpack backpack;

    public Subject<Unit> enermyUpdate = new Subject<Unit>();
    private System.IDisposable equipDisposable;
    private System.IDisposable enermyDisposable;
    public CharacterDefine define;
    public List<FeatureDefine> features;
    public Attributes attributes;
    public FiveElements fiveElements;

    //是否是支援
    public bool isSupport = false;

    //BattleItem
    public string uuid;

    //Define
    public int ID { get; set; }
    public JobType Job { get; set; }
    public string Race { get; set; }
    public GeneralLevel Level { get; set; }
    public NameData nameData;
    public string Name 
    { 
        get 
        { 
            return Config.Language == 0 ? nameData.chineseName: nameData.englishName; 
        } 
    }
    public string Resource { get; set; }
    public string Desc { get; set; }
    public int BornSkill { get; set; }
    public int Skill1 { get; set; }
    public int Skill2 { get; set; }
    public int Skill3 { get; set; }

    public EnemyAIType aiType;

    public EnermyModel()
    { }

    public EnermyModel(string uuid)
    {
        this.uuid = uuid;
    }

    public EnermyModel(CharacterDefine define)
    {
        uuid = GameUtil.Instance.GenerateUniqueId();
        this.define = define;
        ID = define.ID;
        Job = define.Job;
        Race = define.Race;
        nameData = DataManager.Instance.nameGenerator.GetRandomNameByGender(Gender.Random);
        Level = define.Level;
        attributes = new Attributes(enermyUpdate);
        attributes.Init(define);
        fiveElements = GameUtil.Instance.GetFiveElements();
        Resource = define.Resource + "_enemy";
        Desc = define.Desc;
        backpack = new Backpack(uuid, 3, 3, enermyUpdate);
        //只有被动技能，其他技能升级选择后再赋值
        BornSkill = define.BornSkill;
        Skill1 = -1;
        Skill2 = -1;
        Skill3 = -1;
        enermyDisposable = enermyUpdate.AsObservable().Subscribe(_ =>
        {
            NorneStore.Instance.Update<EnermyModel>(this, isFull: true);
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

    public string StorableCategory => "EnermyModel";

    public string Identifier => this.uuid.ToString();

    ~EnermyModel()
    {
        if (enermyDisposable != null)
        {
            enermyDisposable.Dispose();
            enermyDisposable = null;
        }
        if (equipDisposable != null)
        {
            equipDisposable.Dispose();
            equipDisposable = null;
        }
    }

    public void ReloadEquipAttr()
    {
        attributes.LoadEquipAttributes(backpack.equips);
    }

    public BattleItem ToBattleItem(float difficulty)
    {
        BattleItem item = new BattleItem();
        item.uuid = this.uuid;
        item.type = BattleItemType.enemy;
        item.nameData = this.nameData;
        item.Job = this.Job;
        item.Race = this.Race;
        item.Level = this.Level;
        item.attributes = GameUtil.Instance.DeepCopy(this.attributes);
        item.attributes.SetUpdateSubject(item.battleItemUpdate);
        item.fiveElements = GameUtil.Instance.DeepCopy(this.fiveElements);

        item.attributes.Difficulty.MaxHP = (int)(this.attributes.MaxHP * (difficulty - 1));
        item.attributes.Difficulty.Strength = (int)(this.attributes.Strength * (difficulty - 1));
        item.attributes.Difficulty.Magic = (int)(this.attributes.Magic * (difficulty - 1));

        item.Resource = this.Resource;
        item.Desc = this.Desc;
        item.backpack = GameUtil.Instance.DeepCopy(this.backpack);
        item.backpack.fatherUpdate = item.battleItemUpdate;
        item.features = this.features;
        item.BornSkill = this.BornSkill;
        item.Skill1 = difficulty > 1.5 ? define.Skill1 : -1;
        item.Skill2 = difficulty > 2.5 ? define.Skill2 : -1;
        item.Skill3 = difficulty > 4.5 ? define.Skill3 : -1;
        item.LoadSkills();
        switch (aiType)
        {
            case EnemyAIType.TankAI:
                item.enemyAI = new TankAI();
                break;
            case EnemyAIType.WarriorAI:
                item.enemyAI = new WarriorAI();
                break;
            case EnemyAIType.MagicianAI:
                item.enemyAI = new MagicianAI();
                break;
            case EnemyAIType.AssassinAI:
                item.enemyAI = new AssassinAI();
                break;
            case EnemyAIType.PastorAI:
                item.enemyAI = new PastorAI();
                break;
        }
        return item;
    }
}

