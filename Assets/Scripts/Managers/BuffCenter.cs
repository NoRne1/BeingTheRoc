using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniRx;

public class BuffCenter
{
    private string ownerID;
    // (int, string): (buff.ID, casterID)
    private Dictionary<(int, string), BuffModel> buffs = new Dictionary<(int, string), BuffModel>();
    public Subject<Unit> battleItemUpdate;

    public BuffCenter(string ownerID, Subject<Unit> subject)
    {
        this.ownerID = ownerID;
        battleItemUpdate = subject;
    }

    public void AddBuff(BuffModel buffModel)
    {
        switch (buffModel.MergeType)
        {
            case BuffMergeType.single:
                var keys = buffs.Keys.Where(key => key.Item1 == buffModel.ID).ToList();
                if (keys.Count > 0)
                {
                    foreach (var key in keys)
                    {
                        buffs.Remove(key);
                    }
                    buffs[(buffModel.ID, "")] = buffModel;
                } else
                {
                    buffs[(buffModel.ID, "")] = buffModel;
                    if (buffModel.InvokeTime == BuffInvokeTime.constant)
                    {
                        BuffManager.Instance.InvokeBuff(buffModel, true);
                    }
                }
                break;
            case BuffMergeType.normal:
                // 同一角色直接覆盖, 不同角色互不影响
                if (buffs.ContainsKey((buffModel.ID, buffModel.casterID)))
                {
                    buffs[(buffModel.ID, buffModel.casterID)] = buffModel;
                } else
                {
                    buffs[(buffModel.ID, buffModel.casterID)] = buffModel;
                    if (buffModel.InvokeTime == BuffInvokeTime.constant)
                    {
                        BuffManager.Instance.InvokeBuff(buffModel, true);
                    }
                }
                break;
            case BuffMergeType.normalMerge:
                // 同一角色buff合并，持续时间刷新,不同角色互不影响
                if (buffs.ContainsKey((buffModel.ID, buffModel.casterID)))
                {
                    buffs[(buffModel.ID, buffModel.casterID)].num ++;
                    buffs[(buffModel.ID, buffModel.casterID)].Duration = buffModel.Duration;
                } else
                {
                    buffs[(buffModel.ID, buffModel.casterID)] = buffModel;
                    if (buffModel.InvokeTime == BuffInvokeTime.constant)
                    {
                        BuffManager.Instance.InvokeBuff(buffModel, true);
                    }
                }
                break;
            case BuffMergeType.merge:
                // 同一buff合并，持续时间刷新
                var keys2 = buffs.Keys.Where(key => key.Item1 == buffModel.ID).ToList();
                if (keys2.Count > 0)
                {
                    buffs[keys2[0]].num++;
                    buffs[keys2[0]].Duration = buffModel.Duration;
                } else
                {
                    buffs[(buffModel.ID, "")] = buffModel;
                    if (buffModel.InvokeTime == BuffInvokeTime.constant)
                    {
                        BuffManager.Instance.InvokeBuff(buffModel, true);
                    }
                }
                break;
        }
        battleItemUpdate.OnNext(Unit.Default);
    }

    public void AddBuff(BuffDefine buffDefine, string casterID)
    {
        this.AddBuff(new BuffModel(buffDefine, ownerID, casterID, BuffType.normal));
    }

    //public void RemoveBuff(int buffDefineID)
    //{
    //    var keys = buffs.Keys.Where(key => key.Item1 == buffDefineID).ToList();
    //    if (keys.Count > 0)
    //    {
    //        foreach (var key in keys)
    //        {
    //            buffs.Remove(key);
    //        }
    //    }
    //}

    public void RemoveBuff(string buffuuID)
    {
        var pairs = buffs.Where(pair => pair.Value.uuId == buffuuID).ToList();
        if (pairs.Count > 0)
        {
            foreach (var pair in pairs)
            {
                buffs.Remove(pair.Key);
                if (pair.Value.InvokeTime == BuffInvokeTime.constant)
                {
                    BuffManager.Instance.InvokeBuff(pair.Value, false);
                }
            }
        }
        battleItemUpdate.OnNext(Unit.Default);
    }

