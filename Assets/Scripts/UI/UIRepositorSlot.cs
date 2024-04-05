using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRepositorSlot : MonoBehaviour
{
    public StoreItemModel item;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void Update()
    {

    }

    public void UpdateItem(StoreItemModel item)
    {
        Transform itemImage = transform.GetChild(0);
        this.item = item;
        if (item != null)
        {
            itemImage.GetComponent<Image>().overrideSprite =
                Resloader.LoadSprite(item.iconResource);
            itemImage.gameObject.SetActive(true);
        }
        else
        {
            itemImage.gameObject.SetActive(false);
        }
    }
}
