using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
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

    public void Use(int targetID, StoreItemModel item)
    {
        if (item.CanUse())
        {
            if (item.effect1.effectType != null)
            {
                ProcessEffect(targetID, item.effect1);
            }
            if (item.effect2.effectType != null)
            {
                ProcessEffect(targetID, item.effect2);
            }
            if (item.effect3.effectType != null)
            {
                ProcessEffect(targetID, item.effect3);
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

    public void ProcessEffect(int targetID, Effect effect)
    {
        var target = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(targetID)).value;
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