    public void RemoveBuff(BuffType buffType)
    {
        var pairs = buffs.Where(pair => pair.Value.buffType == buffType).ToList();
        if (pairs.Count > 0)
        {
            foreach (var pair in pairs)
            {
                buffs.Remove(pair.Key);
                if (pair.Value.InvokeTime == BuffInvokeTime.constant)
                {
                    BuffManager.Instance.InvokeBuff(pair.Value, false);
                }
            }
        }
        battleItemUpdate.OnNext(Unit.Default);
    }

    public void RoundBegin()
    {
        //执行
        var temp = buffs.Values.Where(buff => buff.InvokeTime == BuffInvokeTime.roundBegin).ToList();
        if (temp.Count > 0)
        {
            foreach (var buff in temp)
            {
                BuffManager.Instance.InvokeBuff(buff);
            }
        }
        //减回合
        var temp2 = buffs.Values.Where(buff => buff.DecreaseTime == BuffDecreaseTime.roundBegin).ToList();
        if (temp2.Count > 0)
        {
            foreach (var buff in temp2)
            {
                buff.Duration = Mathf.Max(0, buff.Duration--);
                if (buff.Duration <= 0)
                {
                    RemoveBuff(buff.uuId);
                }
            }
            battleItemUpdate.OnNext(Unit.Default);
        }
    }

    //public void TurnActing()
    //{
    //    //执行
    //    var temp = buffs.Values.Where(buff => buff.InvokeTime == BuffInvokeTime.turnBegin).ToList();
    //    if (temp.Count > 0)
    //    {
    //        foreach (var buff in temp)
    //        {
    //            BuffManager.Instance.InvokeBuff(buff);
    //        }
    //    }
    //    //减回合
    //    var temp2 = buffs.Values.Where(buff => buff.DecreaseTime == BuffDecreaseTime.turnBegin).ToList();
    //    if (temp2.Count > 0)
    //    {
    //        foreach (var buff in temp2)
    //        {
    //            buff.Duration = Mathf.Max(0, buff.Duration--);
    //            if (buff.Duration <= 0)
    //            {
    //                RemoveBuff(buff.uuId);
    //            }
    //        }
    //    }
    //}

    public void RoundEnd()
    {
        //执行
        var temp = buffs.Values.Where(buff => buff.InvokeTime == BuffInvokeTime.roundEnd).ToList();
        if (temp.Count > 0)
        {
            foreach (var buff in temp)
            {
                BuffManager.Instance.InvokeBuff(buff);
            }
        }
        //减回合
        var temp2 = buffs.Values.Where(buff => buff.DecreaseTime == BuffDecreaseTime.roundEnd).ToList();
        if (temp2.Count > 0)
        {
            foreach (var buff in temp2)
            {
                buff.Duration = Mathf.Max(0, buff.Duration--);
                if (buff.Duration <= 0)
                {
                    RemoveBuff(buff.uuId);
                }
            }
            battleItemUpdate.OnNext(Unit.Default);
        }
    }

    public void CharacterMove(int distance)
    {
        var temp = buffs.Values.Where(buff => buff.InvokeTime == BuffInvokeTime.move).ToList();
        if (temp.Count > 0)
        {
            foreach (var buff in temp)
            {
                BuffManager.Instance.InvokeBuff(buff, distance);
            }
        }
    }

    public List<BuffModel> GetNewestBuffs(int num)
    {
        if (num == 0)
        {
            return new List<BuffModel>(); // 返回一个空列表
        }
        if (num < 0)
        {
            return buffs.Values.ToList(); // 返回全部buff
        }

        int start = Math.Max(0, buffs.Values.Count - num);
        return buffs.Values.Skip(start).ToList();
    }
}
