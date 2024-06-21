using UnityEngine;
using System.Reflection;
using System.Linq;
using UniRx;
using System.Collections.Generic;
using System;

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
    
    private void Trauma(BuffModel buff, bool addOrRemove)
    {
        if (addOrRemove)
        {
            disposables.Add(buff.uuId, BattleManager.Instance.battleItemDamageSubject.AsObservable().Where(pair => pair.Item2 == buff.ownerID && pair.Item4).Subscribe(pair =>
            {
                BattleManager.Instance.ProcessDamage(buff.casterID ,buff.ownerID, buff.num);
            }));
        } else if (disposables.ContainsKey(buff.uuId))
        {
            disposables[buff.uuId].Dispose();
        }
    }

    private void MentalPower(BuffModel buff, bool addOrRemove)
    {
        if (addOrRemove)
        {
            disposables.Add(buff.uuId, BattleManager.Instance.battleItemDamageSubject.AsObservable().Where(pair => pair.Item1 == buff.ownerID && pair.Item4).Subscribe(pair =>
            {
                BattleManager.Instance.ProcessDamage(buff.ownerID, pair.Item2, buff.num);
            }));
        }
        else if (disposables.ContainsKey(buff.uuId))
        {
            disposables[buff.uuId].Dispose();
        }
    }

    private void Corrosion(BuffModel buff)
    {
        BattleManager.Instance.ProcessDamage(buff.casterID, buff.ownerID, buff.num);
    }

    private void Dizzy(BuffModel buff)
    {
        var battleItem = GlobalAccess.GetBattleItem(buff.ownerID);
        battleItem.canActing = false;
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void Burning(BuffModel buff)
    {
        BattleManager.Instance.ProcessAttack(buff.casterID, new List<string> { buff.ownerID }, buff.Value);
    }

    private void ChangeProperty(BuffModel buff, bool addOrRemove)
    {
        var battleItem = GlobalAccess.GetBattleItem(buff.ownerID);
        switch (buff.PropertyType)
        {
            case PropertyType.MaxHP:
                battleItem.MaxHP += buff.Value;
                battleItem.currentHP += buff.Value;
                break;
            case PropertyType.HP:
                BattleManager.Instance.ProcessHealth(buff.casterID, new List<string> { buff.ownerID }, buff.Value);
                break;
            case PropertyType.Strength:
                battleItem.Strength += buff.Value;
                break;
            case PropertyType.Defense:
                battleItem.Defense += buff.Value;
                break;
            case PropertyType.Dodge:
                battleItem.Dodge += buff.Value;
                break;
            case PropertyType.Accuracy:
                battleItem.Accuracy += buff.Value;
                break;
            case PropertyType.Speed:
                battleItem.Speed += buff.Value;
                break;
            case PropertyType.Mobility:
                battleItem.Mobility += buff.Value;
                break;
            case PropertyType.Energy:
                battleItem.currentEnergy += buff.Value;
                break;
            case PropertyType.Lucky:
                battleItem.Lucky += buff.Value;
                break;
            case PropertyType.Exp:
                battleItem.exp += buff.Value;
                break;
            case PropertyType.shield:
                battleItem.shield += buff.Value;
                break;
            default:
                Debug.Log("unknown propertyType");
                break;
        }
        GlobalAccess.SaveBattleItem(battleItem);
    }

    private void Bleeding(BuffModel buff, int distance)
    {
        BattleManager.Instance.ProcessDamage(buff.casterID, buff.ownerID, buff.num * distance);
    }
}

