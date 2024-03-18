using System;
using UnityEngine;
using System.Linq;

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
            return Resloader.Load<Sprite>("Sprite/" + DataManager.Instance.Characters[CurrentCharacterId].Resource);
        }
    }

    ~GlobalAccess()
    {
        PlayerPrefs.Save();
    }
}

