using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;

public class UIMainMenu : MonoBehaviour
{
    public TextMeshProUGUI startGame;
    public TextMeshProUGUI options;
    public TextMeshProUGUI exit;
    public Transform cloudFather;

    private Dictionary<int, Sprite> cloudSpriteDic = new Dictionary<int, Sprite>();
    private List<int> selectedCloudIDs = new List<int>();
    private Subject<int> subject = new Subject<int>();
    
    // 假设你有一个 cloudPrefab 来实例化云对象
    public GameObject cloudPrefab;
    // Start is called before the first frame update
    void Start()
    {
        DataManager.Instance.DataLoaded.AsObservable().TakeUntilDestroy(this).Subscribe(loaded =>
        {
            if (loaded)
            {
                this.Init();
            }
        });
        int i =0;
        foreach (var sprite in Resloader.LoadAllSprite(ConstValue.cloudsPath))
        {
            cloudSpriteDic.Add(i++, sprite);
        }

        subject.AsObservable().Subscribe(id =>
        {
            selectedCloudIDs.Remove(id);
        }).AddTo(this);
        InvokeRepeating("AddCloud", 0f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {
        startGame.text = GameUtil.Instance.GetDisplayString("start_game");
        options.text = GameUtil.Instance.GetDisplayString("options");
        exit.text = GameUtil.Instance.GetDisplayString("exit");
    }

    

    public bool AddCloud()
    {
        // 找到所有没有被选择过的 cloudSpriteID
        var unselectedCloudIDs = cloudSpriteDic.Keys.Where(id => !selectedCloudIDs.Contains(id)).ToList();

        if (unselectedCloudIDs.Count == 0)
        {
            Debug.LogWarning("所有云已经被选择过了！");
            return false; // 如果没有未选择的云，返回
        }

        // 随机选一个 ID
        int randomID = unselectedCloudIDs[Random.Range(0, unselectedCloudIDs.Count)];

        // 获取对应的 Sprite
        Sprite randomSprite = cloudSpriteDic[randomID];

        // 实例化 cloudObject，并设置在屏幕最左边的随机位置
        Vector3 randomPosition = Camera.main.ScreenToWorldPoint(new Vector3(0f, Random.Range(0f, 1080f))); // 假设左边位置为 -30f，Y 坐标随机
        randomPosition.z = cloudFather.position.z;
        GameObject cloudObject = Instantiate(cloudPrefab, randomPosition, Quaternion.identity, cloudFather);

        // 调用 Setup 方法，设置 cloudObject
        cloudObject.GetComponent<UICloud>().Setup(randomID, randomSprite, subject);

        // 将该 Cloud 的 ID 添加到已选择的列表中
        selectedCloudIDs.Add(randomID);
        return true;
    }
}
