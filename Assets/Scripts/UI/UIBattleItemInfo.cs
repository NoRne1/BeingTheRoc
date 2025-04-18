using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class UIBattleItemInfo : MonoBehaviour
{
    public string itemID;

    public Transform energyFather;
    public GameObject energyPrefab;
    public Transform buffFather;
    public GameObject buffPrefab;
    public Slider hpSlider;
    public Slider shieldSlider;
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

    public void Setup(string uuid)
    {
        this.itemID = uuid;
        gameObject.SetActive(itemID != null);
        if (itemID != null)
        {
            disposable.IfNotNull(dis => { dis.Dispose(); });
            disposable = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(itemID))
            .AsObservable().TakeUntilDestroy(this).Subscribe(item =>
            {
                switch (item.type)
                {
                    case BattleItemType.character:
                        hpSlider.maxValue = item.attributes.MaxHP;
                        shieldSlider.maxValue = item.attributes.MaxHP;
                        icon.overrideSprite = Resloader.LoadSprite(item.Resource, ConstValue.battleItemsPath);
                        nameText.text = item.Name;
                        hpSlider.value = item.attributes.currentHP;
                        //value只是为了显示
                        shieldSlider.value = Mathf.Min(shieldSlider.maxValue, item.attributes.currentShield);
                        sliderText.text = item.attributes.currentHP + "/" + item.attributes.MaxHP;
                        energyPool.ReturnAllObject();
                        for (int i = 0; i < item.attributes.currentEnergy; i++)
                        {
                            energyPool.GetObjectFromPool();
                        }
                        //todo actual buff
                        buffPool.ReturnAllObject();
                        var buffList = item.buffCenter.GetNewestBuffs(5);
                        for (int i = 0; i < buffList.Count; i++)
                        {
                            buffPool.GetObjectFromPool().GetComponent<UIBattleBuffIcon>().Setup(buffList[i]);
                        }
                        break;
                    case BattleItemType.time:
                    case BattleItemType.quitTime:
                        Debug.Log("UIBattleItemInfo setup error");
                        break;
                    case BattleItemType.sceneItem:
                    case BattleItemType.granary:
                        hpSlider.maxValue = item.attributes.MaxHP;
                        shieldSlider.maxValue = item.attributes.MaxHP;
                        icon.overrideSprite = Resloader.LoadSprite(item.Resource, ConstValue.battleItemsPath);
                        nameText.text = item.Name;
                        hpSlider.value = item.attributes.currentHP;
                        //value只是为了显示
                        shieldSlider.value = Mathf.Min(shieldSlider.maxValue, item.attributes.currentShield);
                        sliderText.text = item.attributes.currentHP + "/" + item.attributes.MaxHP;
                        energyPool.ReturnAllObject();
                        for (int i = 0; i < item.attributes.currentEnergy; i++)
                        {
                            energyPool.GetObjectFromPool();
                        }
                        buffPool.ReturnAllObject();
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
