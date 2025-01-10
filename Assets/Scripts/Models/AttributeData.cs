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
    /// 力量
    /// </summary>
    public int Strength { get { return data[(int)AttributeType.Strength]; } set { data[(int)AttributeType.Strength] = value; } }
    /// <summary>
    /// 防御
    /// </summary>
    public int Defense { get { return data[(int)AttributeType.Defense]; } set { data[(int)AttributeType.Defense] = value; } }
    /// <summary>
    /// 闪避
    /// </summary>
    public int Dodge { get { return data[(int)AttributeType.Dodge]; } set { data[(int)AttributeType.Dodge] = value; } }
    /// <summary>
    /// 命中
    /// </summary>
    public int Accuracy { get { return data[(int)AttributeType.Accuracy]; } set { data[(int)AttributeType.Accuracy] = value; } }
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
    /// 幸运
    /// </summary>
    public int Lucky { get { return data[(int)AttributeType.Lucky]; } set { data[(int)AttributeType.Lucky] = value; } }
    /// <summary>
    /// 减伤
    /// </summary>
    public int Protection { get { return data[(int)AttributeType.Protection]; } set { data[(int)AttributeType.Protection] = value; } }
    /// <summary>
    /// 增伤
    /// </summary>
    public int EnchanceDamage { get { return data[(int)AttributeType.EnchanceDamage]; } set { data[(int)AttributeType.EnchanceDamage] = value; } }
    /// <summary>
    /// 嘲讽值
    /// </summary>
    public int Taunt { get { return data[(int)AttributeType.Taunt]; } set { data[(int)AttributeType.Taunt] = value; } }
    /// <summary>
    /// 吸血
    /// </summary>
    public int Hematophagia { get { return data[(int)AttributeType.Hematophagia]; } set { data[(int)AttributeType.Hematophagia] = value; } }
    /// <summary>
    /// 距离增伤
    /// </summary>
    public int DistanceDamage { get { return data[(int)AttributeType.DistanceDamage]; } set { data[(int)AttributeType.DistanceDamage] = value; } }
    /// <summary>
    /// 反伤
    /// </summary>
    public int AgainstDamage { get { return data[(int)AttributeType.AgainstDamage]; } set { data[(int)AttributeType.AgainstDamage] = value; } }

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

