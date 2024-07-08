using UnityEngine;
using System.Reflection;
using System.Linq;
using UniRx;
using System.Collections.Generic;
using System;
using static UnityEngine.GraphicsBuffer;

public class BuffManager : MonoSingleton<BuffManager>
{
    private Dictionary<string, IDisposable> disposables = new Dictionary<string, IDisposable>();
    // Use this for initialization
    void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
			
	}

    // for normal
    public void InvokeBuff(BuffModel buff)
    {
        var method = typeof(BuffManager).GetMethod(buff.MethodName);
        object[] parameters = new object[] { buff };
        method?.Invoke(BuffManager.Instance, parameters);
    }

    // for BuffInvokeTime.constant
    public void InvokeBuff(BuffModel buff, bool addOrRemove)
    {
        var method = typeof(BuffManager).GetMethod(buff.MethodName);
        object[] parameters = new object[] { buff, addOrRemove };
        method?.Invoke(BuffManager.Instance, parameters);
    }

    // for BuffInvokeTime.move
    public void InvokeBuff(BuffModel buff, int distance)
    {
        var method = typeof(BuffManager).GetMethod(buff.MethodName);
        object[] parameters = new object[] { buff, distance };
        method?.Invoke(BuffManager.Instance, parameters);
    }

    private void Nothingness(BuffModel buff)
    {
        buff.num++;
        if (buff.num >= 5)
        {
            BattleManager.Instance.CharacterDie(buff.ownerID);
        }
    }

    private void Invisible(BuffModel buff, bool addOrRemove)
    {
        var battleItem = GlobalAccess.GetBattleItem(buff.ownerID);
        battleItem.isInvisible = addOrRemove;
        GlobalAccess.SaveBattleItem(battleItem);
    }

    //外伤，敌方的debuff，需要击伤才可以触发
    private void Trauma(BuffModel buff, bool addOrRemove)
    {
        if (addOrRemove)
        {
            disposables.Add(buff.uuId, BattleManager.Instance.battleItemDamageSubject.AsObservable()
                .Where(pair => {
                    switch (pair.Item4)
                    {
                        case AttackStatus.normal:
                            return pair.Item2 == buff.ownerID && pair.Item6;
                        default:
                            return false;
                    }
                }) // 只有击伤且不造成死亡才触发外伤
                .Subscribe(pair =>
            {
                BattleManager.Instance.ProcessDirectAttack(buff.casterID ,buff.ownerID, buff.num);
            }));
        } else if (disposables.ContainsKey(buff.uuId))
        {
            disposables[buff.uuId].Dispose();
        }
    }

    //念力，自身的buff，不需要击伤就可以触发
    private void MentalPower(BuffModel buff, bool addOrRemove)
    {
        if (addOrRemove)
        {
            disposables.Add(buff.uuId, BattleManager.Instance.battleItemDamageSubject.AsObservable()
                .Where(pair => {
                        switch (pair.Item4)
                        {
                            case AttackStatus.normal:
                                return pair.Item1 == buff.ownerID && pair.Item6;
                            default:
                                return false;
                        }
                    })
                .Subscribe(pair =>
            {
                BattleManager.Instance.ProcessDirectAttack(buff.ownerID, pair.Item2, buff.num);
            }));
        }
        else if (disposables.ContainsKey(buff.uuId))
        {
            disposables[buff.uuId].Dispose();
        }
    }

    private void Corrosion(BuffModel buff)
    {
        BattleManager.Instance.ProcessDirectAttack(buff.casterID, buff.ownerID, buff.num);
    }

    private void Dizzy(BuffModel buff)
    {
        var battleItem = GlobalAccess.GetBattleItem(buff.ownerID);
        battleItem.canActing = false;
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void Burning(BuffModel buff)
    {
        BattleManager.Instance.ProcessNormalAttack(buff.casterID, new List<string> { buff.ownerID }, buff.Value);
    }

    private void Invincible(BuffModel buff, bool addOrRemove)
    {
        if (addOrRemove)
        {
            var battleItem = GlobalAccess.GetBattleItem(buff.ownerID);
            battleItem.isInvincible = true;
            GlobalAccess.SaveBattleItem(battleItem);
        }
        else
        {
            var battleItem = GlobalAccess.GetBattleItem(buff.ownerID);
            battleItem.isInvincible = false;
            GlobalAccess.SaveBattleItem(battleItem);
        }
    }

    private void ChangeProperty(BuffModel buff, bool addOrRemove = true)
    {
        var battleItem = GlobalAccess.GetBattleItem(buff.ownerID);
        switch (buff.PropertyType)
        {
            case PropertyType.MaxHP:
                battleItem.attributes.Buff.MaxHP += addOrRemove ? buff.Value : -buff.Value;
                battleItem.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Health:
                //加血不存在返还
                if (addOrRemove)
                {
                    BattleManager.Instance.ProcessNormalHealth(buff.casterID, new List<string> { buff.ownerID }, buff.Value);
                }
                break;
            case PropertyType.Strength:
                battleItem.attributes.Buff.Strength += addOrRemove ? buff.Value : -buff.Value;
                battleItem.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Defense:
                battleItem.attributes.Buff.Defense += addOrRemove ? buff.Value : -buff.Value;
                battleItem.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Dodge:
                battleItem.attributes.Buff.Dodge += addOrRemove ? buff.Value : -buff.Value;
                battleItem.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Accuracy:
                battleItem.attributes.Buff.Accuracy += addOrRemove ? buff.Value : -buff.Value;
                battleItem.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Speed:
                battleItem.attributes.Buff.Speed += addOrRemove ? buff.Value : -buff.Value;
                battleItem.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Mobility:
                battleItem.attributes.Buff.Mobility += addOrRemove ? buff.Value : -buff.Value;
                battleItem.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Energy:
                battleItem.attributes.Buff.Energy += addOrRemove ? buff.Value : -buff.Value;
                battleItem.attributes.LoadFinalAttributes();
                // 当前精力只加不减
                if (addOrRemove)
                {
                    battleItem.attributes.currentEnergy += buff.Value;
                }
                break;
            case PropertyType.Lucky:
                battleItem.attributes.Buff.Lucky += addOrRemove ? buff.Value : -buff.Value;
                battleItem.attributes.LoadFinalAttributes();
                break;
            case PropertyType.Exp:
                // 当前经验只加不减
                if (addOrRemove)
                {
                    battleItem.attributes.exp += buff.Value;
                }
                break;
            case PropertyType.Shield:
                // 当前护盾只加不减
                if (addOrRemove)
                {
                    battleItem.attributes.currentShield += buff.Value;
                }
                break;
            case PropertyType.Protection:
                battleItem.attributes.Buff.Protection += addOrRemove ? buff.Value : -buff.Value;
                battleItem.attributes.LoadFinalAttributes();
                break;
            case PropertyType.EnchanceDamage:
                battleItem.attributes.Buff.EnchanceDamage += addOrRemove ? buff.Value : -buff.Value;
                battleItem.attributes.LoadFinalAttributes();
                break;
            default:
                Debug.Log("unknown propertyType");
                break;
        }
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void Bleeding(BuffModel buff, int distance)
    {
        BattleManager.Instance.ProcessDirectAttack(buff.casterID, buff.ownerID, buff.num * distance);
    }

    private void Silent(BuffModel buff, bool addOrRemove)
    {
        if (addOrRemove)
        {
            var battleItem = GlobalAccess.GetBattleItem(buff.ownerID);
            battleItem.isSilent = true;
            GlobalAccess.SaveBattleItem(battleItem);
        }
        else
        {
            var battleItem = GlobalAccess.GetBattleItem(buff.ownerID);
            battleItem.isSilent = false;
            GlobalAccess.SaveBattleItem(battleItem);
        }
    }

    private void Confine(BuffModel buff, bool addOrRemove)
    {
        if (addOrRemove)
        {
            var battleItem = GlobalAccess.GetBattleItem(buff.ownerID);
            battleItem.isConfine = true;
            GlobalAccess.SaveBattleItem(battleItem);
        }
        else
        {
            var battleItem = GlobalAccess.GetBattleItem(buff.ownerID);
            battleItem.isConfine = false;
            GlobalAccess.SaveBattleItem(battleItem);
        }
    }
}

