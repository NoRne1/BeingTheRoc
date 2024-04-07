using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.Text;
using UnityEngine.EventSystems;

public class UIEquipItem : MonoBehaviour, IPointerUpHandler, IDragHandler
{
    public StoreItemModel item;
    public CharacterModel character;

    public Vector3 recordPosition;

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
        Debug.Log("OnPointerUp");
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider != null && (hit.collider.CompareTag("Repositor") || hit.collider.CompareTag("Item")))
        {
            GameManager.Instance.repository.AddItem(item);
            character.backpack.RemoveItemsByUUID(item.uuid);
            Destroy(this.gameObject);
        } else if (hit.collider != null && hit.collider.CompareTag("EquipSlot"))
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
        } else
        {
            this.transform.position = recordPosition;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        // 更新装备图像的位置，跟随鼠标移动
        Vector3 tempVector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.transform.position = new Vector3(tempVector.x, tempVector.y, 0);
    }
}
