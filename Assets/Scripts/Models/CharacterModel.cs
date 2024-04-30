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

    public BattleItemType battleItemType { get; set; }
    public int remainActingTime { get; set; }
    public string Resource
    {
        get { return base.Resource; }
        set { base.Resource = value; }
    }

    public CharacterModel()
    {}

    public CharacterModel(int id)
    {
        ID = id;
    }
    public CharacterModel(CharacterDefine define)
    {
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
