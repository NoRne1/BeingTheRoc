using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using static UnityEditor.Progress;

public class UITeamBagPage : MonoBehaviour
{
    public List<Button> itemButtons;

    public GameObject itemPrefab; // 大装备的预制体
    private GameObject draggedItem; // 大装备的实例
    private UIRepositorSlot currentItem; // 仓库中当前被拖动的装备脚本
    
    private Vector3 pressPosition; // 鼠标按下的位置
    private bool isDragging; // 是否正在拖动装备

    private float moveThreshold = 10f; //移动检测阈值

    private Vector3 offset; // 用于调整装备图像的位置
    // 丢弃按钮
    public Button discardButton;

    public CharacterModel character;

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
        discardButton.gameObject.SetActive(false);

        // 添加丢弃按钮的点击事件
        discardButton.onClick.AddListener(OnDiscardButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("Item"))
            {
                currentItem = hit.collider.GetComponent<UIRepositorSlot>();
                // 记录鼠标按下的位置
                pressPosition = Input.mousePosition;
                isDragging = false;

                // 计算偏移量
                //offset = draggedItem.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        if (Input.GetMouseButton(0) && currentItem != null)
        {
            // 如果鼠标按下后移动了一定距离，则执行装备拖动逻辑
            if (!isDragging && (Input.mousePosition - pressPosition).magnitude > moveThreshold)
            {
                isDragging = true;
                // 隐藏丢弃按钮
                discardButton.gameObject.SetActive(false);

                draggedItem = Instantiate(itemPrefab, this.transform);
                draggedItem.GetComponent<Image>().overrideSprite = Resloader.LoadSprite(currentItem.item.iconResource);
            }
            if (isDragging && draggedItem != null)
            {
                //Debug.Log("draggedItem.transform.position: " + draggedItem.transform.position);
                //Debug.Log("Input.mousePosition: " + Input.mousePosition);
                //Debug.Log("pressPosition: " + pressPosition);
                //Debug.Log("+Position: " + Camera.main.ScreenToWorldPoint(Input.mousePosition - pressPosition));
                //// 更新装备图像的位置，跟随鼠标移动
                //Vector3 offset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.ScreenToWorldPoint(pressPosition);
                Vector3 tempVector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                draggedItem.transform.position = new Vector3(tempVector.x, tempVector.y, 0);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (currentItem != null)
            {
                // 如果鼠标按下后未移动或移动距离小于一定距离，则显示丢弃按钮
                if (!isDragging && (Input.mousePosition - pressPosition).magnitude <= moveThreshold)
                {
                    discardButton.gameObject.SetActive(true);
                    Vector3 tempPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    discardButton.transform.position = new Vector3(tempPosition.x, tempPosition.y, 0);
                }
                // 如果鼠标按下后移动了一定距离，则执行装备逻辑
                else if (isDragging)
                {
                    // 获取鼠标松开时的位置
                    Vector2 releasePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(releasePosition), Vector2.zero);
                    if (hit.collider != null && hit.collider.CompareTag("EquipSlot"))
                    {
                        // 检查是否可以放置装备
                        Vector2Int gridPosition = hit.collider.GetComponent<UIEquipSlot>().position;
                        if (character.backpack.Place(currentItem.item, gridPosition))
                        {
                            Debug.Log("Place item at grid position: " + gridPosition);
                        }
                        else
                        {
                            Debug.Log("Cannot place item at grid position: " + gridPosition);
                            Destroy(draggedItem);
                        }
                    } else
                    {
                        Destroy(draggedItem);
                    }

                }
                currentItem = null;
            } else
            {
                // 隐藏丢弃按钮
                discardButton.gameObject.SetActive(false);
            }
        }
    }

    // 丢弃按钮点击事件
    void OnDiscardButtonClick()
    {
        // 在这里实现丢弃装备的逻辑
        Debug.Log("Discard button clicked!");
    }

    public void UpdateCharacter(CharacterModel character)
    {
        this.character = character;
        RefreshBag(character);
    }

    public void RefreshBag(CharacterModel character)
    {
        Debug.Log("RefreshBag");
    }
}
