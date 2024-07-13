using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniRx;

public class ItemUseManager : MonoSingleton<ItemUseManager>
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Use(string casterID, StoreItemModel item)
    {
        switch(item.type)
        {
            case ItemType.equip:
                //战斗中存在能量不够的情况
                var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(casterID)).Value;
                if (target.attributes.currentEnergy >= item.equipDefine.takeEnergy)
                {
                    StartCoroutine(TriggerEffect(casterID, item));
                }
                else if (target.attributes.currentEnergy > 0)
                {
                    BattleManager.Instance.ShakeEnergy();
                }
                else
                {
                    BattleManager.Instance.BlinkEnergy();
                }
                break;
            case ItemType.expendable:
                StartCoroutine(TriggerEffect(casterID, item));
                break;
            case ItemType.treasure:
                StartCoroutine(TriggerEffect(casterID, item));
                break;
            default:
                Debug.LogError("EquipManager use unknown type item");
                break;
        }
    }

    public List<string> targetIDs = null;
    private IEnumerator TriggerEffect(string casterID, StoreItemModel item)
    {
        switch (item.type)
        {
            case ItemType.equip:
                switch (item.equipDefine.invokeType)
                {
                    case EquipInvokeType.use:
                        ProcessItemUse(casterID, item, new List<string> { casterID });
                        break;
                    case EquipInvokeType.targetItem:
                        targetIDs = null;
                        BattleManager.Instance.chessboardManager.SelectTargets(item);
                        // 等待玩家选择目标，比如点击其他游戏对象
                        while (true)
                        {
                            if (targetIDs != null && targetIDs.Count == 0)
                            {
                                break;
                            }
                            else if (targetIDs != null && targetIDs.Count > 0)
                            {
                                ProcessItemUse(casterID, item, targetIDs);
                                break;
                            }
                            yield return null;
                        }
                        break;
                    case EquipInvokeType.targetPos:
                        //todo 暂时不支持，目前复制的选择目标对象逻辑
                        targetIDs = null;
                        BattleManager.Instance.chessboardManager.SelectTargets(item);
                        // 等待玩家选择目标，比如点击其他游戏对象
                        while (true)
                        {
                            if (targetIDs != null && targetIDs.Count == 0)
                            {
                                break;
                            }
                            else if (targetIDs != null && targetIDs.Count > 0)
                            {
                                ProcessItemUse(casterID, item, targetIDs);
                                break;
                            }
                            yield return null;
                        }
                        break;
                    default:
                        Debug.LogError("TriggerEffect unknown item.equipDefine.invokeType");
                        break;
                }
                break;
            case ItemType.expendable:
                ProcessItemUse(casterID, item, new List<string> { casterID });
                break;
            case ItemType.treasure:
                ProcessItemUse(casterID, item, new List<string> { casterID });
                break;
            default:
                Debug.LogError("TriggerEffect unknown item type");
                break;
        }
    }

    public bool Equip(CharacterModel character, StoreItemModel item, Vector2Int gridPosition)
    {
        if (character.backpack.Place(item, gridPosition))
        {
            GameManager.Instance.repository.RemoveItem(item.uuid);
            return true;
        }
        return false;
    }

    public void Unequip(CharacterModel character, StoreItemModel item)
    {
        GameManager.Instance.repository.AddItem(item);
        character.backpack.RemoveItemsByUUID(item.uuid);
        item.Unequip();
    }

    public void RepoDrop(StoreItemModel item)
    {
        GameManager.Instance.repository.RemoveItem(item.uuid);
    }

    public void EquipDrop(CharacterModel character, StoreItemModel item)
    {
        character.backpack.RemoveItemsByUUID(item.uuid);
        item.Unequip();
    }

    public void ProcessItemUse(string casterID, StoreItemModel item, List<string> targetIDs)
    {
        switch (item.type)
        {
            case ItemType.equip:
                if (item.equipDefine.takeEnergy > 0)
                {
                    //战斗需要消耗能量的物品使用时，扣除能量
                    var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(casterID)).Value;
                    target.attributes.currentEnergy -= item.equipDefine.takeEnergy;
                    if (target.attributes.currentEnergy == 0 &&
                        item.effects.Where(effect => effect.invokeType == EffectInvokeType.useInstant &&
                            effect.effectType == EffectType.attack).ToList().Count > 0)
                    {
                        //能量为0，并且是攻击行为
                        target.lastEnergyAttackSubject.OnNext(Unit.Default);
                    }
                    NorneStore.Instance.Update<BattleItem>(target, true);
                }
                InvokeEffect(EffectInvokeType.useInstant, casterID, targetIDs, item);
                if (item.equipDefine.isExpendable)
                {
                    var target = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(casterID)).Value;
                    EquipDrop(target, item);
                }
                break;
            case ItemType.expendable:
                InvokeEffect(EffectInvokeType.useInstant, casterID, targetIDs, item);
                RepoDrop(item);
                break;
            case ItemType.treasure:
                GameManager.Instance.treasureManager.InvokeTreasureEffect(item.treasureDefine.ID);
                break;
            default:
                Debug.LogError("TriggerEffect unknown item type");
                break;
        }
    }

    public void ProcessEffect(string casterID, List<string> targetIDs, StoreItemModel item, Effect effect)
    {
        switch (item.type)
        {
            case ItemType.equip:
                ProcessEffect_BattleItem(casterID, targetIDs, item, effect);
                break;
            case ItemType.expendable:
                ProcessEffect_Character(casterID, targetIDs, item, effect);
                break;
            default:
                Debug.LogError("ProcessEffect unknown item type");
                break;
        }
    }

    public void ProcessEffect_Character(string casterID, List<string> targetIDs, StoreItemModel item, Effect effect)
    {
        foreach (var targetID in targetIDs)
        {
            var target = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(targetID)).Value;
            
            switch (effect.effectType)
            {
                case EffectType.property:
                    switch (effect.propertyType)
                    {
                        case PropertyType.MaxHP:
                            target.attributes.ItemEffect.MaxHP += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Strength:
                            target.attributes.ItemEffect.Strength += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Defense:
                            target.attributes.ItemEffect.Defense += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Dodge:
                            target.attributes.ItemEffect.Dodge += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Accuracy:
                            target.attributes.ItemEffect.Accuracy += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Speed:
                            target.attributes.ItemEffect.Speed += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Mobility:
                            target.attributes.ItemEffect.Mobility += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Energy:
                            target.attributes.ItemEffect.Energy += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Lucky:
                            target.attributes.ItemEffect.Lucky += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Health:
                            Debug.Log("CharacterModel can not add currentHP");
                            break;
                        case PropertyType.Exp:
                            target.attributes.exp += effect.Value;
                            break;
                        case PropertyType.Shield:
                            Debug.Log("CharacterModel no shield");
                            break;
                        case PropertyType.Protection:
                            Debug.Log("CharacterModel can not add Protection");
                            break;
                        case PropertyType.EnchanceDamage:
                            Debug.Log("CharacterModel can not add EnchanceDamage");
                            break;
                        case PropertyType.Hematophagia:
                            target.attributes.ItemEffect.Hematophagia += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        default:
                            Debug.Log("unknown propertyType");
                            break;
                    }
                    NorneStore.Instance.Update<CharacterModel>(target, isFull: true);
                    break;
                case EffectType.skill:
                    SkillManager.Instance.InvokeSkill(targetID, effect.methodName, effect.propertyType ?? PropertyType.none, effect.Value);
                    break;
                case EffectType.buff:
                    //target.buffCenter.AddBuff(DataManager.Instance.BuffDefines[effect.Value], casterID);
                    Debug.LogError("角色使用的物品没有buff行为");
                    break;
                case EffectType.attack:
                    Debug.LogError("角色使用的物品不存在攻击行为");
                    break;
            }
        }
    }
    public void ProcessEffect_BattleItem(string casterID, List<string> targetIDs, StoreItemModel item, Effect effect)
    {
        foreach (var targetID in targetIDs)
        {
            var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(targetID)).Value;

            switch (effect.effectType)
            {
                case EffectType.property:
                    switch (effect.propertyType)
                    {
                        case PropertyType.MaxHP:
                            target.attributes.InBattle.MaxHP += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            target.attributes.currentHP += effect.Value;
                            break;
                        case PropertyType.Health:
                            BattleManager.Instance.ProcessNormalHealth(casterID, targetIDs, effect.Value);
                            break;
                        case PropertyType.Strength:
                            target.attributes.InBattle.Strength += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Defense:
                            target.attributes.InBattle.Defense += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Dodge:
                            target.attributes.InBattle.Dodge += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Accuracy:
                            target.attributes.InBattle.Accuracy += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Speed:
                            target.attributes.InBattle.Speed += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Mobility:
                            target.attributes.InBattle.Mobility += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Energy:
                            target.attributes.currentEnergy += effect.Value;
                            break;
                        case PropertyType.Lucky:
                            target.attributes.InBattle.Lucky += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Exp:
                            target.attributes.exp += effect.Value;
                            break;
                        case PropertyType.Shield:
                            target.attributes.currentShield+= effect.Value;
                            break;
                        case PropertyType.Protection:
                            target.attributes.InBattle.Protection += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.EnchanceDamage:
                            target.attributes.InBattle.EnchanceDamage += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        case PropertyType.Hematophagia:
                            target.attributes.InBattle.Hematophagia += effect.Value;
                            target.attributes.LoadFinalAttributes();
                            break;
                        default:
                            Debug.Log("unknown propertyType");
                            break;
                    }
                    NorneStore.Instance.Update<BattleItem>(target, isFull: true);
                    break;
                case EffectType.skill:
                    SkillManager.Instance.InvokeSkill(casterID, effect.methodName, effect.propertyType ?? PropertyType.none, effect.Value);
                    break;
                case EffectType.buff:
                    target.buffCenter.AddBuff(DataManager.Instance.BuffDefines[effect.Value], casterID);
                    break;
                case EffectType.attack:
                    var caster = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(casterID)).Value;
                    if (!caster.isSilent)
                    {
                        var attackStatus = BattleManager.Instance.ProcessNormalAttack(casterID, targetIDs, item.equipDefine.baseAccuracy, effect.Value, item.equipDefine.equipClass);
                        switch (attackStatus)
                        {
                            case AttackStatus.normal:
                                InvokeEffect(EffectInvokeType.damage, casterID, targetIDs, item);
                                break;
                            case AttackStatus.toDeath:
                                caster.defeatSubject.OnNext(Unit.Default);
                                InvokeEffect(EffectInvokeType.toDeath, casterID, targetIDs, item);
                                break;
                        }
                        caster.haveAttackedInRound = true;
                    } else
                    {
                        Debug.Log("沉默状态攻击失败");
                    }
                    break;
            }
        }
    }

    private void InvokeEffect(EffectInvokeType invokeType, string casterID, List<string> targetIDs, StoreItemModel item)
    {
        var useEffects = item.effects.Where(effect => effect.invokeType == invokeType).ToList();
        if (useEffects.Count > 0)
        {
            foreach (var effect in useEffects)
            {
                ProcessEffect(casterID, targetIDs, item, effect);
            }
        }
    }
}
