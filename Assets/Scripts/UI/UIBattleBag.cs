using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using System.Linq;

public class UIBattleBag : MonoBehaviour
{
    public Transform equipFather;
    public GameObject itemPrefab; // 大装备的预制体

    public List<Transform> equipSlots;
    private List<StoreItemModel> oldEquips = new List<StoreItemModel>();
    private List<UIEquipItem> equipItems = new List<UIEquipItem>();

    public string itemID;
    // Start is called before the first frame update
    void Start()
    {
        
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
            var battleItem = GlobalAccess.GetBattleItem(itemID);
            if (battleItem.backpack == null) { gameObject.SetActive(false); return; }
            NorneStore.Instance.ObservableObject(new BattleItem(itemID)).AsObservable().TakeUntilDestroy(this)
                .Select(battleItem => battleItem.backpack.equips).Subscribe(equips =>
                {
                    if (CheckEquipsChanged(equips))
                    {
                        oldEquips = equips;
                        GameUtil.Instance.DetachChildren(equipFather);
                        equipItems.Clear();
                        foreach (var equip in equips)
                        {
                            GameObject temp = Instantiate(itemPrefab, this.equipFather);
                            UIEquipItem equipItem = temp.GetComponent<UIEquipItem>();
                            equipItem.storeItem = equip;
                            equipItem.ownerID = battleItem.uuid;
                            temp.GetComponent<Image>().overrideSprite = Resloader.LoadSprite(equip.equipModel.equipResource, ConstValue.equipsPath);
                            temp.transform.rotation = temp.transform.rotation * Quaternion.Euler(0, 0, equip.rotationAngle);
                            Vector3 tempVector = equipSlots[equip.position.x * 3 + equip.position.y].position;
                            temp.transform.position = tempVector;
                            tempVector = temp.transform.TransformPoint(-equip.equipModel.originOffset * GlobalAccess.equipSizeBattleMultiply);
                            temp.transform.position = tempVector;
                            temp.GetComponent<RectTransform>().sizeDelta = equip.equipModel.occupiedRect * GlobalAccess.equipSizeBattleMultiply;
                            equipItem.SetAndRecord(tempVector);
                            temp.GetComponent<Button>().OnClickAsObservable().TakeUntilDestroy(this).Subscribe(_ =>
                            {
                                BattleManager.Instance.EquipClicked(battleItem.uuid, equipItem);
                            });
                            equipItems.Add(equipItem);
                        }
                    }
                });
        }
    }

    private bool CheckEquipsChanged(List<StoreItemModel> newEquips)
    {
        if (oldEquips == null || newEquips == null)
        {
            throw new ArgumentNullException(oldEquips == null ? nameof(oldEquips) : nameof(newEquips));
        }

        if (oldEquips.Count != newEquips.Count)
        {
            return true; // Lists are of different sizes, so they are different
        }

        // Create HashSet from UUIDs for quick comparison
        var set1 = new HashSet<string>(oldEquips.Select(item => item.uuid));
        var set2 = new HashSet<string>(newEquips.Select(item => item.uuid));

        // Compare sets
        return !set1.SetEquals(set2);
    }
}
