using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//用作装备强化
public class UIStoreItemPanel : MonoBehaviour
{
    public RectTransform mainHint;
    public UIEquipRange equipRange;
    public Image titleBorder;
    public TextMeshProUGUI title;
    public Transform energyFather;
    public GameObject energyPrefab;
    public Image attackRange;
    public TextMeshProUGUI desc;
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
    }

    public void Setup(StoreItemModel item)
    {
        if (item == null) { return; }
        leftPlaceHolder.SetActive(!equipRange.Setup(item));
        titleBorder.color = GlobalAccess.GetLevelColor(item.level);
        title.text = item.title;
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
            desc.text = item.desc;
        }
        foreach (var index in Enumerable.Range(0, entrys.Count))
        {
            if (index < item.equipModel.extraEntryModels.Count)
            {
                entrys[index].Setup(item.equipModel.extraEntryModels[index], true);
                entrys[index].gameObject.SetActive(true);
            } else {
                entrys[index].gameObject.SetActive(false);
            }
        }

        // if (item.ExtraEntry1 >= 0)
        // {
        //     AddEntry(DataManager.Instance.ExtraEntrys[item.ExtraEntry1]);
        // }
        // if (item.ExtraEntry2 >= 0)
        // {
        //     AddEntry(DataManager.Instance.ExtraEntrys[item.ExtraEntry2]);
        // }
        // if (item.ExtraEntry3 >= 0)
        // {
        //     AddEntry(DataManager.Instance.ExtraEntrys[item.ExtraEntry3]);
        // }
        // Canvas.ForceUpdateCanvases();
        // LayoutRebuilder.ForceRebuildLayoutImmediate(entrysFather.GetComponent<RectTransform>());
    }
}
