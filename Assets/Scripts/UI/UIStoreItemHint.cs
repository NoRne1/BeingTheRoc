using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIStoreItemHint : MonoBehaviour
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

    private ObjectPool energyPool;
    // Start is called before the first frame update
    void Start()
    {
        energyPool = new ObjectPool(energyPrefab, 2, energyFather);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(StoreItemModel item)
    {
        equipRange.Setup(item);
        titleBorder.color = GlobalAccess.GetLevelColor(item.level);
        title.text = item.title;
        energyPool.ReturnAllObject();
        if (item.takeEnergy == 0)
        {
            energyFather.gameObject.SetActive(false);
        } else
        {
            for (int i = 0; i < item.takeEnergy; i++)
            {
                energyPool.GetObjectFromPool();
            }
            energyFather.gameObject.SetActive(true);
        }
        
        if(item.GetTargetRangeResource() != null)
        {
            attackRange.overrideSprite = Resloader.LoadSprite(item.GetTargetRangeResource());
            attackRange.gameObject.SetActive(true);
        } else
        {
            attackRange.gameObject.SetActive(false);
        }

        desc.text = item.desc;

        if (item.extraEntryID1 >= 0)
        {
            AddExtraHint(DataManager.Instance.ExtraEntrys[item.extraEntryID1]);
        }
        if (item.extraEntryID2 >= 0)
        {
            AddExtraHint(DataManager.Instance.ExtraEntrys[item.extraEntryID2]);
        }
        if (item.extraEntryID3 >= 0)
        {
            AddExtraHint(DataManager.Instance.ExtraEntrys[item.extraEntryID3]);
        }
    }

    private void AddExtraHint(ExtraEntryDesc desc)
    {
        UIExtraHint extraHint = Instantiate(extraHintPrefab, extraHintFather).GetComponent<UIExtraHint>();
        extraHint.Setup(desc);
    }
}
