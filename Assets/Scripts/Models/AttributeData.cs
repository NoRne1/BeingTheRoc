using UnityEngine;
using System.Collections;

public class AttributeData
{
    public int[] Data = new int[(int)AttributeType.MAX];
    /// <summary>
    /// 生命
    /// </summary>
    public int MaxHP { get { return Data[(int)AttributeType.MaxHP]; } set { Data[(int)AttributeType.MaxHP] = value; } }
    /// <summary>
    /// 力量
    /// </summary>
    public int Strength { get { return Data[(int)AttributeType.Strength]; } set { Data[(int)AttributeType.Strength] = value; } }
    /// <summary>
    /// 防御
    /// </summary>
    public int Defense { get { return Data[(int)AttributeType.Defense]; } set { Data[(int)AttributeType.Defense] = value; } }
    /// <summary>
    /// 闪避
    /// </summary>
    public int Dodge { get { return Data[(int)AttributeType.Dodge]; } set { Data[(int)AttributeType.Dodge] = value; } }
    /// <summary>
    /// 命中
    /// </summary>
    public int Accuracy { get { return Data[(int)AttributeType.Accuracy]; } set { Data[(int)AttributeType.Accuracy] = value; } }
    /// <summary>
    /// 角色行动速度
    /// </summary>
    public int Speed { get { return Data[(int)AttributeType.Speed]; } set { Data[(int)AttributeType.Speed] = value; } }
    /// <summary>
    /// 行动力
    /// </summary>
    public int Mobility { get { return Data[(int)AttributeType.Mobility]; } set { Data[(int)AttributeType.Mobility] = value; } }
    /// <summary>
    /// 精力
    /// </summary>
    public int Energy { get { return Data[(int)AttributeType.Energy]; } set { Data[(int)AttributeType.Energy] = value; } }
    /// <summary>
    /// 幸运
    /// </summary>
    public int Lucky { get { return Data[(int)AttributeType.Lucky]; } set { Data[(int)AttributeType.Lucky] = value; } }
    /// <summary>
    /// 减伤
    /// </summary>
    public int Protection { get { return Data[(int)AttributeType.Protection]; } set { Data[(int)AttributeType.Protection] = value; } }
    /// <summary>
    /// 增伤
    /// </summary>
    public int EnchanceDamage { get { return Data[(int)AttributeType.EnchanceDamage]; } set { Data[(int)AttributeType.EnchanceDamage] = value; } }
    /// <summary>
    /// 嘲讽值
    /// </summary>
    public int Taunt { get { return Data[(int)AttributeType.Taunt]; } set { Data[(int)AttributeType.Taunt] = value; } }
    /// <summary>
    /// 吸血
    /// </summary>
    public int Hematophagia { get { return Data[(int)AttributeType.Hematophagia]; } set { Data[(int)AttributeType.Hematophagia] = value; } }
    /// <summary>
    /// 距离增伤
    /// </summary>
    public int DistanceDamage { get { return Data[(int)AttributeType.DistanceDamage]; } set { Data[(int)AttributeType.DistanceDamage] = value; } }
    /// <summary>
    /// 反伤
    /// </summary>
    public int AgainstDamage { get { return Data[(int)AttributeType.AgainstDamage]; } set { Data[(int)AttributeType.AgainstDamage] = value; } }


    public void Reset()
    {
        for (int i = 0; i < (int)AttributeType.MAX; i++)
        {
            this.Data[i] = 0;
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
            result.Data[i] = a.Data[i] + b.Data[i];
        }
        return result;
    }
}

