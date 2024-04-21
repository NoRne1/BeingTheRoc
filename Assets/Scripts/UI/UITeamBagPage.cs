using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;
using Unity.Burst.CompilerServices;
using static UnityEditor.Progress;
using Unity.VisualScripting;

public class UITeamBagPage : MonoBehaviour
{
    public Image characterIcon;

    public List<Transform> equipSlots;

    public List<Button> itemButtons;

    public GameObject itemPrefab; // 大装备的预制体
    private List<UIEquipItem> equipItems = new List<UIEquipItem>();
    private UIEquipItem draggedEquipItem; // 大装备的实例
    private UIRepositorSlot currentItem; // 仓库中当前被拖动的装备脚本
    
    private Vector3 pressPosition; // 鼠标按下的位置
    private bool isDragging; // 是否正在拖动装备

    private float moveThreshold = 10f; //移动检测阈值

    // 丢弃按钮
    public UIEquipButtons equipButtons;

    public CharacterModel character;

    public Transform equipFather;
    public GameObject repositor;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.repository.itemsRelay.AsObservable()
            .TakeUntilDestroy(this).Subscribe(items =>
        {
            for(int i = 0; i < itemButtons.Count; i++)
            {
                Transform itemImage = itemButtons[i].transform.GetChild(0);
                if (i < items.Count)
                {
                    itemButtons[i].GetComponent<UIRepositorSlot>().UpdateItem(items[i]);
                } else
                {
                    itemButtons[i].GetComponent<UIRepositorSlot>().UpdateItem(null);
                }
            }
        });

        // 隐藏丢弃按钮
        equipButtons.gameObject.SetActive(false);

