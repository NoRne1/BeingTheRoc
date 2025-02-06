using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UIStoreItemHint : UIHintBase
{
    public RectTransform mainHint;
    public UIEquipRange equipRange;
    public Image titleBorder;
    public TextMeshProUGUI title;
    public Transform energyFather;
    public GameObject energyPrefab;
    public Image attackRange;
    public TextMeshProUGUI desc;
    public Transform extraHintFather;
    public GameObject extraHintPrefab;

    public GameObject leftPlaceHolder;
    public GameObject rightPlaceHolder;

    public List<UIEntryDesc> entrys;

    private ObjectPool energyPool;
    // Start is called before the first frame update
    void Awake()
    {
        energyPool = new ObjectPool(energyPrefab, 2, energyFather);
    }

    // Update is called once per frame
    void Update()
    {
        updatePostion();
    }

    public void Setup(StoreItemModel item)
    {
        Setup((StoreItemDefine)item);
        if (item.equipModel != null)
        {
            foreach (var index in Enumerable.Range(0, entrys.Count))
            {
                if (index < item.equipModel.extraEntryModels.Count)
                {
                    entrys[index].Setup(item.equipModel.extraEntryModels[index], false);
                    entrys[index].gameObject.SetActive(true);
                } else {
                    entrys[index].gameObject.SetActive(false);
                }
            }
        }
    }

    public void Setup(StoreItemDefine item)
    {
        leftPlaceHolder.SetActive(!equipRange.Setup(item));
        titleBorder.color = GlobalAccess.GetLevelColor(item.level);
        title.text = GameUtil.Instance.GetDirectDisplayString(item.title);
        energyPool.ReturnAllObject();
        attackRange.gameObject.SetActive(false);
        if (item.type == ItemType.equip && DataManager.Instance.EquipDefines.ContainsKey(item.subID))
        {
            EquipDefine equipDefine = DataManager.Instance.EquipDefines[item.subID];
            if (equipDefine.takeEnergy == 0)
            {
                energyFather.gameObject.SetActive(false);
            }
            else
            {
                for (int i = 0; i < equipDefine.takeEnergy; i++)
                {
                    energyPool.GetObjectFromPool();
                }
                energyFather.gameObject.SetActive(true);
            }

            if (equipDefine.GetTargetRangeResource() != null)
            {
                attackRange.overrideSprite = Resloader.LoadSprite(equipDefine.GetTargetRangeResource(), ConstValue.equipAttackRangePath);
                attackRange.gameObject.SetActive(true);
                rightPlaceHolder.SetActive(false);
            }
            else
            {
                attackRange.gameObject.SetActive(false);
                rightPlaceHolder.SetActive(true);
            }
        }

        if (item.type == ItemType.food)
        {
            desc.text = ((StoreItemModel)item).GetFoodDesc();
        } else {
            desc.text = GameUtil.Instance.GetDirectDisplayString(item.desc);
        }

        foreach (var index in Enumerable.Range(0, entrys.Count))
        {
            entrys[index].gameObject.SetActive(false);
        }

        if (item.ExtraEntry1 >= 0)
        {
            AddExtraHint(DataManager.Instance.ExtraEntrys[item.ExtraEntry1]);
        }
        if (item.ExtraEntry2 >= 0)
        {
            AddExtraHint(DataManager.Instance.ExtraEntrys[item.ExtraEntry2]);
        }
        if (item.ExtraEntry3 >= 0)
        {
            AddExtraHint(DataManager.Instance.ExtraEntrys[item.ExtraEntry3]);
        }
        StartCoroutine(SetupComplete());
    }

    private void AddExtraHint(ExtraEntryDesc desc)
    {
        UIExtraHint extraHint = Instantiate(extraHintPrefab, extraHintFather).GetComponent<UIExtraHint>();
        extraHint.Setup(desc);
    }
}

