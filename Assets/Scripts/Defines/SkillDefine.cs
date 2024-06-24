using UnityEngine;
using System.Collections;


public enum SkillInvokeType
{
    none = -1, // 不触发
    instant = 0, // 获得立即生效
    battleStart = 1, // 战斗开始时触发
    //equip = 1, // 装备生效
}

public class SkillDefine
{
    public int ID { get; set; }
    public string Title { get; set; }
    public JobType Job { get; set; }
    public string Resource { get; set; }
    public SkillInvokeType InvokeType { get; set; }
    public string MethodName { get; set; }
    public string Desc { get; set; }
    public int ExtraEntry1 { get; set; }
    public int ExtraEntry2 { get; set; }
    public int ExtraEntry3 { get; set; }
}