        // 添加丢弃按钮的点击事件
        equipButtons.dropButton.onClick.AddListener(OnDropButtonClick);
        equipButtons.useButton.onClick.AddListener(OnUseButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("Item") && hit.collider.GetComponent<UIRepositorSlot>().item != null)
            {
                currentItem = hit.collider.GetComponent<UIRepositorSlot>();
                // 记录鼠标按下的位置
                pressPosition = Input.mousePosition;
                isDragging = false;
            }
            else if (hit.collider != null && hit.collider.CompareTag("EquipSlot"))
            {
                Vector2 vector2 = hit.collider.GetComponent<UIEquipSlot>().position;
                //点击的位置是否有装备
                draggedEquipItem = equipItems.Where(equipTtem =>
                {
                    bool result = false;
                    for (int i = 0; i < equipTtem.item.OccupiedCells.Count; i++)
                    {
                        Vector2 pos = equipTtem.item.OccupiedCells[i];
                        if (pos + equipTtem.item.position == vector2)
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
                    draggedEquipItem.item = currentItem.item;
                    draggedEquipItem.character = character;
                    draggedItem.GetComponent<RectTransform>().sizeDelta = draggedEquipItem.item.occupiedRect;
                    draggedItem.GetComponent<Image>().overrideSprite = Resloader.LoadSprite(currentItem.item.iconResource2);
                }
            }
            if (isDragging && draggedEquipItem != null)
            {
                Vector3 tempVector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                draggedEquipItem.transform.position = new Vector3(tempVector.x, tempVector.y, 0);
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
                if (!isDragging && (Input.mousePosition - pressPosition).magnitude <= moveThreshold)
                {
                    Vector3 tempPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(75, -75, 0));
                    equipButtons.gameObject.SetActive(true);
                    equipButtons.transform.position = new Vector3(tempPosition.x, tempPosition.y, 0);
                }
                // 如果鼠标按下后移动了一定距离，则执行装备逻辑
                else if (isDragging && draggedEquipItem != null)
                {
                    // 获取鼠标松开时的位置
                    Vector2 checkPosition = draggedEquipItem.transform.TransformPoint(draggedEquipItem.item.originOffset);
                    RaycastHit2D equipHit = Physics2D.Raycast(checkPosition, Vector2.zero);
                    Vector2 mousePosition2D = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                    RaycastHit2D repHit = Physics2D.Raycast(mousePosition2D, Vector2.zero);

                    if (currentItem != null)
                    {
                        if (equipHit.collider != null && equipHit.collider.CompareTag("EquipSlot"))
                        {
                            // 检查是否可以放置装备
                            Vector2Int gridPosition = equipHit.collider.GetComponent<UIEquipSlot>().position;
                            if (EquipManager.Instance.Equip(character, draggedEquipItem.item, gridPosition))
                            {
                                Vector3 tempVector = equipHit.collider.GetComponent<UIEquipSlot>().transform.position;
                                draggedEquipItem.SetAndRecord(tempVector);
                            }
                            else
                            {
                                draggedEquipItem.item.ResetRotate();
                                Destroy(draggedEquipItem.gameObject);
                            }
                        } else if (draggedEquipItem != null)
                        {
                            draggedEquipItem.item.ResetRotate();
                            Destroy(draggedEquipItem.gameObject);
                        }
                    }
                    else
                    {
                        //draggedEquipItem != null
                        if (repHit.collider != null && GameUtil.Instance.IsPointInsideGameObject(repositor, Input.mousePosition))
                        {
                            //放回到仓库
                            EquipManager.Instance.Unequip(character, draggedEquipItem.item);
                            Destroy(draggedEquipItem.gameObject);
                        }
                        else if (equipHit.collider != null && equipHit.collider.CompareTag("EquipSlot"))
                        {
                            // 检查是否可以放置装备
                            Vector2Int gridPosition = equipHit.collider.GetComponent<UIEquipSlot>().position;
                            if (character.backpack.MoveTo(draggedEquipItem.item, gridPosition))
                            {
                                Vector3 tempVector = equipHit.collider.GetComponent<UIEquipSlot>().transform.position;
                                draggedEquipItem.SetAndRecord(tempVector);
                            }
                            else
                            {
                                draggedEquipItem.item.ResetRotate();
                                draggedEquipItem.Reset();
                            }
                        } else
                        {
                            draggedEquipItem.item.ResetRotate();
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
        // 在这里实现使用装备的逻辑
        Debug.Log("Use button clicked!");
    }

    // 丢弃按钮点击事件
    void OnDropButtonClick()
    {
        // 在这里实现丢弃装备的逻辑
        Debug.Log("Drop button clicked!");
    }

    public void UpdateCharacter(CharacterModel character)
    {
        this.character = character;
        this.characterIcon.overrideSprite = Resloader.LoadSprite(character.Resource);
        RefreshBag(character);
    }

    public void RefreshBag(CharacterModel character)
    {
        // 遍历子节点
        foreach (Transform child in this.equipFather)
        {
            // 销毁子节点
            Destroy(child.gameObject);
        }

        // 清空子节点列表
        equipFather.DetachChildren();

        equipItems.Clear();

        foreach (var equip in character.backpack.equips)
        {
            GameObject temp = Instantiate(itemPrefab, this.equipFather);
            UIEquipItem equipItem = temp.GetComponent<UIEquipItem>();
            equipItem.item = equip;
            equipItem.character = character;
            temp.GetComponent<Image>().overrideSprite = Resloader.LoadSprite(equip.iconResource2);
            temp.transform.rotation = temp.transform.rotation * Quaternion.Euler(0, 0, equip.rotationAngle);
            Vector3 tempVector = equipSlots[equip.position.x * 3 + equip.position.y].position;
            temp.transform.position = tempVector;
            tempVector = temp.transform.TransformPoint(-equip.originOffset);
            temp.transform.position = tempVector;
            temp.GetComponent<RectTransform>().sizeDelta = equip.occupiedRect;
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
            equipItem.item.Rotate(angle);
            // 如果当前没有正在播放的旋转动画，则启动旋转协程
            Quaternion startRotation = equipItem.transform.rotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, angle);
            Debug.Log("startRotation: " + startRotation.eulerAngles + "endRotation: " + endRotation.eulerAngles);
            StartCoroutine(GameUtil.Instance.RotateCoroutine(equipItem.transform, startRotation, endRotation, duration, isRotating)); // 启动旋转协程
        }
    }
}
