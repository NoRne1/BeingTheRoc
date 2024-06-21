using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CharacterModel: IStorable
{
    public Backpack backpack;
    public BuffCenter buffCenter;
    public Attributes attributes;

    public Subject<bool> characterUpdate = new Subject<bool>();
    private System.IDisposable disposable;
    public CharacterDefine define;

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
        Job = define.Job;
        Name = define.Name;
        Level = define.Level;
        attributes = new Attributes();
        attributes.Init(define);
        Resource = define.Resource;
        Desc = define.Desc;
        backpack = new Backpack(uuid, 3, 3, characterUpdate);
        buffCenter = new BuffCenter(uuid);
        //只有被动技能，其他技能升级选择后再赋值
        BornSkill = define.BornSkill;
        Skill1 = -1;
        Skill2 = -1;
        Skill3 = -1;
        disposable = characterUpdate.AsObservable().Subscribe(_ =>
        {
            NorneStore.Instance.Update<CharacterModel>(this, isFull: true);
        });
    }

    public string StorableCategory => "CharacterModel";

    public string Identifier => this.uuid.ToString();

    ~CharacterModel()
    {
        if (disposable != null)
        {
            disposable.Dispose();
            disposable = null;
        }
    }

    public BattleItem ToBattleItem()
    {
        BattleItem item = new BattleItem();
        item.uuid = this.uuid;
        item.battleItemType = BattleItemType.player;
        item.Name = this.Name;
        item.attributes = this.attributes;
        item.Resource = this.Resource;
        item.Desc = this.Desc;
        if (item.attributes.dynamicAttr == null)
        {
            item.attributes.dynamicAttr = new AttributeDynamic();
            item.attributes.dynamicAttr.currentHP = item.attributes.MaxHP;
            item.attributes.dynamicAttr.currentShield = 0;
            item.attributes.dynamicAttr.currentEnergy = 0;
        }
        else
        {
            item.attributes.dynamicAttr.currentHP = item.attributes.MaxHP;
            item.attributes.dynamicAttr.currentShield = 0;
            item.attributes.dynamicAttr.currentEnergy = 0;
        }
        item.backpack = this.backpack;
        item.BornSkill = this.BornSkill;
        item.Skill1 = this.Skill1;
        item.Skill2 = this.Skill2;
        item.Skill3 = this.Skill3;
        item.buffCenter = buffCenter;
        return item;
    }
}
