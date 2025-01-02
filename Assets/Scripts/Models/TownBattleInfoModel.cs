using UnityEngine;
using System.Collections;
using UniRx;
using System.Linq;
using System.Collections.Generic;

public class TownBattleInfoModel
{
    public TownNodeType type;
    public float battleBaseDifficulty;
    public List<Vector2> initPlaceSlots;
    public Vector2 granaryPos;
    public Dictionary<Vector2, EnermyModel> enermys = new Dictionary<Vector2, EnermyModel>();
    public List<EnermyModel> supportEnermys;

    public TownBattleInfoModel(TownNodeType type, List<Vector2> initPlaceSlots)
    {
        this.type = type;
        this.initPlaceSlots = initPlaceSlots;
        this.granaryPos = new Vector2(Random.Range(0, 8), 7);
        switch (type)
        {
            case TownNodeType.king:
                battleBaseDifficulty = 7;
                break;
            case TownNodeType.town1:
                battleBaseDifficulty = 1;
                break;
            case TownNodeType.town2:
                battleBaseDifficulty = 2;
                break;
            case TownNodeType.town3:
                battleBaseDifficulty = 4;
                break;
        }

        var models = DataManager.Instance.getEnermyModels(3, 2);
        var enermyModels = models.Where(a=>!a.isSupport).ToList();
        supportEnermys = models.Where(a=>a.isSupport).ToList();
        var postions = getInitPostions(enermyModels.Count);
        if (enermyModels.Count == postions.Count)
        {
            for (int i = 0; i < enermyModels.Count; i++)
            { 
                enermys.Add(postions[i], enermyModels[i]);
            }
        } else
        {
            Debug.LogError("EnermyModels count != postions count!");
        }
    }

    // 当前逻辑只随机找了不与玩家初始位置重叠的点
    private List<Vector2> getInitPostions(int count)
    {
        List<Vector2> result = new List<Vector2>();
        while (result.Count < count)
        {
            Vector2 v = new Vector2(Random.Range(0, 8), Random.Range(0, 8));
            if (!result.Contains(v) && !initPlaceSlots.Contains(v) && v != granaryPos)
            {
                result.Add(v);
            }
        }
        return result;
    }

    public EnermyModel PopSupportEnermy()
    {
        if (supportEnermys.Count > 0)
        {
            var enermy = supportEnermys[0];
            supportEnermys.RemoveAt(0);
            return enermy;
        }
        return null;
    }
}

