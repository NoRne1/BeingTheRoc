using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UniRx;
using System.Linq;
using Unity.VisualScripting;


public class MapManager : MonoSingleton<MapManager>
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

    private List<UITownNode> townList = new List<UITownNode>();
    public UITownNode CurrentTownNode
    {
        get { return townList[currentTownId]; }
    }

    //计算最短路径的变量
    private int V; // 图中顶点的数量
    private List<int>[] MapLineLink; // 邻接表表示图

    private int townIdcounter = 1;

    public int CurrentTownId
    {
        get { return currentTownId; }
    }
    private int currentTownId = -1;
    private BehaviorSubject<int> nextTownIdSubject = new BehaviorSubject<int>(-1);
    // Start is called before the first frame update
    void Start()
    {
        //for test
        DataManager.Instance.Load();

        nextTownIdSubject.AsObservable().TakeUntilDestroy(this).Subscribe(id =>
        {
            if (id != -1)
            {
                if (currentTownId == -1)
                {
                    //init时
                    townList[id].UpdatePlayerIsThere(true);
                    currentTownId = id;
                    townList[currentTownId].Status = TownNodeStatus.passed;
                } else if (CanGoNextTown(currentTownId, id))
                {
                    //townList[currentTownId].UpdatePlayerIsThere(false);
                    //townList[id].UpdatePlayerIsThere(true);
                    //currentTownId = id;
                    //townList[currentTownId].Status = TownNodeStatus.passed;
                    GameManager.Instance.SwitchPage(PageType.battle, ()=>
                    {
                        BattleManager.Instance.StartBattle(GameManager.Instance.characterIDs, townList[currentTownId].battleInfo);
                    });
                } else
                {
                    UITip tip = UIManager.Instance.Show<UITip>();
                    tip.UpdateTip(DataManager.Instance.Language["go_next_town_tip"]);
                }
            }
        });

        generateMap();
        InitVarible();
        generateMapLine();
        player_init();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void InitVarible()
    {
        V = townList.Count;
        MapLineLink = new List<int>[V];
        for (int i = 0; i < V; i++)
        {
            MapLineLink[i] = new List<int>();
        }
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
                    MapLineLink[i].Add(j);
                    MapLineLink[j].Add(i);
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

    public bool CanGoNextTown(int s, int d)
    {
        return ShortestPath(s, d).Count != 0;
    }

    // 使用 BFS 寻找从顶点 s 到顶点 d 的最短路径
    public List<int> ShortestPath(int s, int d)
    {
        // 记录路径
        int[] predecessor = new int[V];
        bool[] visited = new bool[V];
        Queue<int> queue = new Queue<int>();

        // 初始化数组
        for (int i = 0; i < V; ++i)
        {
            predecessor[i] = -1;
            visited[i] = false;
        }

        // 将起始顶点标记为访问过并加入队列
        visited[s] = true;
        queue.Enqueue(s);

        // BFS
        while (queue.Count != 0)
        {
            int current = queue.Dequeue();

            // 遍历当前顶点的邻居
            foreach (int neighbor in MapLineLink[current])
            {
                if (!visited[neighbor] && (neighbor == d || townList[neighbor].Status == TownNodeStatus.passed))
                {
                    visited[neighbor] = true;
                    predecessor[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        // 构建路径
        List<int> path = new List<int>();
        int crawl = d;
        path.Add(crawl);
        while (predecessor[crawl] != -1)
        {
            path.Add(predecessor[crawl]);
            crawl = predecessor[crawl];
        }

        // 如果不存在路径，则返回空列表
        if (path[path.Count - 1] != s)
        {
            return new List<int>();
        }

        // 反转路径
        path.Reverse();
        return path;
    }
}
