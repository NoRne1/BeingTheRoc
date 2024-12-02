using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeEquipDefine
{
    public int ID { get; set; }
    public int equipA { get; set; }
    public int equipB { get; set; }
    public int equipResult { get; set; }
}

public class MergeEquipInfo
{
    public int equipA { get; set; }
    public int equipB { get; set; }
    public int equipResult { get; set; }
    public MergeEquipInfo(int equipA, int equipB, int equipResult)
    {
        this.equipA = equipA;
        this.equipB = equipB;
        this.equipResult = equipResult;
    }
}
