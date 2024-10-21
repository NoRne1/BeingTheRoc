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
    private System.IDisposable disposable;
    public CharacterDefine define;
    public Attributes attributes;

    //BattleItem
    public string uuid;

    //Define
    public int ID { get; set; }
    public JobType Job { get; set; }
    public GeneralLevel Level { get; set; }
    public string Name { get; set; }
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
        Name = define.Name;
        Level = define.Level;
        attributes = new Attributes(enermyUpdate);
        attributes.Init(define);
        Resource = define.Resource + "_enemy";
        Desc = define.Desc;
        backpack = new Backpack(uuid, 3, 3, enermyUpdate);
        //只有被动技能，其他技能升级选择后再赋值
        BornSkill = define.BornSkill;
        Skill1 = -1;
        Skill2 = -1;
        Skill3 = -1;
        disposable = enermyUpdate.AsObservable().Subscribe(_ =>
        {
            NorneStore.Instance.Update<EnermyModel>(this, isFull: true);
        });
    }

    public string StorableCategory => "EnermyModel";

    public string Identifier => this.uuid.ToString();

    ~EnermyModel()
    {
        if (disposable != null)
        {
            disposable.Dispose();
            disposable = null;
        }
    }

    public BattleItem ToBattleItem(float difficulty)
    {
        BattleItem item = new BattleItem();
        item.uuid = this.uuid;
        item.type = BattleItemType.enemy;
        item.Name = this.Name;
        item.Job = this.Job;
        item.Level = this.Level;
        item.attributes = GameUtil.Instance.DeepCopy(this.attributes);
        item.attributes.SetUpdateSubject(item.battleItemUpdate);

        item.attributes.Difficulty.MaxHP = (int)(this.attributes.MaxHP * (difficulty - 1));
        item.attributes.Difficulty.Strength = (int)(this.attributes.Strength * (difficulty - 1));
        item.attributes.Difficulty.Defense = (int)(this.attributes.Defense * (difficulty - 1));

        item.Resource = this.Resource;
        item.Desc = this.Desc;
        item.backpack = GameUtil.Instance.DeepCopy(this.backpack);
        item.backpack.fatherUpdate = item.battleItemUpdate;
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

