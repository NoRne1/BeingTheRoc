using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniRx;

public enum ChooseTargetType
{
    none = -1,
    items = 0,
    position = 1,
}

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

    //第一层能否使用检查
    public IEnumerator Use(string casterID, StoreItemModel item)
    {
        switch (item.type)
        {
            case ItemType.equip:
                //战斗中存在能量不够的情况
                var caster = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(casterID)).Value;
                if (caster.attributes.currentEnergy >= item.equipDefine.takeEnergy)
                {
                    StartCoroutine(InvokeCheck(casterID, item));
                }
                else if (caster.attributes.currentEnergy > 0)
                {
                    BattleManager.Instance.ShakeEnergy();
                }
                else
                {
                    BattleManager.Instance.BlinkEnergy();
                }
                break;
            case ItemType.expendable:
                yield return StartCoroutine(InvokeEffect(EffectInvokeType.useInstant, casterID, new List<string>{casterID}, Vector2.negativeInfinity, item));
                RepoDrop(item);
                break;
            case ItemType.treasure:
                Debug.LogError("Item Use should not handle treasure type");
                break;
            case ItemType.economicGoods:
                Debug.LogError("Item Use should not handle economicGoods type");
                break;
            case ItemType.food:
                var target = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(casterID)).Value;
                if(item.foodModel.foodPropertys.FirstOrDefault(property => property.type == PropertyType.hungry).trulyValue + target.CurrentHungry <= target.MaxHungry)
                {
                    yield return StartCoroutine(InvokeEffect(EffectInvokeType.useInstant, casterID, new List<string>{casterID}, Vector2.negativeInfinity, item));
                    RepoDrop(item);
                } else 
                {
                    BlackBarManager.Instance.AddMessage("你要撑死这个单位嘛,吃不下啦!");
                }
                break;
            default:
                Debug.LogError("Item Use unknown item type");
                break;
        }
    }

    public List<string> targetIDs = null;
    public Vector2 targetPos = Vector2.negativeInfinity;
    public bool targetChooseBreakFlag = false; // 标志位，表示目标选择是否被打断
    private void TargetChooseInit()
    {
        this.targetIDs = null;
        targetPos = Vector2.negativeInfinity;
        targetChooseBreakFlag = false;
    }

    //触发方式检查
    private IEnumerator InvokeCheck(string casterID, StoreItemModel item)
    {
        switch (item.type)
        {
            case ItemType.equip:
                switch (item.equipDefine.invokeType)
                {
                    case EquipInvokeType.use:
                        yield return StartCoroutine(ProcessEquipUse(casterID, item, new List<string> { casterID }, Vector2.negativeInfinity));
                        break;
                    case EquipInvokeType.targetItem:
                        TargetChooseInit();
                        BattleManager.Instance.chessboardManager.SelectTargets(item, ChooseTargetType.items);
                        // 等待玩家选择目标
                        yield return new WaitUntil(() => (targetIDs != null && targetIDs.Count > 0 && !targetChooseBreakFlag));
                        yield return StartCoroutine(ProcessEquipUse(casterID, item, targetIDs, targetPos));
                        break;
                    case EquipInvokeType.targetPos:
                        TargetChooseInit();
                        BattleManager.Instance.chessboardManager.SelectTargets(item, ChooseTargetType.position);
                        // 等待玩家选择目标
                        yield return new WaitUntil(() => (targetPos != Vector2.negativeInfinity && !targetChooseBreakFlag));
                        yield return StartCoroutine(ProcessEquipUse(casterID, item, targetIDs, targetPos));
                        break;
                    default:
                        Debug.LogError("TriggerEffect unknown " + item.equipDefine.invokeType);
                        break;
                }
                break;
            case ItemType.expendable:
                Debug.LogError("TriggerEffect should not handle expendable type");
                break;
            case ItemType.treasure:
                Debug.LogError("TriggerEffect should not handle treasure type");
                break;
            case ItemType.economicGoods:
                Debug.LogError("TriggerEffect should not handle economicGoods type");
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

    public void EquipDrop(string casterID, StoreItemModel item)
    {
        var character = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(casterID)).Value;
        var battleItem = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(casterID)).Value;
        character.backpack.RemoveItemsByUUID(item.uuid);
        battleItem.backpack.RemoveItemsByUUID(item.uuid);
        item.Unequip();
    }

    //触发装备效果并消耗能量
    public IEnumerator ProcessEquipUse(string casterID, StoreItemModel item, List<string> targetIDs, Vector2 targetPos)
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
                var casterItem = BattleManager.Instance.battleItemManager.GetUIBattleItemByUUid(casterID);
                casterItem.ItemUseAni(item);
                yield return StartCoroutine(InvokeEffect(EffectInvokeType.useInstant, casterID, targetIDs, targetPos, item));
                if (item.equipDefine.isExpendable)
                {
                    EquipDrop(casterID, item);
                }
                break;
            case ItemType.expendable:
                Debug.LogError("EquipUse should not handle expendable type");
                break;
            case ItemType.treasure:
                Debug.LogError("EquipUse should not handle treasure type");
                break;
            case ItemType.economicGoods:
                Debug.LogError("EquipUse should not handle economicGoods type");
                break;
            default:
                Debug.LogError("EquipUse unknown item type");
                break;
        }
        yield return null;
    }

    public IEnumerator InvokeEffect(EffectInvokeType invokeType, string casterID, List<string> targetIDs, Vector2 targetPos, StoreItemModel item)
    {
        var useEffects = item.effects.Where(effect => effect.invokeType == invokeType).ToList();
        if (useEffects.Count > 0)
        {
            foreach (var effect in useEffects)
            {
                yield return StartCoroutine(ProcessEffect(casterID, targetIDs, targetPos, item, effect));
            }
        }
    }

    //检查是战斗中使用还是战斗外使用
    public IEnumerator ProcessEffect(string casterID, List<string> targetIDs, Vector2 targetPos, StoreItemModel item, Effect effect)
    {
        switch (item.type)
        {
            case ItemType.equip:
                yield return StartCoroutine(ProcessEffect_Battle(casterID, targetIDs, targetPos, item, effect));
                break;
            case ItemType.expendable:
            case ItemType.food:
                if (targetIDs == null || targetIDs.Count != 1)
                {
                    Debug.LogError("ProcessEffect expendable targetIDs should be 1");
                }
                else
                {
                    yield return StartCoroutine(ProcessEffect_Normal(casterID, targetIDs.First(), item, effect));
                }
                break;
            default:
                Debug.LogError("ProcessEffect unknown item type");
                break;
        }
    }
    //战斗外对角色的使用
    public IEnumerator ProcessEffect_Normal(string casterID, string targetID, StoreItemModel item, Effect effect)
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
                    case PropertyType.HealthPercent:
                        target.attributes.ItemEffect.Hematophagia += effect.Value;
                        target.attributes.LoadFinalAttributes();
                        break;
                    case PropertyType.hungry:
                        target.HungryChange(effect.Value);
                        break;
                    default:
                        Debug.Log("unknown propertyType");
                        break;
                }
                NorneStore.Instance.Update<CharacterModel>(target, isFull: true);
                break;
            case EffectType.battleEffect:
                Debug.LogError("战斗外使用的物品没有battleEffect行为");
                break;
            case EffectType.buff:
                //target.buffCenter.AddBuff(DataManager.Instance.BuffDefines[effect.Value], casterID);
                Debug.LogError("战斗外使用的物品没有buff行为");
                break;
            case EffectType.attack:
                Debug.LogError("战斗外使用的物品不存在攻击行为");
                break;
        }
        yield return null;
    }
    //战斗内对BattleItem或位置的使用
    public IEnumerator ProcessEffect_Battle(string casterID, List<string> targetIDs, Vector2 targetPos, StoreItemModel item, Effect effect)
    {
        if ((targetIDs == null || targetIDs.Count == 0) && targetPos != Vector2.negativeInfinity)
        {
            //对位置触发效果
            switch (effect.effectType)
            {
                case EffectType.property:
                    Debug.LogError("ProcessEffect_Battle to targetPos should not be property");
                    break;
                case EffectType.battleEffect:
                    yield return StartCoroutine(SkillManager.Instance.InvokeBattleEffect(casterID, effect.methodName, "", targetPos, effect.Value));
                    break;
                case EffectType.buff:
                    Debug.LogError("ProcessEffect_Battle to targetPos should not be buff");
                    break;
                case EffectType.attack:
                    Debug.LogError("ProcessEffect_Battle to targetPos should not be attack");
                    break;
                default:
                    Debug.LogError("ProcessEffect_Battle to targetPos unkonwn effectType");
                    break;
            }
        } else if (targetIDs != null && targetIDs.Count > 0)
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
                                BattleCommonMethods.ProcessNormalHealth(casterID, new List<string>{targetID}, effect.Value);
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
                                target.attributes.currentShield += effect.Value;
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
                            case PropertyType.HealthPercent:
                                BattleCommonMethods.ProcessNormalHealth(casterID, new List<string>{targetID}, (int)(target.attributes.MaxHP / 100.0f * effect.Value));
                                break;
                            case PropertyType.hungry:
                                target.HungryChange(effect.Value);
                                break;
                            default:
                                Debug.Log("unknown propertyType");
                                break;
                        }
                        NorneStore.Instance.Update<BattleItem>(target, isFull: true);
                        break;
                    case EffectType.battleEffect:
                        yield return StartCoroutine(SkillManager.Instance.InvokeBattleEffect(casterID, effect.methodName, targetID, targetPos, effect.Value));
                        break;
                    case EffectType.buff:
                        target.buffCenter.AddBuff(DataManager.Instance.BuffDefines[effect.Value], casterID);
                        break;
                    case EffectType.attack:
                        //对单个targetID不处理，下面进行统一处理
                        break;
                }
            }
            switch (effect.effectType)
            {
                case EffectType.attack:
                    var caster = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(casterID)).Value;
                    if (!caster.isSilent)
                    {
                        var result = BattleCommonMethods.CalcNormalAttack(casterID, targetIDs, item, effect.Value, effect.invokeTime);
                        var casterItem = BattleManager.Instance.battleItemManager.GetUIBattleItemByUUid(casterID);
                        casterItem.SetAttackResult(result);
                        caster.haveAttackedInRound = true;
                    }
                    else
                    {
                        BlackBarManager.Instance.AddMessage("沉默状态，攻击失败");
                    }
                    break;
                default:
                    break;
            }
        }
        yield return null;
    }
}
