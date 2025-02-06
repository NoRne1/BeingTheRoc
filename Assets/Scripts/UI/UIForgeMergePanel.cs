using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIForgeMergePanel : MonoBehaviour
{
    public LineConnector lineConnector;
    public UIBagView bagView;
    public UIRepositorSlot equipSlotLeft;
    public UIRepositorSlot equipSlotRight;
    public UIRepositorSlot equipSlotResult;
    public Button mergeButton;
    public Button collectButton;
    public TextMeshProUGUI collectButtonText;
    private StoreItemModel mergeItemLeft;
    private StoreItemModel mergeItemRight;

    public GameObject merge_start_panel;
    public GameObject merge_ing_panel;

    public Transform equipMoveFather;
    public GameObject itemPrefab; // 装备移动的预制体
    //这里用UIEquipItem只是复用，只是用它存储storeItemModel
    private UIEquipItem draggedEquipItem; // 装备移动的实例
    private List<UIEquipItem> equipItems = new List<UIEquipItem>();
    private UIRepositorSlot currentItem; // 仓库中当前被拖动的装备脚本
    private StoreItemModel useOrDropItem;
    
    private Vector3 pressPosition; // 鼠标按下的位置
    private BehaviorSubject<bool> isDraggingSubject = new BehaviorSubject<bool>(false); // 是否正在拖动装备

    private float moveThreshold = 10f; //移动检测阈值

    public GameObject repository;
    private IDisposable collectButtonTextDisposable;

    // Start is called before the first frame update
    void Start()
    {
        ResetMergeSlot();
        mergeButton.OnClickAsObservable().Subscribe(_ => {
            if (mergeItemLeft == null || mergeItemRight == null) {
                BlackBarManager.Instance.AddMessage("装备没放呢");
                return;
            }
            var resultID = DataManager.Instance.GetMergedEquip(mergeItemLeft.ID, mergeItemRight.ID);
            if (resultID == -1) 
            {
                BlackBarManager.Instance.AddMessage("这俩装备相性不合");
                return;
            }
            if (GlobalAccess.forgeMergePrice > GameManager.Instance.featherCoin.Value)
            {
                //钱不够买
                UITip tip = UIManager.Instance.Show<UITip>();
                //todo
                tip.UpdateTip(GameUtil.Instance.GetDisplayString("钱不够啦"));
            }
            GameManager.Instance.FeatherCoinChanged(-GlobalAccess.forgeMergePrice);
            GameManager.Instance.otherProperty.currentMergeTaskInfo.OnNext(new MergeEquipInfo(mergeItemLeft.ID, mergeItemRight.ID, resultID));   
            GameManager.Instance.otherProperty.mergeTaskTimer.OnNext(3);
            ResetMergeSlot();
        }).AddTo(this);
        GameManager.Instance.otherProperty.currentMergeTaskInfo.DistinctUntilChanged().AsObservable()
            .CombineLatest(GameManager.Instance.otherProperty.mergeTaskTimer.DistinctUntilChanged().AsObservable(), (info, timer)=>(info, timer))
            .Subscribe(para => {
            if (para.info == null)
            {
                merge_start_panel.SetActive(true);
                merge_ing_panel.SetActive(false);
            } else if (para.info != null && para.timer == 0)
            {
                //合成任务到期了
                merge_start_panel.SetActive(false);
                merge_ing_panel.SetActive(true);
                collectButtonTextDisposable.IfNotNull(dispose => { dispose.Dispose(); });
                collectButtonText.text = GameUtil.Instance.GetDisplayString("merge_done");
                collectButton.enabled = true;
            } else if (para.info != null && para.timer > 0)
            {
                //当前有合成任务，也没到期
                merge_start_panel.SetActive(false);
                merge_ing_panel.SetActive(true);
                string[] displayStrings = { GameUtil.Instance.GetDisplayString("merging"), GameUtil.Instance.GetDisplayString("merging") + ".", 
                    GameUtil.Instance.GetDisplayString("merging") + "..", GameUtil.Instance.GetDisplayString("merging") + "..." };
                collectButtonTextDisposable.IfNotNull(dispose => { dispose.Dispose(); });
                // 使用 Observable.Interval 创建一个每隔一段时间发射一次的可观察序列
                collectButtonTextDisposable = Observable.Interval(System.TimeSpan.FromSeconds(1f))
                    .Select(index => displayStrings[index % displayStrings.Length]) // 根据当前索引选择字符串
                    .Subscribe(text => collectButtonText.text = GameUtil.Instance.GetDisplayString(text)) // 更新文本
                    .AddTo(this); // 确保在对象销毁时取消订阅
                collectButton.enabled = false;
            }
        }).AddTo(this);

        collectButton.OnClickAsObservable().Subscribe(_=>
        {
            if (GameManager.Instance.otherProperty.currentMergeTaskInfo.Value != null && GameManager.Instance.otherProperty.mergeTaskTimer.Value == 0)
            {
                var equipResultModel = new StoreItemModel(DataManager.Instance.StoreItems[GameManager.Instance.otherProperty.currentMergeTaskInfo.Value.equipResult]);
                merge_ing_panel.GetComponent<EquipmentFusionAnimation>().StartFusionCompleteAnimation(()=>{
                    GameManager.Instance.repository.AddItem(equipResultModel);
                    ResetMergeSlot();
                    GameManager.Instance.otherProperty.currentMergeTaskInfo.OnNext(null);
                    GameManager.Instance.otherProperty.mergeTaskTimer.OnNext(-1);
                });
            } else {
                Debug.LogError("UIForgeMergePanel collectButton click unexpected");
            }
        }).AddTo(this);

        isDraggingSubject.DistinctUntilChanged().Subscribe(flag=>{
            if (flag){
                if (draggedEquipItem != null) { CanMergeItemsLineup(draggedEquipItem); }
                else {Debug.LogWarning("UIForgeMergePanel isDragging but no draggedEquipItem");}
            } else {
                lineConnector.StopAllConnections();
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
                isDraggingSubject.OnNext(false);
            }
        }

        if (Input.GetMouseButton(0) && (currentItem != null))
        {
            // 如果鼠标按下后移动了一定距离，则执行装备拖动逻辑
            if (!isDraggingSubject.Value && (Input.mousePosition - pressPosition).magnitude > moveThreshold)
            {
                if (currentItem != null && currentItem.item.CanEquipEnhance())
                {
                    var draggedItem = Instantiate(itemPrefab, this.equipMoveFather);
                    draggedEquipItem = draggedItem.GetComponent<UIEquipItem>();
                    draggedEquipItem.storeItem = currentItem.item;
                    draggedItem.GetComponent<RectTransform>().sizeDelta = currentItem.CompareTag("Item") ? new Vector2(150, 150) : new Vector2(200, 200);
                    draggedItem.GetComponent<Image>().overrideSprite = Resloader.LoadSprite(currentItem.item.iconResource, ConstValue.equipsPath);
                }
                //需要先给draggedEquipItem赋值，然后再isDraggingSubject
                isDraggingSubject.OnNext(true);
            }
            if (isDraggingSubject.Value && draggedEquipItem != null)
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
                if (isDraggingSubject.Value &&  (Input.mousePosition - pressPosition).magnitude > moveThreshold)
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
                            if (tempItem == equipSlotLeft)
                            {
                                if (mergeItemLeft != null) { GameManager.Instance.repository.AddItem(mergeItemLeft); }
                                SetupLeftItem(currentItem.item);
                            } else if (tempItem == equipSlotRight)
                            {
                                if (mergeItemRight != null) { GameManager.Instance.repository.AddItem(mergeItemRight); }
                                SetupRightItem(currentItem.item);
                            }
                            GameManager.Instance.repository.RemoveItem(currentItem.item.uuid);
                        } else if (currentItem.CompareTag("ForgeItem") && tempItem.CompareTag("Item"))
                        {
                            //从enhanceSlot移动到仓库,有先后顺序
                            GameManager.Instance.repository.AddItem(currentItem.item);
                            if (currentItem == equipSlotLeft) {
                                SetupLeftItem(null);
                            } else if (currentItem == equipSlotRight) {
                                SetupRightItem(null);
                            }
                        }
                    } else if (currentItem.CompareTag("ForgeItem") && GameUtil.Instance.IsPointInsideGameObject(repository, Input.mousePosition))
                    {
                        //从enhanceSlot移动到仓库,有先后顺序
                        GameManager.Instance.repository.AddItem(currentItem.item);
                        if (currentItem == equipSlotLeft) {
                            SetupLeftItem(null);
                        } else if (currentItem == equipSlotRight) {
                            SetupRightItem(null);
                        }
                    }
                    Destroy(draggedEquipItem.gameObject);
                }
                currentItem = null;
                draggedEquipItem = null;
                isDraggingSubject.OnNext(false);
            }
        }
    }

    public void ResetMergeSlot()
    {
        SetupLeftItem(null);
        SetupRightItem(null);
        SetupMergedItem();
    }

    public void SetupLeftItem(StoreItemModel leftItem)
    {
        this.mergeItemLeft = leftItem;
        equipSlotLeft.Setup(leftItem);
        SetupMergedItem();
    }

    public void SetupRightItem(StoreItemModel rightItem)
    {
        this.mergeItemRight = rightItem;
        equipSlotRight.Setup(rightItem);
        SetupMergedItem();
    }

    public void SetupMergedItem()
    {
        if (mergeItemLeft != null && mergeItemRight != null)
        {
            equipSlotResult.Setup(DataManager.Instance.GetMergedEquip(mergeItemLeft.ID, mergeItemRight.ID));
        } else {
            equipSlotResult.Setup(null);
        }
        
    }

    public void CanMergeItemsLineup(UIEquipItem itemA)
    {
        foreach(var item in GameManager.Instance.repository.itemsRelay.Value)
        {
            if (DataManager.Instance.GetCanMergeEquips(itemA.storeItem.ID).Keys.Contains(item.ID))
            {
                //找到了能合成的装备
                //获取该装备在仓库中的位置
                var slot = bagView.GetItemLocatedSlot(item);
                //添加连接
                slot.IfNotNull(_=>{lineConnector.AddConnection(itemA.transform, slot.transform);});
            }
        }
    }
}
