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
    public Dictionary<Vector2, EnermyModel> enermys = new Dictionary<Vector2, EnermyModel>();

    public TownBattleInfoModel(TownNodeType type, List<Vector2> initPlaceSlots)
    {
        this.type = type;
        this.initPlaceSlots = initPlaceSlots;

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

        var models = DataManager.Instance.getEnermyModels();
        var postions = getInitPostions(models.Count);
        if (models.Count == postions.Count)
        {
            for (int i = 0; i < models.Count; i++)
            {
                enermys.Add(postions[i], models[i]);
            }
        } else
        {
            Debug.LogError("EnermyModels count != postions count!");
        }
    }

    private List<Vector2> getInitPostions(int count)
    {
        List<Vector2> result = new List<Vector2>();
        while (result.Count < count)
        {
            Vector2 v = new Vector2(Random.Range(0, 8), Random.Range(0, 8));
            if (!result.Contains(v) && !initPlaceSlots.Contains(v))
            {
                result.Add(v);
            }
        }
        return result;
    }
}

