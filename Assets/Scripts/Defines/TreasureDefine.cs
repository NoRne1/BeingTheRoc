using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TreasureInvokeType
{
    none = -1,
    normalUse = 0, // 非战斗时使用
    battleUse = 1, // 战斗时使用
    battleStart = 2, // 战斗开始时触发
}

public class TreasureDefine
{
    public int ID { get; set; }
    public TreasureInvokeType invokeType { get; set; }
    public string methodName { get; set; }

    public TreasureDefine Copy()
    {
        TreasureDefine copy = (TreasureDefine)this.MemberwiseClone();

        return copy;
    }
}
