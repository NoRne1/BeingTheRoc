using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CharacterModel: CharacterDefine, IStorable
{
    public int currentHP;
    public int currentEnergy;
    public int level { get { return exp / GlobalAccess.levelUpExp + 1; } }
    public int remainExp { get { return exp % GlobalAccess.levelUpExp; } }
    public int exp;
    public Backpack backpack;

    public Subject<bool> characterUpdate = new Subject<bool>();
    private System.IDisposable disposable;
    private CharacterDefine define;

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
        Name = define.Name;
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
        item.backpack = this.backpack;
        return item;
    }
}
