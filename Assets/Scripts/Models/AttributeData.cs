using UnityEngine;
using System.Collections;

public class AttributeData
{
    public int[] data = new int[(int)AttributeType.MAX];
    /// <summary>
    /// 生命
    /// </summary>
    public int MaxHP { get { return data[(int)AttributeType.MaxHP]; } set { data[(int)AttributeType.MaxHP] = value; } }
    /// <summary>
    /// 饥饿度
    /// </summary>
    public int MaxHungry { get { return data[(int)AttributeType.MaxHungry]; } set { data[(int)AttributeType.MaxHungry] = value; } }
    /// <summary>
    /// 力量
    /// </summary>
    public int Strength { get { return data[(int)AttributeType.Strength]; } set { data[(int)AttributeType.Strength] = value; } }
    /// <summary>
    /// 法力
    /// </summary>
    public int Magic { get { return data[(int)AttributeType.Magic]; } set { data[(int)AttributeType.Magic] = value; } }
    /// <summary>
    /// 角色行动速度
    /// </summary>
    public int Speed { get { return data[(int)AttributeType.Speed]; } set { data[(int)AttributeType.Speed] = value; } }
    /// <summary>
    /// 行动力
    /// </summary>
    public int Mobility { get { return data[(int)AttributeType.Mobility]; } set { data[(int)AttributeType.Mobility] = value; } }
    /// <summary>
    /// 精力
    /// </summary>
    public int Energy { get { return data[(int)AttributeType.Energy]; } set { data[(int)AttributeType.Energy] = value; } }
    /// <summary>
    /// 嘲讽值
    /// </summary>
    public int Taunt { get { return data[(int)AttributeType.Taunt]; } set { data[(int)AttributeType.Taunt] = value; } }

    // just for deepcopy
    public AttributeData(){}

    public void Reset()
    {
        for (int i = 0; i < (int)AttributeType.MAX; i++)
        {
            this.data[i] = 0;
        }
    }

    /// <summary>
    /// 重载 + 运算符
    /// </summary>
    public static AttributeData operator +(AttributeData a, AttributeData b)
    {
        AttributeData result = new AttributeData();
        for (int i = 0; i < (int)AttributeType.MAX; i++)
        {
            var type = (AttributeType)i;
            result.SetAttr(type, a.GetAttr(type) + b.GetAttr(type));
        }
        return result;
    }

    public void SetAttr(AttributeType type, int value)
    {
        this.data[(int)type] = value;
    }

    public int GetAttr(AttributeType type)
    {
        return this.data[(int)type];
    }

    public void SetAttr(PropertyType type, int value)
    {
        var aType = GlobalAccess.PropertyTypeToAttributeType(type);
        if (aType != AttributeType.None)
        {
            this.data[(int)aType] = value;
        }
    }

    public int GetAttr(PropertyType type)
    {
        var aType = GlobalAccess.PropertyTypeToAttributeType(type);
        if (aType != AttributeType.None)
        {
            return this.data[(int)aType];
        }
        return -1;
    }
}

