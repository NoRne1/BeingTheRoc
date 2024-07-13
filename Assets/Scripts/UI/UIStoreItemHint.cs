using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIStoreItemHint : UIHintBase
{
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

    public void Setup(StoreItemDefine item)
    {
        leftPlaceHolder.SetActive(!equipRange.Setup(item));
        titleBorder.color = GlobalAccess.GetLevelColor(item.level);
        title.text = item.title;
        energyPool.ReturnAllObject();
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

        desc.text = item.desc;

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
        StartCoroutine(InitLayoutPosition());
    }

    private void AddExtraHint(ExtraEntryDesc desc)
    {
        UIExtraHint extraHint = Instantiate(extraHintPrefab, extraHintFather).GetComponent<UIExtraHint>();
        extraHint.Setup(desc);
    }
}

