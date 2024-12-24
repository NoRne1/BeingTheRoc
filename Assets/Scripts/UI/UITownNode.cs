using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum TownNodeType
{
    king = 0,
    town3 = 1,
    town2 = 2,
    town1 = 3
}

public enum TownNodeStatus
{
    unpassed = 0,
    passed = 1
}

public class UITownNode : MonoBehaviour
{
    public TownNodeType type;
    public int townID;
    public Image town_icon;

    public List<Sprite> town_icon_list;
    
    private TownNodeStatus status = TownNodeStatus.unpassed;
    public TownNodeStatus Status
    {
        get { return status; }
        set
        {
            status = value;
            town_icon.overrideSprite = town_icon_list[(int)status];
        }
    }

    public List<int> townActions = new List<int>();
    public TownShopInfoModel shopInfo;

    public TownBattleInfoModel battleInfo;

    // Start is called before the first frame update
    void Start()
    {
        town_icon.overrideSprite = town_icon_list[(int)status];
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoNextTown()
    {
        MapManager.Instance.GoNextTown(townID);
    }

    private void Init()
    {
        townActions.Clear();
        HashSet<int> hashSet = new HashSet<int>();

        //todo add a shop for test
        hashSet.Add(2);
        hashSet.Add(5);
        hashSet.Add(0);
        while (hashSet.Count < 3)
        {
            hashSet.Add(Random.Range(0,5));
        }
        townActions = hashSet.ToList();
        //townActions = GameUtil.Instance.GenerateUniqueRandomList(0, DataManager.Instance.TownActions.Count, 3);

        //todo TownBattleInfoModel init
        List<Vector2> temp = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(0, 2),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(1, 2)
        };
        battleInfo = new TownBattleInfoModel(type, temp);
    }
}
