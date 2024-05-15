using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Drop(StoreItemModel item)
    {
        GameManager.Instance.repository.RemoveItem(item.uuid);
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
        if (item.invokeType == InvokeType.bagUse || item.invokeType == InvokeType.equipUse)
        {
            if (!characterOrBattleItem && item.takeEnergy > 0)
            {
                //战斗需要消耗能量的物品使用时，扣除能量
                var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(selfID)).Value;
                target.currentEnergy -= item.takeEnergy;
                NorneStore.Instance.Update<BattleItem>(target, true);
            }
            List<string> selfIDList = new List<string> { selfID };
            if (item.effect1 != null)
            {
                ProcessEffect(selfIDList, item.effect1, characterOrBattleItem);
            }
            if (item.effect2 != null)
            {
                ProcessEffect(selfIDList, item.effect2, characterOrBattleItem);
            }
            if (item.effect3 != null)
            {
                ProcessEffect(selfIDList, item.effect3, characterOrBattleItem);
            }
        } else if (item.invokeType == InvokeType.equipTarget)
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
                    if (!characterOrBattleItem && item.takeEnergy > 0)
                    {
                        //战斗需要消耗能量的物品使用时，扣除能量
                        var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(selfID)).Value;
                        target.currentEnergy -= item.takeEnergy;
                        NorneStore.Instance.Update<BattleItem>(target, true);
                    }
                    if (item.effect1 != null)
                    {
                        ProcessEffect(targetIDs, item.effect1, characterOrBattleItem);
                    }
                    if (item.effect2 != null)
                    {
                        ProcessEffect(targetIDs, item.effect2, characterOrBattleItem);
                    }
                    if (item.effect3 != null)
                    {
                        ProcessEffect(targetIDs, item.effect3, characterOrBattleItem);
                    }
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

    public void ProcessEffect(List<string> targetIDs, Effect effect, bool characterOrBattleItem)
    {
        if (characterOrBattleItem)
        {
            ProcessEffect_Character(targetIDs, effect);
        }
        else
        {
            ProcessEffect_BattleItem(targetIDs, effect);
        }
    }

    public void ProcessEffect_Character(List<string> targetIDs, Effect effect)
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
                            target.currentHP += effect.Value;
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
                            target.currentHP += effect.Value;
                            break;
                        case PropertyType.Exp:
                            target.exp += effect.Value;
                            break;
                        default:
                            Debug.Log("unknown propertyType");
                            break;
                    }
                    NorneStore.Instance.Update<CharacterModel>(target, isFull: true);
                    break;
                case EffectType.skill:
                    SkillManager.Instance.InvokeSkill(targetID, effect.methodName);
                    break;
                case EffectType.buff:
                    BuffManager.Instance.AddBuff(targetID, effect.methodName);
                    break;
            }
        }
    }
    public void ProcessEffect_BattleItem(List<string> targetIDs, Effect effect)
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
                            target.currentHP += effect.Value;
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
                        default:
                            Debug.Log("unknown propertyType");
                            break;
                    }
                    NorneStore.Instance.Update<BattleItem>(target, isFull: true);
                    break;
                case EffectType.skill:
                    SkillManager.Instance.InvokeSkill(targetID, effect.methodName);
                    break;
                case EffectType.buff:
                    BuffManager.Instance.AddBuff(targetID, effect.methodName);
                    break;
            }
        }
    }
}
