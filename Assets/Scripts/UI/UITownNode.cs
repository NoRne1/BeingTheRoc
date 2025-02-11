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

public class TownModel
{
    public TownNodeType type;
    public NameData nameData;
    public string Name 
    { 
        get 
        { 
            return Config.Language == 0 ? nameData.chineseName: nameData.englishName; 
        } 
    }
    public TownNodeStatus status = TownNodeStatus.unpassed;

    public List<int> townActions = new List<int>();
    public TownShopInfoModel shopInfo;

    public TownBattleInfoModel battleInfo;
}

public class UITownNode : MonoBehaviour
{
    public TownNodeType type;
    public int townID;
    public Image town_icon;
    public List<Sprite> town_icon_list;
    public TownModel model = new TownModel();
    public TownNodeStatus Status
    {
        get { return model.status; }
        set
        {
            model.status = value;
            town_icon.overrideSprite = town_icon_list[(int)model.status];
        }
    }
    
    void Awake()
    {
        town_icon.overrideSprite = town_icon_list[(int)model.status];
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoNextTown()
    {
        MapManager.Instance.GoNextTown(townID);
        UIManager.Instance.Close<UITownHint>(false);
    }

    private void Init()
    {
        model.type = type;
        model.nameData = DataManager.Instance.townNameGenerator.GetRandomTownName();
        model.townActions.Clear();
        // HashSet<int> hashSet = new HashSet<int>();
        // while (hashSet.Count < 3)
        // {
        //     hashSet.Add(Random.Range(0, DataManager.Instance.TownActions.Count));
        // }
        // model.townActions = hashSet.ToList();
        model.townActions = GameUtil.Instance.GenerateUniqueRandomList(0, DataManager.Instance.TownActions.Count, 3);

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
        model.battleInfo = new TownBattleInfoModel(type, temp);
    }
}
