using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UniRx;


public class MapManager : MonoSingleton<MapManager>, IPointerMoveHandler
{
    public Transform town_node_father;
    public Transform map_line_father;
    public GameObject town1Perfab;
    public GameObject town2Perfab;
    public GameObject town3Perfab;
    public GameObject kingPerfab;
    public GameObject mapLinePerfab;

    public List<Transform> node_circle_3 = new List<Transform>();
    public List<Transform> node_circle_2 = new List<Transform>();
    public List<Transform> node_circle_1 = new List<Transform>();
    public List<Sprite> map_line_list = new List<Sprite>();

    public List<UITownNode> townList = new List<UITownNode>();
    public List<(int, int)> MapLineLink = new List<(int, int)>();

    private int townIdcounter = 1;

    private int currentTownId = -1;
    private BehaviorSubject<int> nextTownIdSubject = new BehaviorSubject<int>(-1);
    // Start is called before the first frame update
    void Start()
    {
        //townIdcounter = 1;
        //currentTownId = null;

        //for test
        DataManager.Instance.Load();

        nextTownIdSubject.AsObservable().Subscribe(id =>
        {
            if(id != -1)
            {
                if(currentTownId == -1)
                {
                    //init时
                    townList[id].UpdatePlayerIsThere(true);
                    currentTownId = id;
                } else if(CanGoNextTown(currentTownId, id))
                {
                    townList[currentTownId].UpdatePlayerIsThere(false);
                    townList[id].UpdatePlayerIsThere(true);
                    currentTownId = id;
                } else
                {
                    UITip tip = UIManager.Instance.Show<UITip>();
                    tip.UpdateTip(DataManager.Instance.Language["go_next_town_tip"]);
                }
            }
        });

        generateMap();
        player_init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void generateMap()
    {
        GameObject kingNode = Instantiate(kingPerfab, this.town_node_father);
        UITownNode uIKingNode = kingNode.GetComponent<UITownNode>();
        uIKingNode.character_icon.overrideSprite = GlobalAccess.CurrentCharacterIcon;
        uIKingNode.townID = 0;
        townList.Add(kingNode.GetComponent<UITownNode>());
        generateTownCircle(node_circle_3, town3Perfab, 4);
        generateTownCircle(node_circle_2, town2Perfab, 7);
        generateTownCircle(node_circle_1, town1Perfab, 10);
        generateMapLine();
    }

    private void generateTownCircle(List<Transform> node_circle, GameObject townPerfab, int town_num)
    {
        int initIndex = Random.Range(0, node_circle.Count);
        int increase = 0;
        for (int i = 0; i < town_num; i++)
        {
            GameObject townNode = Instantiate(townPerfab, node_circle[initIndex].position, Quaternion.identity, this.town_node_father);
            UITownNode uITownNode = townNode.GetComponent<UITownNode>();
            uITownNode.townID = townIdcounter;
            townIdcounter++;
            uITownNode.character_icon.overrideSprite = GlobalAccess.CurrentCharacterIcon;
            townList.Add(uITownNode);
            if (increase <= (node_circle.Count / (float)town_num) * i)
            {
                int randomInt = (int)Random.Range(2.5f, 4f);
                initIndex = (initIndex + randomInt) % node_circle.Count;
                increase += randomInt;
            }
            else
            {
                initIndex = (initIndex + 2) % node_circle.Count;
                increase += 2;
            }
        }
    }

    private void generateMapLine()
    {
        for (int i = 0; i < townList.Count; i++)
        {
            for (int j = i + 1; j < townList.Count; j++)
            {
                Vector3 townNode1Pos = Camera.main.WorldToScreenPoint(townList[i].gameObject.transform.position);
                Vector3 townNode2Pos = Camera.main.WorldToScreenPoint(townList[j].gameObject.transform.position);
                Vector3 middlePos = (townNode1Pos + townNode2Pos) / 2.0f;
                Vector3 dirVector3 = townNode1Pos - townNode2Pos;
                float distance = Vector3.Magnitude(dirVector3);
                //连线不能跨越两个级别
                bool flag = Mathf.Abs(townList[i].type - townList[j].type) <= 1;
                if (distance < 400 & flag)
                {
                    int map_line_index = Random.Range(0, map_line_list.Count);
                    GameObject mapLine = Instantiate(mapLinePerfab, Camera.main.ScreenToWorldPoint(middlePos), Quaternion.identity, this.map_line_father);
                    mapLine.GetComponent<Image>().overrideSprite = map_line_list[map_line_index];
                    mapLine.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(distance - 120, 20);
                    if (dirVector3.y > 0)
                    {
                        mapLine.gameObject.transform.Rotate(new Vector3(0, 0, Vector3.Angle(new Vector3(1, 0, 0), dirVector3)));
                    } else
                    {
                        mapLine.gameObject.transform.Rotate(new Vector3(0, 0, -Vector3.Angle(new Vector3(1, 0, 0), dirVector3)));
                    }
                    MapLineLink.Add((i, j));
                }
            }
        }
    }

    private void player_init()
    {
        int temp_id = townList.FindIndex(town => town.type == TownNodeType.town1);
        nextTownIdSubject.OnNext(temp_id);
    }

    public void GoNextTown(int desTownID)
    {
        nextTownIdSubject.OnNext(desTownID);
    }

    public bool CanGoNextTown(int sourceTownID, int desTownID)
    {
        return MapLineLink.Contains((sourceTownID, desTownID)) || MapLineLink.Contains((desTownID, sourceTownID));
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
