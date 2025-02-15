using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;

public class UITeamBagPage : MonoBehaviour
{
    public Image characterIcon;

    public List<Transform> equipSlots;
    public GameObject itemPrefab; // 大装备的预制体
    private List<UIEquipItem> equipItems = new List<UIEquipItem>();
    private UIEquipItem draggedEquipItem; // 大装备的实例
    private UIRepositorSlot currentItem; // 仓库中当前被拖动的装备脚本
    private StoreItemModel useOrDropItem;
    
    private Vector3 pressPosition; // 鼠标按下的位置
    private bool isDragging; // 是否正在拖动装备

    private float moveThreshold = 10f; //移动检测阈值

    // 丢弃按钮
    public UIEquipButtons equipButtons;

    public CharacterModel character;
    public BattleItem battleItem;

    public Transform equipFather;
    public GameObject repository;
    void Awake()
    {
        // 隐藏丢弃按钮
        equipButtons.gameObject.SetActive(false);

        // 添加丢弃按钮的点击事件
        equipButtons.dropButton.onClick.AddListener(OnDropButtonClick);
        equipButtons.sellButton.onClick.AddListener(OnSellButtonClick);
        equipButtons.useButton.onClick.AddListener(OnUseButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D? hit = GameUtil.Instance.RaycastAndFindFirstHit(mousePosition, hit =>
                hit.collider != null && hit.collider.CompareTag("Item") && hit.collider.GetComponent<UIRepositorSlot>().item != null);
            if (hit != null && hit.HasValue)
            {
                currentItem = hit?.collider.GetComponent<UIRepositorSlot>();
                // 记录鼠标按下的位置
                pressPosition = Input.mousePosition;
                isDragging = false;
            }
            else
            {
                RaycastHit2D? hit2 = GameUtil.Instance.RaycastAndFindFirstHit(mousePosition, hit =>
                    hit.collider != null && hit.collider.CompareTag("EquipSlot"));
                if (hit2 != null && hit2.HasValue)
                {
                    Vector2 vector2 = hit2.Value.collider.GetComponent<UIEquipSlot>().position;
                    //点击的位置是否有装备
                    draggedEquipItem = equipItems.Where(equipTtem =>
                    {
                        bool result = false;
                        for (int i = 0; i < equipTtem.storeItem.equipModel.OccupiedCells.Count; i++)
                        {
                            Vector2 pos = equipTtem.storeItem.equipModel.OccupiedCells[i];
                            if (pos + equipTtem.storeItem.position == vector2)
                            {
                                result = true;
                                break;
                            }
                        }
                        return result;
                    }).FirstOrDefault();
                    pressPosition = Input.mousePosition;
                    isDragging = false;
                }
            }
        }

        if (Input.GetMouseButton(0) && (currentItem != null || draggedEquipItem != null))
        {
            // 如果鼠标按下后移动了一定距离，则执行装备拖动逻辑
            if (!isDragging && (Input.mousePosition - pressPosition).magnitude > moveThreshold)
            {
                isDragging = true;
                // 隐藏丢弃按钮
                equipButtons.gameObject.SetActive(false);
                if (currentItem != null && currentItem.item.CanEquip())
                {
                    var draggedItem = Instantiate(itemPrefab, this.equipFather);
                    draggedEquipItem = draggedItem.GetComponent<UIEquipItem>();
                    draggedEquipItem.storeItem = currentItem.item;
                    draggedEquipItem.ownerID = character.uuid;
                    draggedItem.GetComponent<RectTransform>().sizeDelta = draggedEquipItem.storeItem.equipModel.occupiedRect * GlobalAccess.equipSizeBagMultiply;
                    draggedItem.GetComponent<Image>().overrideSprite = Resloader.LoadSprite(currentItem.item.equipModel.equipResource, ConstValue.equipsPath);
                }
            }
            if (isDragging && draggedEquipItem != null)
            {
                Vector3 tempVector = Input.mousePosition;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(tempVector);
                //todo worldPosition.z
                worldPosition.z = 90;
                draggedEquipItem.transform.position = worldPosition;
                if (Input.GetMouseButtonDown(1))
                {
                    this.Rotate(draggedEquipItem, 90, 0.3f);
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (currentItem != null || draggedEquipItem != null)
            {
                // 如果鼠标按下后未移动或移动距离小于一定距离，则显示丢弃按钮
                if (draggedEquipItem == null && !isDragging && (Input.mousePosition - pressPosition).magnitude <= moveThreshold)
                {
                    useOrDropItem = currentItem.item;
                    Vector3 tempPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(5, -5, 0));
                    equipButtons.gameObject.SetActive(true);
                    equipButtons.sellButton.gameObject.SetActive(GameManager.Instance.currentPageType == PageType.shop);
                    equipButtons.useButton.gameObject.SetActive(useOrDropItem.CanBagUse());
                    equipButtons.transform.position = new Vector3(tempPosition.x, tempPosition.y, 0);
                }
                // 如果鼠标按下后移动了一定距离，则执行装备逻辑
                else if (isDragging && draggedEquipItem != null)
                {
                    // 获取鼠标松开时的位置
                    Vector2 checkPosition = draggedEquipItem.transform.TransformPoint(draggedEquipItem.storeItem.equipModel.originOffset * GlobalAccess.equipSizeBagMultiply);
                    RaycastHit2D? equipHit = GameUtil.Instance.RaycastAndFindFirstHit(checkPosition, hit =>
                        hit.collider != null && hit.collider.CompareTag("EquipSlot"));

                    if (currentItem != null)
                    {
                        if (equipHit != null && equipHit.HasValue)
                        {
                            // 检查是否可以放置装备
                            Vector2Int gridPosition = equipHit.Value.collider.GetComponent<UIEquipSlot>().position;
                            if (ItemUseManager.Instance.Equip(character, draggedEquipItem.storeItem, gridPosition))
                            {
                                Vector3 tempVector = equipHit.Value.collider.GetComponent<UIEquipSlot>().transform.position;
                                draggedEquipItem.SetAndRecord(tempVector);
                            }
                            else
                            {
                                draggedEquipItem.storeItem.ResetRotate();
                                Destroy(draggedEquipItem.gameObject);
                            }
                        } else if (draggedEquipItem != null)
                        {
                            draggedEquipItem.storeItem.ResetRotate();
                            Destroy(draggedEquipItem.gameObject);
                        }
                    }
                    else
                    {
                        //draggedEquipItem != null
                        if (GameUtil.Instance.IsPointInsideGameObject(repository, Input.mousePosition))
                        {
                            //放回到仓库
                            ItemUseManager.Instance.Unequip(character, draggedEquipItem.storeItem);
                            Destroy(draggedEquipItem.gameObject);
                        }
                        else if (equipHit != null)
                        {
                            // 检查是否可以放置装备
                            Vector2Int gridPosition = equipHit.Value.collider.GetComponent<UIEquipSlot>().position;
                            if (character.backpack.MoveTo(draggedEquipItem.storeItem, gridPosition))
                            {
                                Vector3 tempVector = equipHit.Value.collider.GetComponent<UIEquipSlot>().transform.position;
                                draggedEquipItem.SetAndRecord(tempVector);
                            }
                            else
                            {
                                draggedEquipItem.storeItem.ResetRotate();
                                draggedEquipItem.Reset();
                            }
                        } else
                        {
                            draggedEquipItem.storeItem.ResetRotate();
                            draggedEquipItem.Reset();
                        }
                    }
                }
                currentItem = null;
                draggedEquipItem = null;
            }
            else
            {
                // 隐藏丢弃按钮
                equipButtons.gameObject.SetActive(false);
            }
        }
    }

    // 使用按钮点击事件
    void OnUseButtonClick()
    {
        equipButtons.gameObject.SetActive(false);
        StartCoroutine(ItemUseManager.Instance.Use(character.uuid, useOrDropItem));
    }

    // 回收按钮点击事件
    void OnSellButtonClick()
    {
        equipButtons.gameObject.SetActive(false);
        if (GameManager.Instance.currentPageType == PageType.shop)
        {
            UIConfirmWindow confirmWindow = UIManager.Instance.Show<UIConfirmWindow>();
            confirmWindow.UpdateTip("确认要回收这件物品吗？");
            confirmWindow.OnCloseSubject.Subscribe(closeInfo => {
                switch (closeInfo.Item2) 
                {
                    case UIWindow.WindowResult.Yes:
                        //回收价格50%
                        GameManager.Instance.FeatherCoinChanged((int)(useOrDropItem.noDiscountPrice * 0.5f));
                        ItemUseManager.Instance.RepoDrop(useOrDropItem);
                        break;
                    case UIWindow.WindowResult.No:
                    case UIWindow.WindowResult.None:
                        break;
                }
            });
        } else {
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateGeneralTip("0006");
        }
    }

    // 丢弃按钮点击事件
    void OnDropButtonClick()
    {
        equipButtons.gameObject.SetActive(false);
        UIConfirmWindow confirmWindow = UIManager.Instance.Show<UIConfirmWindow>();
        confirmWindow.UpdateTip("确认要丢弃这件物品吗？");
        confirmWindow.OnCloseSubject.Subscribe(closeInfo => {
            switch (closeInfo.Item2) 
            {
                case UIWindow.WindowResult.Yes:
                    ItemUseManager.Instance.RepoDrop(useOrDropItem);
                    break;
                case UIWindow.WindowResult.No:
                case UIWindow.WindowResult.None:
                    break;
            }
        }); 
    }

    public void UpdateCharacter(CharacterModel character)
    {
        this.character = character;
        this.battleItem = null;
        this.characterIcon.overrideSprite = Resloader.LoadSprite(character.Resource, ConstValue.battleItemsPath);
        RefreshBag(character.backpack);
    }

    public void UpdateBattleItem(BattleItem battleItem)
    {
        this.character = null;
        this.battleItem = battleItem;
        this.characterIcon.overrideSprite = Resloader.LoadSprite(battleItem.Resource, ConstValue.battleItemsPath);
        RefreshBag(battleItem.backpack);
    }

    public void RefreshBag(Backpack backpack)
    {
        GameUtil.Instance.DetachChildren(equipFather);

        equipItems.Clear();

        foreach (var equip in backpack.equips)
        {
            GameObject temp = Instantiate(itemPrefab, this.equipFather);
            temp.GetComponent<HintComponent>().Setup(equip);
            UIEquipItem equipItem = temp.GetComponent<UIEquipItem>();
            equipItem.storeItem = equip;
            equipItem.ownerID = character?.uuid ?? battleItem?.uuid;
            temp.GetComponent<Image>().overrideSprite = Resloader.LoadSprite(equip.equipModel.equipResource, ConstValue.equipsPath);
            temp.transform.rotation = temp.transform.rotation * Quaternion.Euler(0, 0, equip.rotationAngle);
            Vector3 tempVector = equipSlots[equip.position.x * 3 + equip.position.y].position;
            temp.transform.position = tempVector;
            tempVector = temp.transform.TransformPoint(-equip.equipModel.originOffset * GlobalAccess.equipSizeBagMultiply);
            temp.transform.position = tempVector;
            temp.GetComponent<RectTransform>().sizeDelta = equip.equipModel.occupiedRect * GlobalAccess.equipSizeBagMultiply;
            equipItem.SetAndRecord(tempVector);
            equipItems.Add(equipItem);
        }
    }

    //public Dictionary<Vector2Int, StoreItemModel> MergeDictionaryByUuid(Dictionary<Vector2Int, StoreItemModel> originalDictionary)
    //{
    //    // 分组
    //    var groups = originalDictionary.Where(kv => kv.Value != null).GroupBy(kv => kv.Value.uuid);

    //    // 合并后的新字典
    //    Dictionary<Vector2Int, StoreItemModel> mergedDictionary = new Dictionary<Vector2Int, StoreItemModel>();

    //    foreach (var group in groups)
    //    {
    //        // 计算每个组中 Vector2Int 的 x * 10 + y 的值，并找到最小的键值对
    //        KeyValuePair<Vector2Int, StoreItemModel> minKeyValuePair = group
    //            .OrderBy(kv => kv.Key.x * 100 + kv.Key.y)
    //            .FirstOrDefault();

    //        // 将最小的键值对添加到新字典中
    //        if (minKeyValuePair.Key != null && !mergedDictionary.ContainsKey(minKeyValuePair.Key))
    //        {
    //            mergedDictionary.Add(minKeyValuePair.Key, minKeyValuePair.Value);
    //        }
    //    }

    //    return mergedDictionary;
    //}

    private BehaviorSubject<bool> isRotating = new BehaviorSubject<bool>(false); // 标志是否正在播放旋转动画

    public void Rotate(UIEquipItem equipItem, int angle, float duration)
    {
        if (!isRotating.Value)
        {
            equipItem.storeItem.Rotate(angle);
            // 如果当前没有正在播放的旋转动画，则启动旋转协程
            Quaternion startRotation = equipItem.transform.rotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, angle);
            Debug.Log("startRotation: " + startRotation.eulerAngles + "endRotation: " + endRotation.eulerAngles);
            StartCoroutine(GameUtil.Instance.RotateCoroutine(equipItem.transform, startRotation, endRotation, duration, isRotating)); // 启动旋转协程
        }
    }
}
