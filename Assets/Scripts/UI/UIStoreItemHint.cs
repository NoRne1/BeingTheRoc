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

    public void updatePostion()
    {
        // 获取鼠标在屏幕上的位置
        Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        // 计算大小
        Vector2 prefabSize = gameObject.GetComponent<RectTransform>().sizeDelta;

        // 计算合适的偏移量
        Vector2 offset = GameUtil.Instance.CalculateOffset(screenPosition, prefabSize);

        Vector2 temp = Camera.main.ScreenToWorldPoint(screenPosition + offset);
        // 更新位置
        gameObject.transform.position = new Vector3(temp.x, temp.y, gameObject.transform.position.z);
    }

    public void Setup(StoreItemModel item)
    {
        leftPlaceHolder.SetActive(!equipRange.Setup(item));
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
            rightPlaceHolder.SetActive(false);
        } else
        {
            attackRange.gameObject.SetActive(false);
            rightPlaceHolder.SetActive(true);
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

    public IEnumerator InitLayoutPosition()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        updatePostion();
    }
}

