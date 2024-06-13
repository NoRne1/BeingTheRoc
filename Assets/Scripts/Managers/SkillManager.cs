using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoSingleton<SkillManager>
{
    private StoreItemModel hunyuanSword;
    // Start is called before the first frame update 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InvokeSkill(string selfID, string targetID, string methodName, int value)
    {
        var method = typeof(SkillManager).GetMethod(methodName);
        object[] parameters = new object[] { selfID, targetID, value };
        method?.Invoke(SkillManager.Instance, parameters);
        Debug.Log("skill " + methodName + " has been invoked");
    }

    private void HunYuanSword_0(string selfID, string targetID, int value)
    {
        StoreItemDefine sword = new StoreItemDefine();
        sword.type = ItemType.equip;
        sword.equipType = EquipType.X2_1;
        sword.invokeType = ItemInvokeType.equipTarget;
        sword.targetRange = TargetRange.range_2;
        sword.title = GameUtil.Instance.GetDIsplayString("混元剑坯");
        sword.level = GenerlLevel.green;
        sword.takeEnergy = 1;
        sword.iconResource = "hunyuan_sword_icon";
        sword.iconResource2 = "hunyuan_sword_icon_big";
        sword.desc = GameUtil.Instance.GetDIsplayString("混元剑坯balabalabalabala");
        Effect effect1 = new Effect();
        effect1.effectType = EffectType.attack;
        effect1.propertyType = PropertyType.none;
        effect1.Value = 10;
        sword.effect1 = effect1;
        hunyuanSword = new StoreItemModel(sword);
        GameManager.Instance.repository.AddItem(new StoreItemModel(sword));
    }

    private void HunYuanYu(string selfID, string targetID, int value)
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

    private void HunYuanLi(string selfID, string targetID, int value)
    {
        if (hunyuanSword != null)
        {
            hunyuanSword.effect1.value += 5;
        }
    }

    private void HunYuanCheng(string selfID, string targetID, int value)
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

    private void HunYuanJi(string selfID, string targetID, int value)
    {
        if (hunyuanSword != null)
        {
            Effect effect = new Effect();
            effect.effectType = EffectType.skill;
            effect.invokeType = EffectInvokeType.damage;
            effect.methodName = "";
            effect.Value = 1;
            hunyuanSword.effects.Add(effect);
        }
    }

    private void HunYuanWu(string selfID, string targetID, int value)
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

    private void HunYuanLing(string selfID, string targetID, int value)
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

    private void MoveChangeSelf(string selfID, string targetID, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(selfID);
        battleItem.remainActingDistance = Mathf.Max(0,
            battleItem.remainActingDistance - GlobalAccess.roundDistance * value / 100.0f);
        GlobalAccess.SaveBattleItem(battleItem);
        BattleManager.Instance.CalcBattleItemAndShow(0);
    }

    private void MoveChangeTarget(string selfID, string targetID, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(targetID);
        battleItem.remainActingDistance = Mathf.Max(0,
            battleItem.remainActingDistance - GlobalAccess.roundDistance * value / 100.0f);
        GlobalAccess.SaveBattleItem(battleItem);
        BattleManager.Instance.CalcBattleItemAndShow(0);
    }
}
