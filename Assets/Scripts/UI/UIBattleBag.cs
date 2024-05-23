using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.TextCore.Text;

public class UIBattleBag : MonoBehaviour
{
    public Transform equipFather;
    public GameObject itemPrefab; // 大装备的预制体

    public List<Transform> equipSlots;
    private List<UIEquipItem> equipItems = new List<UIEquipItem>();

    public BattleItem battleItem;
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
        this.battleItem = battleItem;
        gameObject.SetActive(battleItem != null);
        if (battleItem != null)
        {
            GameUtil.Instance.DetachChildren(equipFather);
            equipItems.Clear();
            foreach (var equip in battleItem.backpack.equips)
            {
                GameObject temp = Instantiate(itemPrefab, this.equipFather);
                UIEquipItem equipItem = temp.GetComponent<UIEquipItem>();
                equipItem.storeItem = equip;
                equipItem.ownerID = battleItem.uuid;
                temp.GetComponent<Image>().overrideSprite = Resloader.LoadSprite(equip.iconResource2, ConstValue.equipsPath);
                temp.transform.rotation = temp.transform.rotation * Quaternion.Euler(0, 0, equip.rotationAngle);
                Vector3 tempVector = equipSlots[equip.position.x * 3 + equip.position.y].position;
                temp.transform.position = tempVector;
                tempVector = temp.transform.TransformPoint(-equip.originOffset * GlobalAccess.equipSizeBattleMultiply);
                temp.transform.position = tempVector;
                temp.GetComponent<RectTransform>().sizeDelta = equip.occupiedRect * GlobalAccess.equipSizeBattleMultiply;
                equipItem.SetAndRecord(tempVector);
                temp.GetComponent<Button>().OnClickAsObservable().TakeUntilDestroy(this).Subscribe(_ =>
                {
                    BattleManager.Instance.EquipClicked(battleItem.uuid, equipItem);
                });
                equipItems.Add(equipItem);
            }
        }
    }
}
