using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffInvokeTime
{
    none = -1,
    roundBegin = 0, // 回合开始触发
    roundEnd = 1, // 回合结束触发
    constant = 2, // 持续型，施加或移除buff时触发
    move = 3, // 移动时触发
}

public enum BuffDecreaseTime
{
    none = -1,
    roundBegin = 0, // 回合开始触发
    roundEnd = 1, // 回合结束触发
}

public enum BuffMergeType
{
    none = -1,
    single = 0, // 独一无二的，重复添加更新持续时间（眩晕等）
    normal = 1, // 常规buff，同一角色施加的更新时间，不同角色施加的共存(灼伤，减防等)
    normalMerge = 2, // 合并，同一角色施加的合并，不同角色施加的共存（特殊可叠加的，如风化）
    merge = 3, // 合并，同一角色或不同角色施加的都合并（虚无等）
}


public class BuffDefine
{
    // ID
    public int ID { get; set; }
    // 名字
    public string Name { get; set; }
    // 描述
    public string Description { get; set; }
    // 图标
    public string Resource { get; set; }
    // 方法名
    public string MethodName { get; set; }
    public PropertyType PropertyType { get; set; }
    public int Value { get; set; }

    // buff的持续时间
    public int Duration { get; set; }

    public BuffInvokeTime InvokeTime { get; set; }
    public BuffDecreaseTime DecreaseTime { get; set; }
    public BuffMergeType MergeType { get; set; }

    public BuffDefine Copy()
    {
        BuffDefine copy = new BuffDefine();
        copy.ID = this.ID;
        copy.Name = this.Name;
        copy.Description = this.Description;
        copy.Resource = this.Resource;
        copy.MethodName = this.MethodName;
        copy.PropertyType = this.PropertyType;
        copy.Value = this.Value;
        copy.Duration = this.Duration;
        copy.InvokeTime = this.InvokeTime;
        copy.DecreaseTime = this.DecreaseTime;
        copy.MergeType = this.MergeType;
        return copy;
    }
}
