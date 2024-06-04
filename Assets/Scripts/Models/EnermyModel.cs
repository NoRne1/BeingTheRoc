using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class EnermyModel : CharacterDefine, IStorable
{
    public int level = 0;
    public int remainExp = 0;
    public Backpack backpack;

    public Subject<bool> enermyUpdate = new Subject<bool>();
    private System.IDisposable disposable;
    public CharacterDefine define;

    public string uuid;

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
        MaxHP = define.MaxHP;
        Strength = define.Strength;
        Defense = define.Defense;
        Dodge = define.Dodge;
        Accuracy = define.Accuracy;
        Speed = define.Speed;
        Mobility = define.Mobility;
        Energy = define.Energy;
        Lucky = define.Lucky;
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
        item.battleItemType = BattleItemType.enemy;
        item.Name = this.Name;
        item.MaxHP = (int)(this.MaxHP * difficulty);
        item.Strength = (int)(this.Strength * difficulty);
        item.Defense = (int)(this.Defense * difficulty);
        item.Dodge = this.Dodge;
        item.Accuracy = this.Accuracy;
        item.Speed = this.Speed;
        item.Mobility = this.Mobility;
        item.Energy = this.Energy;
        item.Lucky = this.Lucky;
        item.Resource = this.Resource;
        item.Desc = this.Desc;
        item.currentHP = item.MaxHP;
        item.currentEnergy = item.Energy;
        item.level = this.level;
        item.backpack = this.backpack;
        item.BornSkill = this.BornSkill;
        item.Skill1 = difficulty > 1.5 ? define.Skill1 : -1;
        item.Skill2 = difficulty > 2.5 ? define.Skill2 : -1;
        item.Skill3 = difficulty > 4.5 ? define.Skill3 : -1;
        return item;
    }
}

