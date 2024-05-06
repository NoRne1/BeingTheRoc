using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CharacterModel: CharacterDefine, BattleItem, IStorable
{
    public BehaviorSubject<int> currentHp;
    public int level { get { return exp / GlobalAccess.levelUpExp + 1; } }
    public int remainExp { get { return exp % GlobalAccess.levelUpExp; } }
    public int exp;
    public Backpack backpack;

    public Subject<bool> characterUpdate = new Subject<bool>();
    private System.IDisposable disposable;
    private CharacterDefine define;

    //BattleItem
    public string uuid;
    public string uuID { get { return uuid; } }
    public new string Name { get; set; }
    public BattleItemType battleItemType { get; set; }
    public int remainActingTime { get; set; }
    public new string Resource { get; set; }
    public new int Mobility { get; set; }
    public new int Speed { get; set; }
    public new int MaxHP { get; set; }
    public new int Strength { get; set; }
    public new int Defense { get; set; }
    public new int Dodge { get; set; }
    public new int Accuracy { get; set; }
    public new int Energy { get; set; }
    public new int Lucky { get; set; }
    public new string Desc { get; set; }

    public CharacterModel()
    {}

    public CharacterModel(int id)
    {
        ID = id;
    }

    public CharacterModel(CharacterDefine define)
    {
        uuid = GameUtil.Instance.GenerateUniqueId();
        this.battleItemType = BattleItemType.player;
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
        backpack = new Backpack(define.ID, 3, 3, characterUpdate);
        currentHp = new BehaviorSubject<int>(define.MaxHP);
        disposable = characterUpdate.AsObservable().Subscribe(_ =>
        {
            NorneStore.Instance.Update<CharacterModel>(this, isFull: true);
        });
    }

    public string StorableCategory => "Character";

    public string Identifier => this.ID.ToString();

    ~CharacterModel()
    {
        if (disposable != null)
        {
            disposable.Dispose();
            disposable = null;
        }
    }

    public void healthChange(int value)
    {
        this.currentHp.OnNext(this.currentHp.Value + value);
        //if (value > 0)
        //{
        //    //回复飘字
        //}
        //else
        //{
        //    //伤害飘字
        //}
    }
}
