using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIForgeMergePanel : MonoBehaviour
{
    public UIRepositorSlot equipSlotLeft;
    public UIRepositorSlot equipSlotRight;
    public UIRepositorSlot equipSlotResult;
    public Button mergeButton;

    private StoreItemModel mergeItemLeft;
    private StoreItemModel mergeItemRight;


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

    // Start is called before the first frame update
    void Start()
    {
        SetupLeftItem(mergeItemLeft);
        SetupRightItem(mergeItemRight);
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
            }
        }
    }

    public void SetupLeftItem(StoreItemModel leftItem)
    {
        this.mergeItemLeft = leftItem;
        equipSlotLeft.Setup(leftItem);
    }

    public void SetupRightItem(StoreItemModel rightItem)
    {
        this.mergeItemRight = rightItem;
        equipSlotRight.Setup(rightItem);
    }
}
