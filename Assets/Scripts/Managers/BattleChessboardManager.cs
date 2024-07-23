using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class BattleChessboardManager
{
    private BattleManager battleManager = BattleManager.Instance;

    public UIChessboard chessBoard;
    public UICharacterPlaceBox placeBox;
    public GameObject battleItemPrefab;
    public Transform battleItemFather;
    public UIBattleItemInfo uiBattleItemInfo;
    public UIBattleBag uiBattleBag;

    public Subject<UIChessboardSlot> clickedSlot = new Subject<UIChessboardSlot>();
    public ClickSlotReason clickSlotReason = ClickSlotReason.none;
    private Vector2 lastSelectedPos = Vector2.positiveInfinity;

    public BehaviorSubject<int> currentPlaceIndex = new BehaviorSubject<int>(-1);

    public BattleChessboardManager(UIChessboard chessBoard, UICharacterPlaceBox placeBox,
        GameObject battleItemPrefab, Transform battleItemFather, UIBattleItemInfo uiBattleItemInfo,
        UIBattleBag uiBattleBag)
    {
        this.chessBoard = chessBoard;
        this.placeBox = placeBox;
        this.battleItemPrefab = battleItemPrefab;
        this.battleItemFather = battleItemFather;
        this.uiBattleItemInfo = uiBattleItemInfo;
        this.uiBattleBag = uiBattleBag;

        currentPlaceIndex.AsObservable().TakeUntilDestroy(battleManager).Subscribe(index =>
        {
            if (index < 0) { return; }
            if (index >= battleManager.battleItemManager.PlayerItemIDs.Count)
            {
                //放置完成，回合正式开始
                //需要手动把placebox隐藏
                placeBox.gameObject.SetActive(false);
                chessBoard.ResetColors();
                clickSlotReason = ClickSlotReason.viewCharacter;
                battleManager.roundManager.roundTime.OnNext((null, RoundTime.end));
                currentPlaceIndex.OnNext(-1);
                return;
            }
            placeBox.Setup(GlobalAccess.GetBattleItem(battleManager.battleItemManager.PlayerItemIDs[currentPlaceIndex.Value]).Resource);
            placeBox.gameObject.SetActive(true);
        });

        clickedSlot.AsObservable().TakeUntilDestroy(battleManager).Subscribe(slot =>
        {
            switch (clickSlotReason)
            {
                case ClickSlotReason.none:
                    break;
                case ClickSlotReason.placeCharacter:
                    PlaceCharacter(slot);
                    break;
                case ClickSlotReason.viewCharacter:
                    ShowMovePath(slot);
                    break;
                case ClickSlotReason.move:
                    ItemMove(slot);
                    break;
                case ClickSlotReason.selectTarget:
                    TargetSelected(slot);
                    break;
            }
        });
    }

    public void Init()
    {
        currentPlaceIndex.OnNext(-1);
        chessBoard.ResetColors();
        uiBattleItemInfo.Setup(null);
        uiBattleBag.Setup(null);
        chessBoard.ResetMiddle(true, false);
        clickSlotReason = ClickSlotReason.none;
        lastSelectedPos = Vector2.positiveInfinity;
    }

    public void PlaceCharacter(UIChessboardSlot slot)
    {
        if (battleManager.battleInfo.initPlaceSlots.Contains(slot.position))
        {
            if (!battleManager.battleItemManager.HasBattleItem(slot))
            {
                //放置成功
                PlaceBattleItem(battleManager.battleItemManager.PlayerItemIDs[currentPlaceIndex.Value], slot);
                currentPlaceIndex.OnNext(currentPlaceIndex.Value + 1);
            }
            else
            {
                //不做处理
            }
        }
        else
        {
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateGeneralTip("0005");
        }
    }

    public void PlaceBattleItem(string uuid, UIChessboardSlot slot)
    {
        GameObject temp = UnityEngine.Object.Instantiate(battleItemPrefab, this.battleItemFather);
        UIBattleItem battleItem = temp.GetComponent<UIBattleItem>();
        battleItem.Setup(uuid);
        temp.transform.position = slot.transform.position;
        battleManager.battleItemManager.pos_uibattleItemDic.Add(slot.position, battleItem);
        battleManager.battleItemManager.id_posDic.Add(uuid, slot.position);
    }

    public void ShowMovePath(UIChessboardSlot chessboardSlot)
    {
        if (battleManager.battleItemManager.HasBattleItem(chessboardSlot))
        {
            chessBoard.ResetColors();
            Dictionary<Vector2, ChessboardSlotColor> dic = new Dictionary<Vector2, ChessboardSlotColor>();
            foreach (var slot in chessBoard.slots.Values)
            {
                if (GameUtil.Instance.CanMoveTo(chessboardSlot.position, slot.position,
                    GlobalAccess.GetBattleItem(battleManager.battleItemManager.pos_uibattleItemDic[chessboardSlot.position].itemID).attributes.Mobility))
                {
                    dic.Add(slot.position, ChessboardSlotColor.green);
                }
            }
            chessBoard.SetColors(dic);
            clickSlotReason = ClickSlotReason.move;
            SelectItem(chessboardSlot.position);
        }
    }

    public void ItemMove(UIChessboardSlot slot)
    {
        UIChessboardSlot lastClickedSlot = chessBoard.slots.GetValueOrDefault(lastSelectedPos);
        string itemID = battleManager.battleItemManager.pos_uibattleItemDic[lastSelectedPos].itemID;
        if (battleManager.battleItemManager.HasBattleItem(slot))
        {
            // 点击了被其他BattleItem占用的地点
            clickSlotReason = ClickSlotReason.viewCharacter;
            clickedSlot.OnNext(slot);
        }
        else if (lastClickedSlot != null && itemID ==
            GlobalAccess.GetBattleItem(battleManager.battleItemManager.battleItemIDs[0]).uuid &&
                battleManager.battleItemManager.HasBattleItem(lastClickedSlot))
        {
            var battleItem = GlobalAccess.GetBattleItem(itemID);
            var result = BattleCommonMethods.CanMoveTo(lastSelectedPos, slot.position,
            battleItem.attributes.Mobility, battleManager.battleItemManager.pos_uibattleItemDic.Keys.ToList());
            if (!battleItem.isConfine && battleItem.attributes.currentEnergy > 0 && result.Item1)
            {
                // 移动成功
                Debug.Log("move success to :" + slot.position);
                battleManager.battleItemManager.pos_uibattleItemDic.Add(slot.position, battleManager.battleItemManager.pos_uibattleItemDic[lastSelectedPos]);
                battleManager.battleItemManager.pos_uibattleItemDic.Remove(lastSelectedPos);
                battleManager.battleItemManager.id_posDic.Remove(itemID);
                battleManager.battleItemManager.id_posDic.Add(itemID, slot.position);
                
                //battleItemDic[slot.position].transform.position = slot.transform.position;
                BattleCommonMethods.MoveAlongPath(result.Item2.Select(pos => chessBoard.slots[pos].transform.position).ToList(),
                    battleManager.battleItemManager.pos_uibattleItemDic[slot.position].transform);
                ShowMovePath(slot);
                SelectItem(slot.position);
                battleItem.attributes.currentEnergy -= 1;
                GlobalAccess.SaveBattleItem(battleItem);
                battleItem.moveSubject.OnNext(slot.position);
            }
            else
            {
                //移动失败
                Debug.Log("move failure to :" + slot.position);
                chessBoard.ResetColors();
                clickSlotReason = ClickSlotReason.viewCharacter;
                UnselectItem();
            }
        }
        else
        {
            //移动失败
            Debug.Log("move failure to :" + slot.position);
            chessBoard.ResetColors();
            clickSlotReason = ClickSlotReason.viewCharacter;
            UnselectItem();
        }
    }

    public void SelectItem(Vector2 pos)
    {
        if (lastSelectedPos != Vector2.positiveInfinity)
        {
            battleManager.battleItemManager.pos_uibattleItemDic.GetValueOrDefault(lastSelectedPos)?.IfNotNull(a =>
            {
                a.Selected = false;
            });
        }
        battleManager.battleItemManager.pos_uibattleItemDic[pos].Selected = true;
        uiBattleItemInfo.Setup(battleManager.battleItemManager.pos_uibattleItemDic[pos].itemID);
        battleManager.uiBattleBag.Setup(battleManager.battleItemManager.pos_uibattleItemDic[pos].itemID);
        chessBoard.ResetMiddle(false);
        lastSelectedPos = pos;
    }

    public void UnselectItem()
    {
        if (lastSelectedPos != Vector2.positiveInfinity)
        {
            battleManager.battleItemManager.pos_uibattleItemDic.GetValueOrDefault(lastSelectedPos)?.IfNotNull(a =>
            {
                a.Selected = false;
            });
        }
        uiBattleItemInfo.Setup(null);
        battleManager.uiBattleBag.Setup(null);
        chessBoard.ResetMiddle(true);
        lastSelectedPos = Vector2.positiveInfinity;
    }

    private StoreItemModel clickedStoreItem;

    public void SelectTargets(StoreItemModel storeItem)
    {
        //SetColors
        string uuID = GlobalAccess.GetBattleItem(battleManager.battleItemManager.battleItemIDs[0]).uuid;
        try
        {
            Vector2 vect = battleManager.battleItemManager.pos_uibattleItemDic.First(x => x.Value.itemID == uuID).Key;
            List<Vector2> vectList = GameUtil.Instance.GetTargetRangeList(vect, storeItem.equipDefine.targetRange);
            chessBoard.ResetColors();
            Dictionary<Vector2, ChessboardSlotColor> dic = new Dictionary<Vector2, ChessboardSlotColor>();
            foreach (var vector in vectList)
            {
                dic.Add(vector, ChessboardSlotColor.red);
            }
            chessBoard.SetColors(dic);
        }
        catch (InvalidOperationException e)
        {
            Debug.Log("MoveBarItemClicked InvalidOperationException: " + e.Message);
        }
        clickedStoreItem = storeItem;
        clickSlotReason = ClickSlotReason.selectTarget;
    }

    public void TargetSelected(UIChessboardSlot slot)
    {
        clickSlotReason = ClickSlotReason.viewCharacter;
        Vector2 vect = battleManager.battleItemManager.pos_uibattleItemDic.First(x =>
            x.Value.itemID == GlobalAccess.GetBattleItem(battleManager.battleItemManager.battleItemIDs[0]).uuid).Key;
        if (GameUtil.Instance.GetTargetRangeList(vect, clickedStoreItem.equipDefine.targetRange).Contains(slot.position))
        {
            //todo temp one target
            if (battleManager.battleItemManager.pos_uibattleItemDic.Keys.Contains(slot.position))
            {
                ItemUseManager.Instance.targetIDs = new List<string>() { battleManager.battleItemManager.pos_uibattleItemDic[slot.position].itemID };
                clickSlotReason = ClickSlotReason.move;
                ShowMovePath(chessBoard.slots[vect]);
            }
            else
            {
                ItemUseManager.Instance.targetIDs = new List<string>();
                clickSlotReason = ClickSlotReason.move;
                ShowMovePath(chessBoard.slots[vect]);
            }
        }
        else
        {
            ItemUseManager.Instance.targetIDs = new List<string>();
            clickSlotReason = ClickSlotReason.move;
            ShowMovePath(chessBoard.slots[vect]);
        }
    }
}
