﻿using System;
using System.Collections.Generic;
using System.Text;

//用于技能池的区分
public enum JobType
{
    General = 0,
    //骁战
    Warrior = 1,
    //奥术
    Magician = 2,
    //援御
    Tank = 3,
    //影卫
    Assassin = 4,
    //灵祈
    Assistance = 5,
    Special = 6,
}

/// <summary>
/// 角色定义
    /*"1": {"ID":1,
     * "Name":"大红",
     * "MaxHP":300,
     * "Strength":20,
     * "Defense":20,
     * "Dodge":0,
     * "Accuracy":0,
     * "Speed":100,
     * "Mobility":2
     * "Energy":2
     * }...
    */
/// </summary>
public class CharacterDefine
{
    //TID
    public int ID { get; set; }

    public JobType Job { get; set; }

    public GeneralLevel Level { get; set; }
    //角色名字
    public string Name { get; set; }
    //种族
    public string Race { get; set; }

    public int MaxHungry{ get; set; }

    //基本属性
    /// <summary>
    /// 生命
    /// </summary>
    public int MaxHP { get; set; }
    /// <summary>
    /// 力量
    /// </summary>
    public int Strength { get; set; }
    /// <summary>
    /// 防御
    /// </summary>
    public int Defense { get; set; }
    /// <summary>
    /// 闪避
    /// </summary>
    public int Dodge { get; set; }
    /// <summary>
    /// 命中
    /// </summary>
    public int Accuracy { get; set; }
    /// <summary>
    /// 角色行动速度
    /// </summary>
    public int Speed { get; set; }
    /// <summary>
    /// 行动力
    /// </summary>
    public int Mobility { get; set; }
    /// <summary>
    /// 精力
    /// </summary>
    public int Energy { get; set; }
    /// <summary>
    /// 幸运
    /// </summary>
    public int Lucky { get; set; }

    public int MaxHPFloat { get; set; }
    public int StrengthFloat { get; set; }
    public int DefenseFloat { get; set; }
    public int SpeedFloat { get; set; }

    /// <summary>
    /// 图片资源
    /// </summary>
    public string Resource { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Desc { get; set; }

    public int BornSkill { get; set; }
    public int Skill1 { get; set; }
    public int Skill2 { get; set; }
    public int Skill3 { get; set; }
}
