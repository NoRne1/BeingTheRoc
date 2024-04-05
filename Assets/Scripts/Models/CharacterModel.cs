using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class CharacterModel: CharacterDefine, IStorable
{
    public int level { get { return exp / GlobalAccess.levelUpExp + 1; } }
    public int remainExp { get { return exp % GlobalAccess.levelUpExp; } }
    public int exp;
    public Backpack backpack;

    public CharacterModel()
    {}

    public CharacterModel(int id)
    {
        ID = id;
    }
    public CharacterModel(CharacterDefine define)
    {
        ID = define.ID;
        Name = define.Name;
        MaxHP = define.MaxHP;
        Strength = define.Strength;
        Defense = define.Defense;
        Dodge = define.Dodge;
        Accuracy = define.Accuracy;
        Speed = define.Speed;
        Mobility = define.Mobility;
        Energy = define.Energy;
        Resource = define.Resource;
        Desc = define.Desc;
    }

    public string StorableCategory => "Character";

    public string Identifier => this.ID.ToString();
}
