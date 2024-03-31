using System;
using UnityEngine;
using System.Linq;

public enum EquipLevel
{
    green = 0,
    blue = 1,
    red = 2
}

class GlobalAccess
{
    public static int CurrentCharacterId
    {
        get
        {
            //return PlayerPrefs.GetInt("CurrentCharacterId", -1);
            return 1;
        }
        set
        {
            if (DataManager.Instance.Characters.Keys.ToList().Contains(value))
            {
                PlayerPrefs.SetInt("CurrentCharacterId", value);
            }
        }
    }

    public static Sprite CurrentCharacterIcon
    {
        get
        {
            return Resloader.Load<Sprite>(ConstValue.spritePath + DataManager.Instance.Characters[CurrentCharacterId].Resource);
        }
    }

    public static Color GetLevelColor(EquipLevel level)
    {
        switch(level)
        {
            case EquipLevel.green:
                return GameUtil.Instance.hexToColor("98CF76");
            case EquipLevel.blue:
                return GameUtil.Instance.hexToColor("67CDFF");
            case EquipLevel.red:
                return GameUtil.Instance.hexToColor("E36157");
            default:
                return Color.white;
        }
    }

    ~GlobalAccess()
    {
        PlayerPrefs.Save();
    }
}

