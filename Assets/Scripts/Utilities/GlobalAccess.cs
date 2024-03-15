using System;
using UnityEngine;
using System.Linq;

class GlobalAccess
{
    public static int CurrentCharacterId
    {
        get
        {
            return PlayerPrefs.GetInt("CurrentCharacterId", -1);
        }
        set
        {
            if (DataManager.Instance.Characters.Keys.ToList().Contains(value))
            {
                PlayerPrefs.SetInt("CurrentCharacterId", value);
            }
        }
    }

    ~GlobalAccess()
    {
        PlayerPrefs.Save();
    }
}

