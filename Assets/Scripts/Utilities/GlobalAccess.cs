using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

public enum GeneralLevel
{
    green = 0,
    blue = 1,
    red = 2
}

class GlobalAccess
{
    public static int levelUpExp = 100;
    public static int maxLevel = 3;
    public static int teamOpacity = 6;
    public static int mainCharacterNum = 3;
    public static int subCharacterStartIndex = 100;
    public static float roundDistance = 10000.0f;
    public static float totalDayTime = 100.0f;
    public static int moveBarMaxShowNum = 7;
    public static string friendColorHex = "#9bf6ff";
    public static string enermyColorHex = "#ffadad";

    public static float equipSizeBagMultiply = 180;
    public static float equipSizeBattleMultiply = 150;
    public static float timeLeftAnimTime = 0.5f;
    public static float switchPageTime = 0.85f;

    public static int knockbackDirectDamage = 20;

    public static int subCharacterNum
    {
        get { return DataManager.Instance.Characters.Count - mainCharacterNum; }
    }
    //public static int CurrentCharacterId = 1;
    public static int CurrentCharacterId
    {
        get
        {
            return 0;
            // return PlayerPrefs.GetInt("CurrentCharacterId", -1);
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
            return Resloader.LoadSprite(DataManager.Instance.Characters[CurrentCharacterId].Resource, ConstValue.battleItemsPath);
        }
    }

    public static Color GetLevelColor(GeneralLevel level)
    {
        switch(level)
        {
            case GeneralLevel.green:
                return GameUtil.Instance.hexToColor("98CF76");
            case GeneralLevel.blue:
                return GameUtil.Instance.hexToColor("67CDFF");
            case GeneralLevel.red:
                return GameUtil.Instance.hexToColor("E36157");
            default:
                return Color.white;
        }
    }

    public static int GetTauntForJob(JobType job)
    {
        switch(job)
        {
            case JobType.Warrior:
                return 40;
            case JobType.Magician:
                return 20;
            case JobType.Tank:
                return 60;
            default:
                Debug.LogError("GetWeightForJob unknown JobType: " + job);
                return 0;
        }
    }

    public static BattleItem GetBattleItem(string uuid)
    {
        return NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(uuid)).Value;
    }

    public static void SaveBattleItem(BattleItem battleItem)
    {
        NorneStore.Instance.Update<BattleItem>(battleItem, true);
    }

    ~GlobalAccess()
    {
        PlayerPrefs.Save();
    }
}

