using System;
using System.Collections.Generic;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System.Reflection;

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

        BattleManager.Instance.roundManager.roundTime.AsObservable().TakeUntilDestroy(this)
            .Where(round => round.Item2 == RoundTime.begin && BattleManager.Instance.roundManager.extraRound == 0)
            .Subscribe(round =>
        {
            timer.NextRound(round.Item1);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    ~SkillManager()
    {
        disposablePool.CleanDisposables();
    }

    public void BattleEnd()
    {
        disposablePool.CleanDisposables();
        timer.Clean();
    }

    public void InvokeSkill(string casterID, string methodName, PropertyType type = PropertyType.none, int value = 0)
    {
        var method = typeof(SkillManager).GetMethod(methodName,
    BindingFlags.NonPublic | BindingFlags.Instance);
        object[] parameters = new object[] { casterID, type, value };
        method?.Invoke(SkillManager.Instance, parameters);
        Debug.Log("skill " + methodName + " has been invoked");
    }

    public void InvokeBattleEffect(string casterID, string methodName, string targetID, Vector2 targetPos, int value)
    {
        var method = typeof(SkillManager).GetMethod(methodName,
    BindingFlags.NonPublic | BindingFlags.Instance);
        object[] parameters = new object[] { casterID, targetID, targetPos, value };
        method?.Invoke(SkillManager.Instance, parameters);
        Debug.Log("BattleEffect " + methodName + " has been invoked");
    }

// ----------------------Skill----------------------
    private void HunYuanSword_0(string casterID, PropertyType type, int value)
    {
        StoreItemDefine sword = DataManager.Instance.StoreItems[19];
        hunyuanSword = new StoreItemModel(DataManager.Instance.StoreItems[19]);
        GameManager.Instance.repository.AddItem(hunyuanSword);
    }

    private void HunYuanYu(string casterID, PropertyType type, int value)
    {
        if (hunyuanSword != null)
        {
            Effect effect = new Effect();
            effect.effectType = EffectType.property;
            effect.invokeType = EffectInvokeType.useInstant;
            effect.propertyType = PropertyType.Shield;
            effect.Value = 8;
            hunyuanSword.effects.Add(effect);
        }
    }

    private void HunYuanLi(string casterID, PropertyType type, int value)
    {
        if (hunyuanSword != null)
        {
            hunyuanSword.equipDefine.effect1.value += 5;
        }
    }

    private void HunYuanCheng(string casterID, PropertyType type, int value)
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

    private void HunYuanJi(string casterID, PropertyType type, int value)
    {
        if (hunyuanSword != null)
        {
            var battleItem = GlobalAccess.GetBattleItem(casterID);
            battleItem.moveAdvancedDistance += (int)(GlobalAccess.roundDistance * 10 / 100.0f);
            GlobalAccess.SaveBattleItem(battleItem);
        }
    }

    private void HunYuanWu(string casterID, PropertyType type, int value)
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

    private void HunYuanLing(string casterID, PropertyType type, int value)
    {
        if (hunyuanSword != null)
        {
            Effect effect = GameUtil.Instance.BattleEffectToItemUseEffect(BattleEffect.ReturnEnergy);
            effect.Value = hunyuanSword.equipDefine.takeEnergy;
            hunyuanSword.effects.Add(effect);
        }
    }

    private void HuanMie(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        timer.CreateTimer(TimerType.normal, casterID + "HuanMie", 2);
        disposablePool.SaveDisposable(casterID + "HuanMie", battleItem.defeatSubject.AsObservable().Where(_ => timer.TimerNext(casterID + "HuanMie")).Subscribe(_ =>
        {
            BattleManager.Instance.roundManager.extraRound++;
        }));
    }

    private void HuanMeng(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        disposablePool.SaveDisposable(casterID + "HuanMeng", battleItem.defeatSubject.AsObservable().Subscribe(_ =>
        {
            var battleItem = GlobalAccess.GetBattleItem(casterID);
            battleItem.buffCenter.AddBuff(DataManager.Instance.BuffDefines[1], casterID);
            GlobalAccess.SaveBattleItem(battleItem);
        }));
    }

    private void HuanShen(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        disposablePool.SaveDisposable(casterID + "HuanShen", battleItem.defeatSubject.AsObservable().Subscribe(_ =>
        {
            var battleItem = GlobalAccess.GetBattleItem(casterID);
            battleItem.attributes.currentEnergy += 1;
            GlobalAccess.SaveBattleItem(battleItem);
        }));
    }

    private void HuanYing(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        disposablePool.SaveDisposable(casterID + "HuanYing", battleItem.attributes.hpChangeSubject.AsObservable().Where(hp =>
        {
            var temp = GlobalAccess.GetBattleItem(casterID);
            return temp.attributes.currentHP < 0.3f * temp.attributes.MaxHP && hp + temp.attributes.currentHP > 0.3f * temp.attributes.MaxHP;
        }).Subscribe(hp =>
        {
            var temp = GlobalAccess.GetBattleItem(casterID);
            temp.buffCenter.AddBuff(DataManager.Instance.BuffDefines[1], casterID);
            GlobalAccess.SaveBattleItem(temp);
        }));
    }

    private void StrongBone(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        // 虽然这里是技能赋予的，也没有独立buff，但是是战斗开始赋予，结束时需要清除，所以算到buff里
        battleItem.attributes.Buff.Protection += 20;
        GlobalAccess.SaveBattleItem(battleItem);
        disposablePool.SaveDisposable(casterID + "StrongBone", battleItem.moveSubject.AsObservable().Subscribe(vect =>
        {
            var targetIDs = BattleManager.Instance.battleItemManager.GetBattleItemsByRange(vect, TargetRange.range_1, BattleItemType.player);
            foreach (var targetID in targetIDs)
            {
                var battleItem = GlobalAccess.GetBattleItem(casterID);
                var buffCopy = DataManager.Instance.BuffDefines[1].Copy();
                buffCopy.Value = 10;
                buffCopy.Duration = 2;
                battleItem.buffCenter.AddBuff(buffCopy, casterID);
                GlobalAccess.SaveBattleItem(battleItem);
            }
        }));
    }

    private void UnstoppableAspiration(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        timer.CreateTimer(TimerType.normal, casterID + "UnstoppableAspiration", 2);
        disposablePool.SaveDisposable(casterID + "UnstoppableAspiration", battleItem.attributes.hpChangeSubject.AsObservable().Where(hp =>
        {
            var temp = GlobalAccess.GetBattleItem(casterID);
            return timer.TimerNext(casterID + "UnstoppableAspiration") &&
                hp > 0.15f * temp.attributes.MaxHP;
        }).Subscribe(hp =>
        {
            var temp = GlobalAccess.GetBattleItem(casterID);
            BattleManager.Instance.ProcessNormalHealth("", new List<string> { casterID }, (int)(temp.attributes.MaxHP * 0.05));
        }));
    }

    private void WeAreBrother(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        battleItem.attributes.Skill.Taunt += 100;
        battleItem.attributes.LoadFinalAttributes();
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void TreeAngry(string casterID, PropertyType type, int value)
    {
        disposablePool.SaveDisposable(casterID + "TreeAngry", BattleManager.Instance.battleItemDamageSubject.AsObservable()
            .Where(pair => {
                switch (pair.Item4)
                {
                    case AttackStatus.normal:
                        return pair.Item1 == casterID && pair.Item6;
                    default:
                        return false;
                }
            }).Subscribe(pair =>
            {
                var caster = GlobalAccess.GetBattleItem(casterID);
                BattleManager.Instance.ProcessDirectAttack(casterID, pair.Item2, (int)(caster.attributes.lostHP * 0.1f));
            }));
    }

    private void MoneyToStrength(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        battleItem.attributes.Buff.Strength += (int)(GameManager.Instance.featherCoin.Value * (value / 100.0f));
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void RecoverBody(string casterID, PropertyType type, int value)
    {
        disposablePool.SaveDisposable(casterID + "RecoverBody",
            BattleManager.Instance.roundManager.roundTime.AsObservable().Where(roundTime =>
                {
                    var caster = GlobalAccess.GetBattleItem(casterID);
                    return roundTime.Item1 == casterID && roundTime.Item2 == RoundTime.end && !caster.haveAttackedInRound;
                }).Subscribe(roundTime =>
                {
                    var battleItem = GlobalAccess.GetBattleItem(casterID);
                    battleItem.attributes.currentShield += value;
                    GlobalAccess.SaveBattleItem(battleItem);
                })
        );
    }

    private void LuckyChance(string casterID, PropertyType type, int value)
    {
        //todo LuckyChance
    }

    private void AngryTexture(string casterID, PropertyType type, int value)
    {
        var caster = GlobalAccess.GetBattleItem(casterID);
        //战斗开始是一定是加0，所以可以不写
        //int currentHpPercent = (int)(caster.attributes.currentHP * 100.0f / caster.attributes.MaxHP);
        //caster.attributes.Buff.Strength += GameUtil.Instance.CalculateStrengthAngryTexture(currentHpPercent);
        disposablePool.SaveDisposable(casterID + "AngryTexture",
            caster.attributes.hpChangeSubject.AsObservable().Subscribe(hpChange =>
            {
                var battleItem = GlobalAccess.GetBattleItem(casterID);
                int lastHpPercent = (int)((battleItem.attributes.currentHP - hpChange) * 100.0f / battleItem.attributes.MaxHP);
                int currentHpPercent = (int)(battleItem.attributes.currentHP * 100.0f / battleItem.attributes.MaxHP);
                battleItem.attributes.Buff.Strength += GameUtil.Instance.CalculateStrengthAngryTexture(currentHpPercent) -
                    GameUtil.Instance.CalculateStrengthAngryTexture(lastHpPercent);
                GlobalAccess.SaveBattleItem(battleItem);
            })
        );
    }

    private void BleedEnchant(string casterID, PropertyType type, int value)
    {
        disposablePool.SaveDisposable(casterID + "BleedEnchant", BattleManager.Instance.battleItemDamageSubject.AsObservable()
            .Where(pair => {
                switch (pair.Item4)
                {
                    case AttackStatus.normal:
                        return pair.Item1 == casterID && pair.Item6;
                    default:
                        return false;
                }
            }).Subscribe(pair =>
            {
                var battleItem = GlobalAccess.GetBattleItem(casterID);
                battleItem.buffCenter.AddBuff(DataManager.Instance.BuffDefines[8], casterID);
                GlobalAccess.SaveBattleItem(battleItem);
            }));
    }

    private void MingDao(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        battleItem.avoidDeath = true;
        battleItem.avoidDeathFunc = MingDaoAction;
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void MingDaoAction(string targetID)
    {
        var battleItem = GlobalAccess.GetBattleItem(targetID);
        battleItem.avoidDeath = false;
        battleItem.attributes.currentHP = 1;
        battleItem.attributes.currentShield = 0;
        battleItem.buffCenter.AddBuff(DataManager.Instance.BuffDefines[10], targetID);
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void PoseidonBlessing(string casterID, PropertyType type, int value)
    {
        disposablePool.SaveDisposable(casterID + "PoseidonBlessing", BattleManager.Instance.battleItemDamageSubject.AsObservable()
            .Where(pair => {
                switch(pair.Item4)
                {
                    case AttackStatus.miss:
                    case AttackStatus.errorTarget:
                    case AttackStatus.toDeath:
                        return false;
                    case AttackStatus.normal:
                    case AttackStatus.block:
                        return pair.Item1 == casterID;
                    default:
                        throw new InvalidOperationException($"Unhandled enum value: {pair.Item4}");
                }
            }).Subscribe(pair =>
            {
                BattleCommonMethods.KnockbackTarget(pair.Item1, pair.Item2, value);
            }));
    }

    private void GoForward(string casterID, PropertyType type, int value)
    {
        disposablePool.SaveDisposable(casterID + "GoForward", BattleManager.Instance.battleItemDamageSubject.AsObservable()
            .Where(pair => {
                //自己被攻击并闪避
                return pair.Item2 == casterID && pair.Item4 == AttackStatus.miss;
            }).Subscribe(pair =>
            {
                BattleManager.Instance.ProcessDirectAttack(pair.Item2, pair.Item1, value);
            }));
    }

    private void BeforeDawn(string casterID, PropertyType type, int value)
    {
        timer.CreateTimer(TimerType.round, casterID + "BeforeDawn", 2);
        disposablePool.SaveDisposable(casterID + "BeforeDawn", BattleManager.Instance.battleItemDamageSubject.AsObservable()
            .Where(pair => {
                //自己的攻击暴击，且技能冷却完毕
                return pair.Item1 == casterID && pair.Item5 && timer.TimerNext(casterID + "BeforeDawn");
            }).Subscribe(pair =>
            {
                var battleItem = GlobalAccess.GetBattleItem(casterID);
                battleItem.attributes.currentEnergy += 1;
                GlobalAccess.SaveBattleItem(battleItem);
            }));
    }

    private void ExtremeOperation(string casterID, PropertyType type, int value)
    {
        timer.CreateTimer(TimerType.round, casterID + "ExtremeOperation", 1);
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        disposablePool.SaveDisposable(casterID + "ExtremeOperation", battleItem.lastEnergyAttackSubject.AsObservable()
            .Where(_ => {
                return timer.TimerNext(casterID + "ExtremeOperation");
            }).Subscribe(_ =>
            {
                var buffCopy = DataManager.Instance.BuffDefines[11].Copy();
                buffCopy.Value = value;
                buffCopy.Duration = 1;
                battleItem.buffCenter.AddBuff(buffCopy, casterID);
                GlobalAccess.SaveBattleItem(battleItem);
            }));
    }

    private void PrepareEscape(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        battleItem.avoidDeath = true;
        battleItem.avoidDeathFunc = PrepareEscapeAction;
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void PrepareEscapeAction(string targetID)
    {
        var battleItem = GlobalAccess.GetBattleItem(targetID);
        battleItem.avoidDeath = false;
        battleItem.attributes.currentHP = 1;
        battleItem.attributes.currentShield = 0;
        BattleManager.Instance.CharacterLeave(targetID);
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void VeryHeavy(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        battleItem.attributes.Skill.MaxHP += 50;
        battleItem.attributes.Skill.Defense += 50;
        battleItem.attributes.Skill.Strength -= 20;
        battleItem.attributes.Skill.Dodge -= 10;
        battleItem.attributes.Skill.Accuracy -= 10;
        battleItem.attributes.Skill.Speed -= 20;
        battleItem.attributes.LoadFinalAttributes();
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void ReinforceDefense(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        battleItem.reinforceDefense = true;
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void LookMe(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        battleItem.attributes.Skill.Taunt += (int)(battleItem.attributes.Taunt * value / 100.0f);
        battleItem.attributes.LoadFinalAttributes();
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void LastContribution(string casterID, PropertyType type, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(casterID);
        battleItem.avoidDeath = true;
        battleItem.avoidDeathFunc = LastContributionAction;
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void LastContributionAction(string targetID)
    {
        var battleItem = GlobalAccess.GetBattleItem(targetID);
        battleItem.avoidDeath = false;
        GlobalAccess.SaveBattleItem(battleItem);
        var targetIDs = BattleManager.Instance.battleItemManager.battleItemIDs.Where(id =>
        {
            var item = GlobalAccess.GetBattleItem(id);
            switch (item.battleItemType)
            {
                case BattleItemType.enemy:
                    return true;
                default:
                    return false;
            }
        }).ToList();
        foreach (var id in targetIDs)
        {
            BattleManager.Instance.ProcessDirectAttack(targetID, id, (int)(battleItem.attributes.MaxHP * 0.2f));
        }
        
    }

    private void ChangeProperty(string casterID, PropertyType type, int value)
    {
        //由于战斗中不能升级，所以提升属性的技能只可能对于characterModel生效
        var characterModel = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(casterID)).Value;
        switch (type)
        {
            case PropertyType.MaxHP:
                characterModel.attributes.Skill.MaxHP += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Health:
                Debug.Log("skill will not change HP");
                break;
            case PropertyType.Strength:
                characterModel.attributes.Skill.Strength += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Defense:
                characterModel.attributes.Skill.Defense += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Dodge:
                characterModel.attributes.Skill.Dodge += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Accuracy:
                characterModel.attributes.Skill.Accuracy += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Speed:
                characterModel.attributes.Skill.Speed += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Mobility:
                characterModel.attributes.Skill.Mobility += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Energy:
                characterModel.attributes.Skill.Energy += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Lucky:
                characterModel.attributes.Skill.Lucky += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Exp:
                characterModel.attributes.exp += value;
                break;
            case PropertyType.Shield:
                Debug.Log("skill will not change Shield");
                break;
            case PropertyType.Protection:
                characterModel.attributes.Skill.Protection += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            case PropertyType.EnchanceDamage:
                characterModel.attributes.Skill.EnchanceDamage += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Hematophagia:
                characterModel.attributes.Skill.Hematophagia += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            case PropertyType.DistanceDamage:
                characterModel.attributes.Skill.DistanceDamage += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            case PropertyType.AgainstDamage:
                characterModel.attributes.Skill.AgainstDamage += value;
                characterModel.attributes.LoadFinalAttributes();
                break;
            default:
                Debug.Log("unknown propertyType");
                break;
        }
        NorneStore.Instance.Update<CharacterModel>(characterModel, true);
    }
// ----------------------BattleEffect----------------------
    private void ReturnEnergy(string casterID, string targetID, Vector2 targetPos, int value)
    {
        if (casterID != null && casterID != "")
        {
            if (GlobalAccess.GetRandomRate_affected(20))
            {
                var battleItem = GlobalAccess.GetBattleItem(casterID);
                battleItem.attributes.currentEnergy += value;
                GlobalAccess.SaveBattleItem(battleItem);
            }
        }
    }
}
