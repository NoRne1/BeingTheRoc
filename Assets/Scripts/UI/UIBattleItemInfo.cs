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
    private System.IDisposable disposable;
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
            disposable.IfNotNull(dis => { dis.Dispose(); });
            disposable = NorneStore.Instance.ObservableObject<BattleItem>(item)
            .AsObservable().TakeUntilDestroy(this).Subscribe(item =>
            {
                switch (item.battleItemType)
                {
                    case BattleItemType.enemy:
                    case BattleItemType.player:
                        hpSlider.maxValue = item.MaxHP;
                        icon.overrideSprite = Resloader.LoadSprite(item.Resource);
                        nameText.text = item.Name;
                        hpSlider.value = item.currentHP;
                        sliderText.text = item.currentHP + "/" + item.MaxHP;
                        energyPool.ReturnAllObject();
                        for (int i = 0; i < item.currentEnergy; i++)
                        {
                            energyPool.GetObjectFromPool();
                        }
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
            });
        }
    }

    public void ShakeEnergy()
    {
        energyFather.GetComponent<ShakeEffect>().TriggerShake();
    }

    private Coroutine blinkCoroutine;
    public void BlinkEnergy()
    {
        var energyIcon = energyPool.GetObjectFromPool();
        energyIcon.SetActive(false);
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(GameUtil.Instance.BlinkObject(energyIcon, 0.3f, 2));
    }
}
