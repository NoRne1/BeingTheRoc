using UnityEngine;
using System.Collections;

public enum AttributeType
{
    None = -1,
    /// <summary>
    /// 生命
    /// </summary>
    MaxHP = 0,
    /// <summary>
    /// 饥饿度
    /// </summary>
    MaxHungry = 1,
    /// <summary>
    /// 力量
    /// </summary>
    Strength = 2,
    /// <summary>
    /// 法力
    /// </summary>
    Magic = 3,
    /// <summary>
    /// 角色行动速度
    /// </summary>
    Speed = 4,
    /// <summary>
    /// 行动力
    /// </summary>
    Mobility = 5,
    /// <summary>
    /// 精力
    /// </summary>
    Energy = 6,
    /// <summary>
    /// 嘲讽值
    /// </summary>
    Taunt = 7,
    MAX
}

