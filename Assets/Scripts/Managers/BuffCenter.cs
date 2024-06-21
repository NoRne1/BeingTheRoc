using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniRx;

public class BuffCenter
{
    private string ownerID;
    // (string, string): (buff.ID, casterID)
    private Dictionary<(int, string), BuffModel> buffs = new Dictionary<(int, string), BuffModel>();

    public BuffCenter(string ownerID)
    {
        this.ownerID = ownerID;
    }

    public void AddBuff(BuffDefine buffDefine, string casterID)
    {
        BuffModel buff = new BuffModel(buffDefine, ownerID, casterID);
        switch (buffDefine.MergeType)
        {
            case BuffMergeType.single:
                var keys = buffs.Keys.Where(key => key.Item1 == buff.ID).ToList();
                if (keys.Count > 0)
                {
                    foreach (var key in keys)
                    {
                        buffs.Remove(key);
                    }
                    buffs[(buff.ID, "")] = buff;
                } else
                {
                    buffs[(buff.ID, "")] = buff;
                    if (buff.InvokeTime == BuffInvokeTime.constant)
                    {
                        BuffManager.Instance.InvokeBuff(buff, true);
                    }
                }
                break;
            case BuffMergeType.normal:
                // 同一角色直接覆盖, 不同角色互不影响
                if (buffs.ContainsKey((buff.ID, casterID)))
                {
                    buffs[(buff.ID, casterID)] = buff;
                } else
                {
                    buffs[(buff.ID, casterID)] = buff;
                    if (buff.InvokeTime == BuffInvokeTime.constant)
                    {
                        BuffManager.Instance.InvokeBuff(buff, true);
                    }
                }
                break;
            case BuffMergeType.normalMerge:
                // 同一角色buff合并，持续时间刷新,不同角色互不影响
                if (buffs.ContainsKey((buff.ID, casterID)))
                {
                    buffs[(buff.ID, casterID)].num ++;
                    buffs[(buff.ID, casterID)].Duration = buff.Duration;
                } else
                {
                    buffs[(buff.ID, casterID)] = buff;
                    if (buff.InvokeTime == BuffInvokeTime.constant)
                    {
                        BuffManager.Instance.InvokeBuff(buff, true);
                    }
                }
                break;
            case BuffMergeType.merge:
                // 同一buff合并，持续时间刷新
                var keys2 = buffs.Keys.Where(key => key.Item1 == buff.ID).ToList();
                if (keys2.Count > 0)
                {
                    buffs[keys2[0]].num++;
                    buffs[keys2[0]].Duration = buff.Duration;
                } else
                {
                    buffs[(buff.ID, "")] = buff;
                    if (buff.InvokeTime == BuffInvokeTime.constant)
                    {
                        BuffManager.Instance.InvokeBuff(buff, true);
                    }
                }
                break;
        }
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
    }

    public void TurnBegin()
    {
        //执行
        var temp = buffs.Values.Where(buff => buff.InvokeTime == BuffInvokeTime.turnBegin).ToList();
        if (temp.Count > 0)
        {
            foreach (var buff in temp)
            {
                BuffManager.Instance.InvokeBuff(buff);
            }
        }
        //减回合
        var temp2 = buffs.Values.Where(buff => buff.DecreaseTime == BuffDecreaseTime.turnBegin).ToList();
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

    public void TurnEnd()
    {
        //执行
        var temp = buffs.Values.Where(buff => buff.InvokeTime == BuffInvokeTime.turnEnd).ToList();
        if (temp.Count > 0)
        {
            foreach (var buff in temp)
            {
                BuffManager.Instance.InvokeBuff(buff);
            }
        }
        //减回合
        var temp2 = buffs.Values.Where(buff => buff.DecreaseTime == BuffDecreaseTime.turnEnd).ToList();
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
}
