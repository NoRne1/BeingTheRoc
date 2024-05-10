using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
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

    public void Use(string selfID, StoreItemModel item)
    {
        if (item.CanUse())
        {
            StartCoroutine(TriggerEquipEffect(selfID, item));
        }
    }

    public List<string> targetIDs = null;
    private IEnumerator TriggerEquipEffect(string selfID, StoreItemModel item)
    {
        targetIDs = null;
        if (item.invokeType == InvokeType.use)
        {
            List<string> selfIDList = new List<string> { selfID };
            if (item.effect1.effectType != null)
            {
                ProcessEffect(selfIDList, item.effect1);
            }
            if (item.effect2.effectType != null)
            {
                ProcessEffect(selfIDList, item.effect2);
            }
            if (item.effect3.effectType != null)
            {
                ProcessEffect(selfIDList, item.effect3);
            }
        } else if (item.invokeType == InvokeType.target)
        {
            BattleManager.Instance.SelectTargets(item);
            // 等待玩家选择目标，比如点击其他游戏对象
            while (true)
            {
                if (targetIDs != null)
                {
                    if (item.effect1.effectType != null)
                    {
                        ProcessEffect(targetIDs, item.effect1);
                    }
                    if (item.effect2.effectType != null)
                    {
                        ProcessEffect(targetIDs, item.effect2);
                    }
                    if (item.effect3.effectType != null)
                    {
                        ProcessEffect(targetIDs, item.effect3);
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

    public void ProcessEffect(List<string> targetIDs, Effect effect)
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
                            target.healthChange(effect.Value);
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
                        default:
                            Debug.Log("unknown propertyType");
                            break;
                    }
                    NorneStore.Instance.Update<CharacterModel>(target, isFull: true);
                    break;
                case EffectType.health:
                    target.healthChange(effect.Value);
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
