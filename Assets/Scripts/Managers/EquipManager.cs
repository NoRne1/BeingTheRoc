using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UnityEditor.Progress;

public class EquipManager : MonoSingleton<EquipManager>
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Use(string selfID, StoreItemModel item, bool characterOrBattleItem)
    {
        if (item.CanUse())
        {
            if (characterOrBattleItem)
            {
                StartCoroutine(TriggerEquipEffect(selfID, item, characterOrBattleItem));
            }
            else
            {
                //战斗中存在能量不够的情况
                var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(selfID)).Value;
                if (target.currentEnergy >= item.takeEnergy)
                {
                    StartCoroutine(TriggerEquipEffect(selfID, item, characterOrBattleItem));
                }
                else if (target.currentEnergy > 0)
                {
                    BattleManager.Instance.ShakeEnergy();
                } else
                {
                    BattleManager.Instance.BlinkEnergy();
                }
            }
        }
    }

    public List<string> targetIDs = null;
    private IEnumerator TriggerEquipEffect(string selfID, StoreItemModel item, bool characterOrBattleItem)
    {
        targetIDs = null;
        if (item.invokeType == ItemInvokeType.bagUse || item.invokeType == ItemInvokeType.equipUse)
        {
            List<string> selfIDList = new List<string> { selfID };
            ProcessItemUse(selfID, item, selfIDList, characterOrBattleItem);
        } else if (item.invokeType == ItemInvokeType.equipTarget)
        {
            BattleManager.Instance.SelectTargets(item);
            // 等待玩家选择目标，比如点击其他游戏对象
            while (true)
            {
                if (targetIDs != null && targetIDs.Count == 0)
                {
                    break;
                } else if (targetIDs != null && targetIDs.Count > 0)
                {
                    ProcessItemUse(selfID, item, targetIDs, characterOrBattleItem);
                    break;
                }
                yield return null;
            }
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

    public void ProcessItemUse(string selfID, StoreItemModel item, List<string> targetIDs, bool characterOrBattleItem)
    {
        if (!characterOrBattleItem && item.takeEnergy > 0)
        {
            //战斗需要消耗能量的物品使用时，扣除能量
            var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(selfID)).Value;
            target.currentEnergy -= item.takeEnergy;
            NorneStore.Instance.Update<BattleItem>(target, true);
        }
        InvokeEffect(EffectInvokeType.useInstant, selfID, targetIDs, item, characterOrBattleItem);
        if (item.type == ItemType.expendable)
        {
            if (characterOrBattleItem)
            {
                RepoDrop(item);
            } else
            {
                var target = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(selfID)).Value;
                EquipDrop(target, item);
            }
        }
    }

    public void ProcessEffect(string selfID, List<string> targetIDs, StoreItemModel item, Effect effect, bool characterOrBattleItem)
    {
        if (characterOrBattleItem)
        {
            ProcessEffect_Character(selfID, targetIDs, item, effect);
        }
        else
        {
            ProcessEffect_BattleItem(selfID, targetIDs, item, effect);
        }
    }

    public void ProcessEffect_Character(string selfID, List<string> targetIDs, StoreItemModel item, Effect effect)
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
                            target.MaxHP += effect.Value;
                            break;
                        case PropertyType.Strength:
                            target.Strength += effect.Value;
                            break;
                        case PropertyType.Defense:
                            target.Defense += effect.Value;
                            break;
                        case PropertyType.Dodge:
                            target.Dodge += effect.Value;
                            break;
                        case PropertyType.Accuracy:
                            target.Accuracy += effect.Value;
                            break;
                        case PropertyType.Speed:
                            target.Speed += effect.Value;
                            break;
                        case PropertyType.Mobility:
                            target.Mobility += effect.Value;
                            break;
                        case PropertyType.Energy:
                            target.Energy += effect.Value;
                            break;
                        case PropertyType.Lucky:
                            target.Lucky += effect.Value;
                            break;
                        case PropertyType.HP:
                            Debug.Log("CharacterModel can not add currentHP");
                            break;
                        case PropertyType.Exp:
                            target.exp += effect.Value;
                            break;
                        case PropertyType.shield:
                            Debug.Log("CharacterModel no shield");
                            break;
                        default:
                            Debug.Log("unknown propertyType");
                            break;
                    }
                    NorneStore.Instance.Update<CharacterModel>(target, isFull: true);
                    break;
                case EffectType.skill:
                    SkillManager.Instance.InvokeSkill(targetID, targetID, effect.methodName, effect.Value);
                    break;
                case EffectType.buff:
                    BuffManager.Instance.AddBuff(targetID, effect.methodName);
                    break;
                case EffectType.attack:
                    Debug.LogError("角色使用的物品不存在攻击行为");
                    break;
            }
        }
    }
    public void ProcessEffect_BattleItem(string selfID, List<string> targetIDs, StoreItemModel item, Effect effect)
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
                            target.MaxHP += effect.Value;
                            target.currentHP += effect.Value;
                            break;
                        case PropertyType.HP:
                            BattleManager.Instance.ProcessHealth(selfID, targetIDs, effect.Value);
                            break;
                        case PropertyType.Strength:
                            target.Strength += effect.Value;
                            break;
                        case PropertyType.Defense:
                            target.Defense += effect.Value;
                            break;
                        case PropertyType.Dodge:
                            target.Dodge += effect.Value;
                            break;
                        case PropertyType.Accuracy:
                            target.Accuracy += effect.Value;
                            break;
                        case PropertyType.Speed:
                            target.Speed += effect.Value;
                            break;
                        case PropertyType.Mobility:
                            target.Mobility += effect.Value;
                            break;
                        case PropertyType.Energy:
                            target.currentEnergy += effect.Value;
                            break;
                        case PropertyType.Lucky:
                            target.Lucky += effect.Value;
                            break;
                        case PropertyType.Exp:
                            target.exp += effect.Value;
                            break;
                        case PropertyType.shield:
                            target.shield += effect.Value;
                            break;
                        default:
                            Debug.Log("unknown propertyType");
                            break;
                    }
                    NorneStore.Instance.Update<BattleItem>(target, isFull: true);
                    break;
                case EffectType.skill:
                    SkillManager.Instance.InvokeSkill(selfID, targetID, effect.methodName, effect.Value);
                    break;
                case EffectType.buff:
                    BuffManager.Instance.AddBuff(targetID, effect.methodName);
                    break;
                case EffectType.attack:
                    var attackStatus = BattleManager.Instance.ProcessAttack(selfID, targetIDs, effect.Value);
                    switch (attackStatus)
                    {
                        case AttackStatus.normal:
                            InvokeEffect(EffectInvokeType.damage, selfID, targetIDs, item, false);
                            break;
                        case AttackStatus.toDeath:
                            InvokeEffect(EffectInvokeType.toDeath, selfID, targetIDs, item, false);
                            break;
                    }
                    break;
            }
        }
    }

    private void InvokeEffect(EffectInvokeType invokeType, string selfID, List<string> targetIDs, StoreItemModel item, bool characterOrBattleItem)
    {
        var useEffects = item.effects.Where(effect => effect.invokeType == invokeType).ToList();
        if (useEffects.Count > 0)
        {
            foreach (var effect in useEffects)
            {
                ProcessEffect(selfID, targetIDs, item, effect, characterOrBattleItem);
            }
        }
    }
}
