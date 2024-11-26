using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIForgeEnhancePanel : MonoBehaviour
{
    public UIRepositorSlot equipSlot;
    public UIStoreItemPanel storeItemPanel;
    public Button enhanceButton;

    private StoreItemModel enhanceItem;


    public Transform equipMoveFather;
    public GameObject itemPrefab; // 装备移动的预制体
    //这里用UIEquipItem只是复用，只是用它存储storeItemModel
    private UIEquipItem draggedEquipItem; // 装备移动的实例
    private List<UIEquipItem> equipItems = new List<UIEquipItem>();
    private UIRepositorSlot currentItem; // 仓库中当前被拖动的装备脚本
    private StoreItemModel useOrDropItem;
    
    private Vector3 pressPosition; // 鼠标按下的位置
    private bool isDragging; // 是否正在拖动装备

    private float moveThreshold = 10f; //移动检测阈值

    public GameObject repository;

    private int enhanceNumToday = 0;

    private Dictionary<GeneralLevel, List<EquipExtraEntryDefine>> levelItems = new Dictionary<GeneralLevel, List<EquipExtraEntryDefine>>();
    // Start is called before the first frame update
    void Awake()
    {
        // 根据定义的等级将词条分类到不同的集合中
        foreach (var define in DataManager.Instance.equipExtraEntryDefines.Values)
        {
            if (!levelItems.ContainsKey(define.level))
            {
                levelItems[define.level] = new List<EquipExtraEntryDefine>();
            }
            levelItems[define.level].Add(define);
        }
    }

    void Start()
    {
        Setup(enhanceItem);
        GameManager.Instance.timeLeft.Select(timeleft=>(int)(timeleft / 3)).DistinctUntilChanged().Subscribe(_=>{
            enhanceNumToday = 0;
        }).AddTo(this);
        enhanceButton.OnClickAsObservable().Subscribe(_ => {
            if (enhanceItem == null) {
                BlackBarManager.Instance.AddMessage("装备没放呢");
            } else if (enhanceNumToday >= GlobalAccess.forgeEnhanceNumLimit)
            {
                //超过每日上限
                UITip tip = UIManager.Instance.Show<UITip>();
                //todo
                tip.UpdateTip(GameUtil.Instance.GetDisplayString("今天铁匠已经累啦，明天再说吧"));
            } else if (GlobalAccess.forgeEnhancePrice > GameManager.Instance.featherCoin.Value)
            {
                //钱不够买
                UITip tip = UIManager.Instance.Show<UITip>();
                //todo
                tip.UpdateTip(GameUtil.Instance.GetDisplayString("钱不够啦"));
            } else {
                enhanceNumToday += 1;
                GameManager.Instance.FeatherCoinChanged(-GlobalAccess.forgeEnhancePrice);
                enhanceItem.equipModel.AddExtraEntrys(GetEntrys(0.05f, 0.15f, 0.25f));
                Setup(enhanceItem);
            }
        }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D? hit = GameUtil.Instance.RaycastAndFindFirstHit(mousePosition, hit =>
                hit.collider != null && (hit.collider.CompareTag("Item") || hit.collider.CompareTag("ForgeItem")) 
                && hit.collider.GetComponent<UIRepositorSlot>().item != null);
            if (hit != null && hit.HasValue)
            {
                currentItem = hit?.collider.GetComponent<UIRepositorSlot>();
                // 记录鼠标按下的位置
                pressPosition = Input.mousePosition;
                isDragging = false;
            }
        }

        if (Input.GetMouseButton(0) && (currentItem != null))
        {
            // 如果鼠标按下后移动了一定距离，则执行装备拖动逻辑
            if (!isDragging && (Input.mousePosition - pressPosition).magnitude > moveThreshold)
            {
                isDragging = true;
                if (currentItem != null && currentItem.item.CanEquipEnhance())
                {
                    var draggedItem = Instantiate(itemPrefab, this.equipMoveFather);
                    draggedEquipItem = draggedItem.GetComponent<UIEquipItem>();
                    draggedEquipItem.storeItem = currentItem.item;
                    draggedItem.GetComponent<RectTransform>().sizeDelta = currentItem.CompareTag("Item") ? new Vector2(150, 150) : new Vector2(200, 200);
                    draggedItem.GetComponent<Image>().overrideSprite = Resloader.LoadSprite(currentItem.item.iconResource, ConstValue.equipsPath);
                }
            }
            if (isDragging && draggedEquipItem != null)
            {
                Vector3 tempVector = Input.mousePosition;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(tempVector);
                //todo worldPosition.z
                worldPosition.z = 90;
                draggedEquipItem.transform.position = worldPosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (currentItem != null && draggedEquipItem != null)
            {
                if (isDragging &&  (Input.mousePosition - pressPosition).magnitude > moveThreshold)
                {
                    // 获取鼠标松开时的位置
                    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D? equipHit = GameUtil.Instance.RaycastAndFindFirstHit(mousePosition, hit =>
                        hit.collider != null && (hit.collider.CompareTag("Item") || hit.collider.CompareTag("ForgeItem")));
                    if (equipHit != null && equipHit.HasValue)
                    {
                        var tempItem = equipHit?.collider.GetComponent<UIRepositorSlot>();
                        if (currentItem.CompareTag("Item") && tempItem.CompareTag("ForgeItem"))
                        {
                            //从仓库移动到enhanceSlot
                            if (enhanceItem != null) { GameManager.Instance.repository.AddItem(enhanceItem); }
                            Setup(currentItem.item);
                            GameManager.Instance.repository.RemoveItem(currentItem.item.uuid);
                        } else if (currentItem.CompareTag("ForgeItem") && tempItem.CompareTag("Item"))
                        {
                            //从enhanceSlot移动到仓库,有先后顺序
                            GameManager.Instance.repository.AddItem(currentItem.item);
                            Setup(null);
                            
                        }
                    } else if (currentItem.CompareTag("ForgeItem") && GameUtil.Instance.IsPointInsideGameObject(repository, Input.mousePosition))
                    {
                        //从enhanceSlot移动到仓库,有先后顺序
                        GameManager.Instance.repository.AddItem(currentItem.item);
                        Setup(null);
                    }
                    Destroy(draggedEquipItem.gameObject);
                }
                currentItem = null;
                draggedEquipItem = null;
            }
        }
    }

    public void Setup(StoreItemModel enhanceItem)
    {
        this.enhanceItem = enhanceItem;
        equipSlot.Setup(enhanceItem);
        storeItemPanel.gameObject.SetActive(enhanceItem != null);
        storeItemPanel.Setup(enhanceItem);
    }
    private HashSet<int> selectedIds = new HashSet<int>();

    // 使用这三个参数来设定概率
    public List<EquipExtraEntryModel> GetEntrys(float levelRate0, float levelRate1, float levelRate2)
    {
        List<EquipExtraEntryModel> result = new List<EquipExtraEntryModel>();

        for (int i = 0; i < 3; i++)
        {
            EquipExtraEntryDefine selectedItem = GetEntry(levelRate0, levelRate1, levelRate2);
            if(selectedItem != null)
            {
                result.Add(new EquipExtraEntryModel(selectedItem));
            }
        }

        return result;
    }

    private EquipExtraEntryDefine GetEntry(float levelRate0, float levelRate1, float levelRate2)
    {
        float randomValue = Random.Range(0f, 1f);
        GeneralLevel selectedLevel = GeneralLevel.none;

        if (randomValue < levelRate0)
        {
            selectedLevel = GeneralLevel.green;
        }
        else if (randomValue < levelRate0 + levelRate1)
        {
            selectedLevel = GeneralLevel.blue;
        }
        else if (randomValue < levelRate0 + levelRate1 + levelRate2)
        {
            selectedLevel = GeneralLevel.red;
        }

        // 如果未返回有效等级，则返回 null
        if (selectedLevel == GeneralLevel.none || !levelItems.ContainsKey(selectedLevel))
        {
            return null; // 如果没有对应等级的词条，返回null
        }

        List<EquipExtraEntryDefine> items = levelItems[selectedLevel];
        EquipExtraEntryDefine selectedItem = null;

        // 在没有选择到有效词条的情况下，随机尝试选择一个
        // 检查当前等级的可选项
        List<EquipExtraEntryDefine> availableItems = items.Where(item => !selectedIds.Contains(item.ID)).ToList();

        if (availableItems.Count == 0)
        {
            return null; // 如果所有可用项都已被选择，返回null
        }

        // 随机选择一个未被选择的词条
        int randomIndex = Random.Range(0, availableItems.Count);
        selectedItem = availableItems[randomIndex];
        selectedIds.Add(selectedItem.ID); // 记录已选择的ID

        return selectedItem; // 返回选中的词条
    }
}
