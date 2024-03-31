using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class CharacterModel: CharacterDefine, IStorable
{
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
    }

    public string StorableCategory => "Character";

    public string Identifier => this.ID.ToString();
}
