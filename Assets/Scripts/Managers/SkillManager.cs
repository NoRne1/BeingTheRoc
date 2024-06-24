using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class SkillManager : MonoSingleton<SkillManager>
{
    private StoreItemModel hunyuanSword;
    private DisposablePool disposablePool;
    private Timer timer;
    // Start is called before the first frame update 
    void Start()
    {
        disposablePool = new DisposablePool();
        timer = new Timer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    ~SkillManager()
    {
        disposablePool.CleanDisposables();
    }

    public void InvokeSkill(string casterID, string targetID, string methodName)
    {
        var method = typeof(SkillManager).GetMethod(methodName);
        object[] parameters = new object[] { casterID, targetID };
        method?.Invoke(SkillManager.Instance, parameters);
        Debug.Log("skill " + methodName + " has been invoked");
    }

    private void HunYuanSword_0(string casterID, string targetID)
    {
        StoreItemDefine sword = new StoreItemDefine();
        sword.type = ItemType.equip;
        sword.equipType = EquipType.X2_1;
        sword.invokeType = ItemInvokeType.equipTarget;
        sword.targetRange = TargetRange.range_2;
        sword.title = GameUtil.Instance.GetDisplayString("混元剑坯");
        sword.level = GeneralLevel.green;
        sword.takeEnergy = 1;
        sword.iconResource = "hunyuan_sword_icon";
        sword.iconResource2 = "hunyuan_sword_icon_big";
        sword.desc = GameUtil.Instance.GetDisplayString("混元剑坯balabalabalabala");
        Effect effect1 = new Effect();
        effect1.effectType = EffectType.attack;
        effect1.propertyType = PropertyType.none;
        effect1.Value = 10;
        sword.effect1 = effect1;
        hunyuanSword = new StoreItemModel(sword);
        GameManager.Instance.repository.AddItem(new StoreItemModel(sword));
    }

    private void HunYuanYu(string casterID, string targetID)
    {
        if (hunyuanSword != null)
        {
            Effect effect = new Effect();
            effect.effectType = EffectType.property;
            effect.invokeType = EffectInvokeType.useInstant;
            effect.propertyType = PropertyType.shield;
            effect.Value = 8;
            hunyuanSword.effects.Add(effect);
        }
    }

    private void HunYuanLi(string casterID, string targetID)
    {
        if (hunyuanSword != null)
        {
            hunyuanSword.effect1.value += 5;
        }
    }

    private void HunYuanCheng(string casterID, string targetID)
    {
        if (hunyuanSword != null)
        {
            Effect effect = new Effect();
            effect.effectType = EffectType.property;
            effect.invokeType = EffectInvokeType.toDeath;
            effect.propertyType = PropertyType.Lucky;
            effect.Value = 1;
            hunyuanSword.effects.Add(effect);
        }
    }

    private void HunYuanJi(string casterID, string targetID)
    {
        if (hunyuanSword != null)
        {
            var battleItem = GlobalAccess.GetBattleItem(casterID);
            battleItem.moveAdvancedDistance += (int)(GlobalAccess.roundDistance * 10 / 100.0f);
            GlobalAccess.SaveBattleItem(battleItem);
        }
    }

    private void HunYuanWu(string casterID, string targetID)
    {
        if (hunyuanSword != null)
        {
            Effect effect = new Effect();
            effect.effectType = EffectType.buff;
            effect.invokeType = EffectInvokeType.useInstant;
            effect.Value = 0;
            hunyuanSword.effects.Add(effect);
        }
    }

    private void HunYuanLing(string casterID, string targetID)
    {
        if (hunyuanSword != null)
        {
            Effect effect = new Effect();
            effect.effectType = EffectType.skill;
            effect.invokeType = EffectInvokeType.useInstant;
            effect.methodName = "ReturnEnergy";
            effect.Value = hunyuanSword.takeEnergy;
            hunyuanSword.effects.Add(effect);
        }
    }

    private void HuanMie(string casterID, string targetID)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        timer.CreateTimer(casterID + "HuanMie", 2);
        battleItem.defeatSubject.AsObservable().Where(_ => timer.TimerNext(casterID + "HuanMie")).Subscribe(_ =>
        {
            BattleManager.Instance.extraRound++;
        });
    }

    private void HuanMeng(string casterID, string targetID)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        battleItem.defeatSubject.AsObservable().Subscribe(_ =>
        {
            var battleItem = GlobalAccess.GetBattleItem(casterID);
            battleItem.buffCenter.AddBuff(DataManager.Instance.BuffDefines[1], casterID);
            GlobalAccess.SaveBattleItem(battleItem);
        });
    }

    //private void HuanShen(string casterID, string targetID)
    //{
    //    var battleItem = GlobalAccess.GetBattleItem(casterID);
    //    battleItem.defeatSubject.AsObservable().Subscribe(_ =>
    //    {
    //        var battleItem = GlobalAccess.GetBattleItem(casterID);
    //        battleItem.buffCenter.AddBuff(DataManager.Instance.BuffDefines[1], casterID);
    //        GlobalAccess.SaveBattleItem(battleItem);
    //    });
    //}
    
    private void ReturnEnergy(string casterID, string targetID, int value)
    {
        if (hunyuanSword != null)
        {
            if (GlobalAccess.GetRandomRate_affected(20))
            {
                var battleItem = GlobalAccess.GetBattleItem(casterID);
                battleItem.attributes.currentEnergy += value;
                GlobalAccess.SaveBattleItem(battleItem);
            }
        }
    }

    private void MoveChangeSelf(string casterID, string targetID, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        battleItem.remainActingDistance = (int)(battleItem.remainActingDistance * (1 - (value / 100.0f)));
        GlobalAccess.SaveBattleItem(battleItem);
        BattleManager.Instance.CalcBattleItemAndShow(0);
    }

    private void MoveChangeTarget(string casterID, string targetID, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(targetID);
        battleItem.remainActingDistance = (int)(battleItem.remainActingDistance * (1 - (value / 100.0f)));
        GlobalAccess.SaveBattleItem(battleItem);
        BattleManager.Instance.CalcBattleItemAndShow(0);
    }
}
