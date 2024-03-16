using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


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

    public List<UITownNode> townList = new List<UITownNode>();
    public List<(int, int)> MapLineLink = new List<(int, int)>();
    // Start is called before the first frame update
    void Start()
    {
        generateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void generateMap()
    {
        GameObject kingNode = Instantiate(kingPerfab, this.town_node_father);
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
            GameObject town3Node = Instantiate(townPerfab, node_circle[initIndex].position, Quaternion.identity, this.town_node_father);
            townList.Add(town3Node.GetComponent<UITownNode>());
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
                Vector3 townNode1Pos = townList[i].gameObject.transform.position;
                Vector3 townNode2Pos = townList[j].gameObject.transform.position;
                Vector3 middlePos = (townNode1Pos + townNode2Pos) / 2.0f;
                Vector3 dirVector3 = townNode1Pos - townNode2Pos;
                float distance = Vector3.Magnitude(dirVector3);
                //连线不能跨越两个级别
                bool flag = Mathf.Abs(townList[i].type - townList[j].type) <= 1;
                if (distance < 420 & flag)
                {
                    int map_line_index = Random.Range(0, map_line_list.Count);
                    GameObject mapLine = Instantiate(mapLinePerfab, middlePos, Quaternion.identity, this.map_line_father);
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
}
