using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    public Image title;
    public Image charIcon;
    public TextMeshProUGUI startGame;
    public TextMeshProUGUI options;
    public TextMeshProUGUI exit;
    public Transform cloudFather;

    private Dictionary<int, Sprite> cloudSpriteDic = new Dictionary<int, Sprite>();
    private List<int> selectedCloudIDs = new List<int>();
    private Coroutine rotateTitleCoroutine;
    private Coroutine addCloudCoroutine;
    private ObjectPool cloudPool; // 对象池

    // 假设你有一个 cloudPrefab 来实例化云对象
    public GameObject cloudPrefab;

    const float minDistance = 5.0f; // 最小间距（世界单位）
    const float timeInterval = 1.5f;

    public FiveElementsPropertyDisplay propertyDisplay;

    void Awake()
    {
        DataManager.Instance.DataLoaded.AsObservable().TakeUntilDestroy(this).Subscribe(loaded =>
        {
            if (loaded)
            {
                this.Init();
            }
        });

        int i = 0;
        foreach (var sprite in Resloader.LoadAllSprite(ConstValue.cloudsPath))
        {
            cloudSpriteDic.Add(i++, sprite);
        }

        // 初始化对象池
        cloudPool = new ObjectPool(cloudPrefab, 10, cloudFather); // 初始池大小为 10
    }

    public void Init()
    {
        startGame.text = GameUtil.Instance.GetDisplayString("start_game");
        options.text = GameUtil.Instance.GetDisplayString("options");
        exit.text = GameUtil.Instance.GetDisplayString("exit");
        FiveElements data = new FiveElements(8, 5, 2, 4, 7);
        propertyDisplay.UpdateElements(data);
    }

    private void OnEnable()
    {
        // 一次性生成 5～8 个初始云朵
        SpawnInitialClouds();

        addCloudCoroutine = StartCoroutine(RandomRotateTitleCoroutine());

        // 启动协程，继续随机生成云朵
        addCloudCoroutine = StartCoroutine(RandomAddCloudCoroutine());
    }

    private void OnDisable()
    {
        if (rotateTitleCoroutine != null)
        {
            StopCoroutine(rotateTitleCoroutine);
        }
        if (addCloudCoroutine != null)
        {
            StopCoroutine(addCloudCoroutine);
        }
        cloudPool.ReturnAllObject();
        selectedCloudIDs.Clear();
    }

    // 随机旋转标题的协程
    private IEnumerator RandomRotateTitleCoroutine()
    {
        while (true) // 无限循环
        {
            // 调用 AddCloud 方法
            Rotate(title.gameObject);
            Rotate(charIcon.gameObject);       
            // 随机等待 1.5～2.5 秒
            // float randomDelay = Random.Range(1.5f, 2.5f);
            yield return new WaitForSeconds(1.5f);
        }
    }

    // 随机时间间隔生成云朵的协程
    private IEnumerator RandomAddCloudCoroutine()
    {
        while (true) // 无限循环
        {
            // 调用 AddCloud 方法
            AddCloud();

            // 随机等待 0.5～1.5 秒
            float randomDelay = Random.Range(1.5f, 2.5f);
            yield return new WaitForSeconds(randomDelay);
        }
    }

    public bool AddCloud()
    {
        var unselectedCloudIDs = cloudSpriteDic.Keys.Where(id => !selectedCloudIDs.Contains(id)).ToList();
        if (unselectedCloudIDs.Count == 0)
        {
            Debug.LogWarning("All clouds have been selected!");
            return false;
        }

        int randomID = unselectedCloudIDs[Random.Range(0, unselectedCloudIDs.Count)];
        Sprite randomSprite = cloudSpriteDic[randomID];

        // 获取屏幕边界的世界坐标
        Vector3 screenBottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        Vector3 screenTopRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
        float minY = screenBottomLeft.y;
        float maxY = screenTopRight.y;

        bool positionFound = false;
        Vector3 randomPosition = Vector3.zero;
        int attempts = 0;
        const int maxAttempts = 10;
        // const float minVerticalDistance = 1.5f; // 最小垂直间距（世界单位）

        do
        {
            // 生成候选Y坐标并计算位置
            float candidateY = Random.Range(minY, maxY);
            float screenLeftX = screenBottomLeft.x;
            randomPosition = new Vector3(screenLeftX, candidateY, cloudFather.position.z);

            // 检查与现有云朵的间距
            bool tooClose = false;
            foreach (Transform child in cloudFather)
            {
                if (child.gameObject.activeSelf && Vector3.Distance(child.position, randomPosition) < minDistance) // 检查整体距离
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                positionFound = true;
                break;
            }
            attempts++;
        } while (attempts < maxAttempts);

        if (!positionFound)
        {
            Debug.Log("Failed to find valid position, skipping cloud spawn.");
            return false;
        }

        // 从对象池中获取云朵对象
        GameObject cloudObject = cloudPool.GetObjectFromPool();
        cloudObject.transform.position = randomPosition;
        cloudObject.transform.rotation = Quaternion.identity;

        // 设置云朵的属性和事件
        UICloud cloudComponent = cloudObject.GetComponent<UICloud>();
        cloudComponent.Setup(randomID, randomSprite);

        // 监听云朵销毁事件，将其返回到对象池
        cloudComponent.cloudDestroySubject.Subscribe(id =>
        {
            selectedCloudIDs.Remove(id);
            cloudPool.ReturnObjectToPool(cloudObject);
        });

        selectedCloudIDs.Add(randomID);
        return true;
    }
    // 一次性生成 7～10 个初始云朵
    private void SpawnInitialClouds()
    {
        int cloudCount = Random.Range(7, 11); // 生成 7～10 个云朵
        for (int i = 0; i < cloudCount; i++)
        {
            SpawnCloudAtRandomPosition();
        }
    }

    // 在随机位置生成一个云朵
    private void SpawnCloudAtRandomPosition()
    {
        var unselectedCloudIDs = cloudSpriteDic.Keys.Where(id => !selectedCloudIDs.Contains(id)).ToList();
        if (unselectedCloudIDs.Count == 0)
        {
            Debug.LogWarning("All clouds have been selected!");
            return;
        }

        int randomID = unselectedCloudIDs[Random.Range(0, unselectedCloudIDs.Count)];
        Sprite randomSprite = cloudSpriteDic[randomID];

        // 获取屏幕边界的世界坐标
        Vector3 screenBottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        Vector3 screenTopRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
        float minX = screenBottomLeft.x;
        float maxX = screenTopRight.x;
        float minY = screenBottomLeft.y;
        float maxY = screenTopRight.y;

        bool positionFound = false;
        Vector3 randomPosition = Vector3.zero;
        int attempts = 0;
        const int maxAttempts = 50;

        do
        {
            // 生成候选位置
            float candidateX = Random.Range(minX, maxX);
            float candidateY = Random.Range(minY, maxY);
            randomPosition = new Vector3(candidateX, candidateY, cloudFather.position.z);

            // 检查与现有云朵的间距
            bool tooClose = false;
            foreach (Transform child in cloudFather)
            {
                if (child.gameObject.activeSelf && Vector3.Distance(child.position, randomPosition) < minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                positionFound = true;
                break;
            }
            attempts++;
        } while (attempts < maxAttempts);

        if (!positionFound)
        {
            Debug.Log("Failed to find valid position, skipping cloud spawn.");
            return;
        }

        // 从对象池中获取云朵对象
        GameObject cloudObject = cloudPool.GetObjectFromPool();
        cloudObject.transform.position = randomPosition;
        cloudObject.transform.rotation = Quaternion.identity;

        // 设置云朵的属性和事件
        UICloud cloudComponent = cloudObject.GetComponent<UICloud>();
        cloudComponent.Setup(randomID, randomSprite);

        // 监听云朵销毁事件，将其返回到对象池
        cloudComponent.cloudDestroySubject.Subscribe(id =>
        {
            selectedCloudIDs.Remove(id);
            cloudPool.ReturnObjectToPool(cloudObject);
        });

        selectedCloudIDs.Add(randomID);
    }

    // 最大重试次数
    public int rotateMaxRetries = 5;

    // 调用此方法来旋转 GameObject
    public void Rotate(GameObject obj)
    {
        float rotateAngle = 2f;
        float rotateAngleLimit = 6f;
        int retries = 0;

        // 获取 GameObject 当前的 Z 轴旋转角度
        float currentRotation = obj.transform.eulerAngles.z;

        // 由于 eulerAngles 是 [0, 360) 范围内的值，转换为 -180 到 180 范围
        if (currentRotation > 180f)
        {
            currentRotation -= 360f;
        }

        while (retries < rotateMaxRetries)
        {
            // 随机旋转角度 -10 到 10 度
            float randomRotation = Random.Range(-rotateAngle, rotateAngle);

            // 计算旋转后的角度
            float newRotation = currentRotation + randomRotation;

            // 检查旋转后的角度是否在 -30 到 30 度之间
            if (newRotation >= -rotateAngleLimit && newRotation <= rotateAngleLimit)
            {
                // 如果在范围内，应用旋转并更新当前角度
                obj.transform.rotation = Quaternion.Euler(0f, 0f, newRotation);
                return; // 成功旋转，退出方法
            }

            // 如果不在范围内，增加重试次数
            retries++;
        }

        // 超过最大重试次数，直接返回
        Debug.LogWarning("Maximum retries reached, rotation not applied.");
    }
}
