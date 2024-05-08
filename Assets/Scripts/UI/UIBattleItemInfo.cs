using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class UIBattleItemInfo : MonoBehaviour
{
    public BattleItem item;

    public Transform energyFather;
    public GameObject energyPrefab;
    public Transform buffFather;
    public GameObject buffPrefab;
    public Slider hpSlider;
    public TextMeshProUGUI sliderText;
    public Image icon;
    public TextMeshProUGUI nameText;

    private ObjectPool energyPool;
    private ObjectPool buffPool;
    private System.IDisposable hpDisposable;
    private System.IDisposable energyDisposable;
    private System.IDisposable buffDisposable;
    // Start is called before the first frame update
    void Start()
    {
        energyPool = new ObjectPool(energyPrefab, 4, energyFather);
        buffPool = new ObjectPool(buffPrefab, 4, buffFather);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(BattleItem item)
    {
        this.item = item;
        this.gameObject.SetActive(item != null);
        if (item != null)
        {
            switch (item.battleItemType)
            {
                case BattleItemType.enemy:
                case BattleItemType.player:
                    hpSlider.maxValue = item.MaxHP;
                    icon.overrideSprite = Resloader.LoadSprite(item.Resource);
                    nameText.text = item.Name;
                    hpDisposable.IfNotNull(disposable => { disposable.Dispose(); });
                    hpDisposable = item.currentHp.AsObservable().TakeUntilDestroy(this).Subscribe(hp =>
                    {
                        hpSlider.value = hp;
                        sliderText.text = hp + "/" + item.MaxHP; 
                    });
                    energyDisposable.IfNotNull(disposable => { disposable.Dispose(); });
                    energyDisposable = item.currentEnergy.AsObservable().TakeUntilDestroy(this).Subscribe(energy =>
                    {
                        energyPool.ReturnAllObject();
                        for (int i = 0; i < energy; i++)
                        {
                            energyPool.GetObjectFromPool();
                        }
                    });
                    //todo actual buff
                    buffPool.ReturnAllObject();
                    for (int i = 0; i < Random.Range(1, 3); i++)
                    {
                        buffPool.GetObjectFromPool();
                    }
                    break;
                case BattleItemType.sceneItem:
                case BattleItemType.time:
                    Debug.Log("UIBattleItemInfo setup error");
                    break;
            }
        }
    }
}
