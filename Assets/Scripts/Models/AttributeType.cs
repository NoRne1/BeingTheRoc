﻿using UnityEngine;
using System.Collections;

public enum AttributeType
{
    None = -1,
    /// <summary>
    /// 生命
    /// </summary>
    MaxHP = 0,
    /// <summary>
    /// 力量
    /// </summary>
    Strength = 1,
    /// <summary>
    /// 防御
    /// </summary>
    Defense = 2,
    /// <summary>
    /// 闪避
    /// </summary>
    Dodge = 3,
    /// <summary>
    /// 命中
    /// </summary>
    Accuracy = 4,
    /// <summary>
    /// 角色行动速度
    /// </summary>
    Speed = 5,
    /// <summary>
    /// 行动力
    /// </summary>
    Mobility = 6,
    /// <summary>
    /// 精力
    /// </summary>
    Energy = 7,
    /// <summary>
    /// 幸运
    /// </summary>
    Lucky = 8,
    MAX
}
