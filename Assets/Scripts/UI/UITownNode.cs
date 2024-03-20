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
    normal = 0,
    unpassed = 1,
    passed = 2
}

public class UITownNode : MonoBehaviour
{
    public TownNodeType type;
    public int townID;
    public Image town_icon;
    public Image character_icon;

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

    public void UpdatePlayerIsThere(bool isThere)
    {
        if (isThere)
        {
            character_icon.gameObject.SetActive(true);
        } else
        {
            character_icon.gameObject.SetActive(false);
        }
    }

    public void GoNextTown()
    {
        MapManager.Instance.GoNextTown(townID);
    }

    private void Init()
    {
        townActions.Clear();
        HashSet<int> hashSet = new HashSet<int>();
        while (hashSet.Count < 3)
        {
            hashSet.Add(Random.Range(0,5));
        }
        townActions = hashSet.ToList();
    }
}
