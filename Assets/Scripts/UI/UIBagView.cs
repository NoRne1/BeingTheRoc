using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIBagView : MonoBehaviour
{
    public List<UIRepositorSlot> itemButtons;
    public System.IDisposable disposable;
    // Start is called before the first frame update
    void Start()
    {
        itemButtons = new List<UIRepositorSlot>(GetComponentsInChildren<UIRepositorSlot>());

        disposable = GameManager.Instance.repository.itemsRelay.AsObservable()
            .TakeUntilDestroy(this).Subscribe(items =>
        {
            for(int i = 0; i < itemButtons.Count; i++)
            {
                Transform itemImage = itemButtons[i].transform.GetChild(0);
                if (i < items.Count)
                {
                    itemButtons[i].Setup(items[i]);
                } else
                {
                    itemButtons[i].GetComponent<UIRepositorSlot>().Setup(null);
                }
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        // 确保子对象订阅被取消
        disposable.IfNotNull(dis => { dis.Dispose(); });
    }
}
