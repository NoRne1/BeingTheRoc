using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.Text;
using UnityEngine.EventSystems;
using UniRx;
using static UnityEditor.Progress;

public class UIEquipItem : MonoBehaviour, IPointerUpHandler, IDragHandler, IPointerClickHandler
{
    public StoreItemModel item;
    public CharacterModel character;

    public Vector3 recordPosition;
    public bool leftButtonDown;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("OnPointerUp");
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null && (hit.collider.CompareTag("Repositor") || hit.collider.CompareTag("Item")))
            {
                GameManager.Instance.repository.AddItem(item);
                character.backpack.RemoveItemsByUUID(item.uuid);
                item.unEquip();
                Destroy(this.gameObject);
            }
            else if (hit.collider != null && hit.collider.CompareTag("EquipSlot"))
            {
                // 检查是否可以放置装备
                Vector2Int gridPosition = hit.collider.GetComponent<UIEquipSlot>().position;
                if (character.backpack.MoveTo(item, gridPosition))
                {
                    Vector3 tempVector = hit.collider.GetComponent<UIEquipSlot>().transform.position;
                    this.transform.position = tempVector;
                    this.recordPosition = tempVector;
                }
                else
                {
                    this.transform.position = recordPosition;
                }
            }
            else
            {
                this.transform.position = recordPosition;
            }
            this.leftButtonDown = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 更新装备图像的位置，跟随鼠标移动
            Vector3 tempVector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            this.transform.position = new Vector3(tempVector.x, tempVector.y, 0);
            this.leftButtonDown = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && this.leftButtonDown)
        {
            //左键在拖拽时，按下右键进行旋转操作
            Rotate(90, 2f);
        }
    }

    private BehaviorSubject<bool> isRotating = new BehaviorSubject<bool>(false); // 标志是否正在播放旋转动画

    public void Rotate(int angle, float duration)
    {
        if (!isRotating.Value)
        {
            this.item.Rotate(angle);
            // 如果当前没有正在播放的旋转动画，则启动旋转协程
            Quaternion startRotation = transform.rotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, angle);
            StartCoroutine(GameUtil.Instance.RotateCoroutine(transform, startRotation, endRotation, duration, isRotating)); // 启动旋转协程
        }
    }
}
