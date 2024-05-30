using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CharacterModel: CharacterDefine, IStorable
{
    public int currentHP;
    public int currentEnergy;
    public int level = 0;
    public int remainExp { get { return exp - GlobalAccess.levelUpExp * level; } }
    public int exp = 0;
    public Backpack backpack;

    public Subject<bool> characterUpdate = new Subject<bool>();
    private System.IDisposable disposable;
    public CharacterDefine define;

    //BattleItem
    public string uuid;
    

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
        MaxHP = define.MaxHP;
        Strength = define.Strength;
        Defense = define.Defense;
        Dodge = define.Dodge;
        Accuracy = define.Accuracy;
        Speed = define.Speed;
        Mobility = define.Mobility;
        Energy = define.Energy;
        Lucky = define.Lucky;
        Resource = define.Resource;
        Desc = define.Desc;
        backpack = new Backpack(uuid, 3, 3, characterUpdate);
        currentHP = define.MaxHP;
        currentEnergy = define.Energy;
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

    public string StorableCategory => "Character";

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
        item.MaxHP = this.MaxHP;
        item.Strength = this.Strength;
        item.Defense = this.Defense;
        item.Dodge = this.Dodge;
        item.Accuracy = this.Accuracy;
        item.Speed = this.Speed;
        item.Mobility = this.Mobility;
        item.Energy = this.Energy;
        item.Lucky = this.Lucky;
        item.Resource = this.Resource;
        item.Desc = this.Desc;
        item.currentHP = this.currentHP;
        item.currentEnergy = this.currentEnergy;
        item.exp = this.exp;
        item.level = this.level;
        item.backpack = this.backpack;
        item.BornSkill = this.BornSkill;
        item.Skill1 = this.Skill1;
        item.Skill2 = this.Skill2;
        item.Skill3 = this.Skill3;
        return item;
    }
}
