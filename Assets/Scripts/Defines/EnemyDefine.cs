using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyAIType
{
    TankAI = 0,
    WarriorAI = 1,
    MagicianAI = 2,
    AssassinAI = 3,
    PastorAI = 4,
}

public class EnemyEquip
{
    public int id { get; set; }
    public Vector2Int postion { get; set; }
    public int rotation { get; set; }
}

public class EnemyDefine
{
    public int ID { get; set; }
    public int CID { get; set; }
    public EnemyAIType aiType { get; set; }
    public EnemyEquip equip1 { get; set; }
    public EnemyEquip equip2 { get; set; }
    public EnemyEquip equip3 { get; set; }
    public EnemyEquip equip4 { get; set; }
    public EnemyEquip equip5 { get; set; }
}
