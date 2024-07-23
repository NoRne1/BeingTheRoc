using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class UIBuffsPanel : MonoBehaviour
{
    public Transform buffsFather;
    public GameObject showBuffItemPrefab;
    public System.IDisposable disposable;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(BattleItem battleItem)
    {
        if (battleItem != null)
        {
            disposable?.Dispose();
            disposable = NorneStore.Instance.ObservableObject<BattleItem>(battleItem)
                .AsObservable().TakeUntilDestroy(this).Subscribe(bi =>
                {
                    GameUtil.Instance.DetachChildren(buffsFather);
                    foreach (var buff in bi.buffCenter.GetNewestBuffs(-1))
                    {
                        var showBuffItem = Instantiate(showBuffItemPrefab, buffsFather);
                        showBuffItem.GetComponent<UIShowBuffItem>().Setup(buff);
                    }
                });
        }
        else
        {
            Debug.Log("UIBuffsPanel setup battleItem is null");
        }
    }

    private void OnDestroy()
    {
        // 确保在对象销毁时取消订阅
        disposable?.Dispose();
    }
}
