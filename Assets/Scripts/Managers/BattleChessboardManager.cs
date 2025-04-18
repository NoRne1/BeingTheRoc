using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public enum ClickSlotReason
{
    none = 0,
    placeCharacter = 1,
    viewBattleItem = 2,
    move = 3,
    selectTargets = 4,
    selectPosition = 5,
}

public class BattleChessboardManager
{
    // alias
    private BattleManager battleManager = BattleManager.Instance;
    // ui
    public UIChessboard chessBoard;
    public UICharacterPlaceBox placeBox;
    public GameObject battleItemPrefab;
    public Transform battleItemFather;
    public UIBattleItemInfo uiBattleItemInfo;
    public UIBattleBag uiBattleBag;
    //property
    public Subject<UIChessboardSlot> clickedSlot = new Subject<UIChessboardSlot>();
    public ClickSlotReason clickSlotReason = ClickSlotReason.none;
    //因为点击角色和移动的点击是分开的，所以记录点击时角色的位置
    private Vector2 lastSelectedPos = Vector2.positiveInfinity;
    private bool isExecuting = false;
    //subject
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
            if (index < 0) 
            { 
                return; 
            } else if (index == 0)
            {
                //放置粮仓
                placeBox.Setup(GlobalAccess.GetBattleItem(battleManager.battleItemManager.playerGranaryID).Resource);
                placeBox.gameObject.SetActive(true);
            } else if (index > 0 && index <= battleManager.battleItemManager.playerItemIDs.Count)
            {
                //放置参战角色
                placeBox.Setup(GlobalAccess.GetBattleItem(battleManager.battleItemManager.playerItemIDs[currentPlaceIndex.Value-1]).Resource);
                placeBox.gameObject.SetActive(true);
            } else if (index > battleManager.battleItemManager.playerItemIDs.Count)
            {
                //放置完成，回合正式开始
                //需要手动把placebox隐藏
                placeBox.gameObject.SetActive(false);
                chessBoard.ResetColors();
                clickSlotReason = ClickSlotReason.viewBattleItem;
                battleManager.roundManager.roundTime.OnNext((null, RoundTime.end));
                currentPlaceIndex.OnNext(-1);
            }
        });

        clickedSlot.AsObservable().TakeUntilDestroy(battleManager).Subscribe(slot =>
        {
            if (battleManager.isInBattle)
            {
                switch (clickSlotReason)
                {
                    case ClickSlotReason.none:
                        break;
                    case ClickSlotReason.placeCharacter:
                        PlaceBattleItem(slot);
                        break;
                    case ClickSlotReason.viewBattleItem:
                        if (battleManager.battleItemManager.HasBattleItem(slot))
                        {
                            SelectItem(slot.position);
                        }
                        break;
                    case ClickSlotReason.move:
                        battleManager.StartCoroutine(ClickItemMove(slot));
                        break;
                    case ClickSlotReason.selectTargets:
                        TargetItemsSelected(slot);
                        break;
                    case ClickSlotReason.selectPosition:
                        TargetPositionSelected(slot);
                        break;
                    default:
                        Debug.LogError("clickSlotReason unknown type");
                        break;
                }
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

    public void PlaceBattleItem(UIChessboardSlot slot)
    {
        if (battleManager.battleInfo.initPlaceSlots.Contains(slot.position))
        {
            if (!battleManager.battleItemManager.HasBattleItem(slot))
            {
                if(currentPlaceIndex.Value == 0)
                {
                    //放置成功
                    PlaceBattleItem(battleManager.battleItemManager.playerGranaryID, slot);
                    currentPlaceIndex.OnNext(currentPlaceIndex.Value + 1);
                } else if (currentPlaceIndex.Value > 0 && currentPlaceIndex.Value <= battleManager.battleItemManager.playerItemIDs.Count)
                {
                    //放置成功
                    PlaceBattleItem(battleManager.battleItemManager.playerItemIDs[currentPlaceIndex.Value - 1], slot);
                    currentPlaceIndex.OnNext(currentPlaceIndex.Value + 1);
                }
            }
            else
            {
                //不做处理
            }
        }
        else
        {
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip("place_character_wrong");
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

    public UIChessboardSlot GetRandomAvailableSlot()
    {
        var availableSlotPosList = chessBoard.slots.Keys.Where(pos=>!battleManager.battleItemManager.pos_uibattleItemDic.Keys.Contains(pos)).ToList();
        return chessBoard.slots[availableSlotPosList[UnityEngine.Random.Range(0, availableSlotPosList.Count)]];
    }

    public void ShowMovePath(string uuid)
    {
        Vector2 pos = battleManager.battleItemManager.GetPosByUUid(uuid);
        if (pos != Vector2.negativeInfinity)
        {
            chessBoard.ResetColors();
            Dictionary<Vector2, ChessboardSlotColor> dic = new Dictionary<Vector2, ChessboardSlotColor>();
            foreach (var slot in chessBoard.slots.Values)
            {
                if (GameUtil.Instance.CanMoveTo(pos, slot.position,
                    GlobalAccess.GetBattleItem(uuid).attributes.Mobility))
                {
                    dic.Add(slot.position, ChessboardSlotColor.green);
                }
            }
            chessBoard.SetColors(dic);
            clickSlotReason = ClickSlotReason.move;
        }
    }

    public IEnumerator ClickItemMove(UIChessboardSlot slot)
    {
        UIChessboardSlot lastClickedSlot = chessBoard.slots.GetValueOrDefault(lastSelectedPos);
        string itemID = battleManager.battleItemManager.pos_uibattleItemDic[lastSelectedPos].itemID;
        if (battleManager.battleItemManager.HasBattleItem(slot))
        {
            // 点击了被其他BattleItem占用的地点
            clickSlotReason = ClickSlotReason.viewBattleItem;
            clickedSlot.OnNext(slot);
        }
        else if (lastClickedSlot != null && itemID ==
            GlobalAccess.GetBattleItem(battleManager.battleItemManager.roundBattleItemIDs[0]).uuid &&
                battleManager.battleItemManager.HasBattleItem(lastClickedSlot))
        {
            yield return ItemMove(itemID, slot.position, result => {
                //移动结果
                if (result)
                {
                    lastSelectedPos = slot.position;
                } else
                {
                    Debug.Log("move failure to :" + slot.position);
                    chessBoard.ResetColors();
                    clickSlotReason = ClickSlotReason.viewBattleItem;
                    UnselectItem();
                }
            });
        }
        else
        {
            //移动失败
            Debug.Log("move failure to :" + slot.position);
            chessBoard.ResetColors();
            clickSlotReason = ClickSlotReason.viewBattleItem;
            UnselectItem();
        }
    }

    public IEnumerator ItemMove(string uuid, Vector2 des, Action<bool> resultHandler)
    {
        if (uuid == GlobalAccess.GetBattleItem(battleManager.battleItemManager.roundBattleItemIDs[0]).uuid)
        {
            Vector2 originPos = battleManager.battleItemManager.GetPosByUUid(uuid);
            var battleItem = GlobalAccess.GetBattleItem(uuid);
            var moveResult = BattleCommonMethods.CanMoveTo(originPos, des,
            battleItem.attributes.Mobility, battleManager.battleItemManager.pos_uibattleItemDic.Keys.ToList());
            if (!battleItem.isConfine && battleItem.attributes.currentEnergy > 0 && moveResult.Item1)
            {
                // 移动成功
                Debug.Log("move success to :" + des);
                battleManager.battleItemManager.pos_uibattleItemDic.Add(des, battleManager.battleItemManager.pos_uibattleItemDic[originPos]);
                battleManager.battleItemManager.pos_uibattleItemDic.Remove(originPos);
                battleManager.battleItemManager.id_posDic.Remove(uuid);
                battleManager.battleItemManager.id_posDic.Add(uuid, des);

                battleItem.attributes.currentEnergy -= 1;
                GlobalAccess.SaveBattleItem(battleItem);
                battleItem.moveSubject.OnNext(des);
                yield return battleManager.StartCoroutine(BattleCommonMethods.MoveToPosition(battleManager.battleItemManager.pos_uibattleItemDic[des], 
                    moveResult.Item2));
                resultHandler?.Invoke(true);
                yield return null;
            } else 
            {
                resultHandler?.Invoke(false);
                if (battleItem.isConfine)
                {
                    BlackBarManager.Instance.AddMessage(GameUtil.Instance.GetDisplayString("confine_move_failure"));
                }
                else if (battleItem.attributes.currentEnergy <= 0)
                {
                    BlackBarManager.Instance.AddMessage(GameUtil.Instance.GetDisplayString("energy_move_failure"));
                }
                else
                {
                    BlackBarManager.Instance.AddMessage(GameUtil.Instance.GetDisplayString("wrong_postion_move_failure"));
                }
            }
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
        var itemID = battleManager.battleItemManager.pos_uibattleItemDic[pos].itemID;
        battleManager.battleItemManager.pos_uibattleItemDic[pos].Selected = true;
        uiBattleItemInfo.Setup(itemID);
        battleManager.uiBattleBag.Setup(itemID);
        chessBoard.ResetMiddle(false);
        lastSelectedPos = pos;
        ShowMovePath(itemID);
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

    //用来在使用装备时选择目标
    private StoreItemModel clickedStoreItem;

    public void SelectTargets(StoreItemModel storeItem, ChooseTargetType targetType)
    {
        //SetColors
        string uuID = GlobalAccess.GetBattleItem(battleManager.battleItemManager.roundBattleItemIDs[0]).uuid;
        try
        {
            Vector2 vect = battleManager.battleItemManager.pos_uibattleItemDic.First(x => x.Value.itemID == uuID).Key;
            List<Vector2> vectList = GameUtil.Instance.GetTargetRangeList(vect, storeItem.equipModel.targetRange);
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
        switch (targetType)
        {
            case ChooseTargetType.items:
                clickSlotReason = ClickSlotReason.selectTargets;
                break;
            case ChooseTargetType.position:
                clickSlotReason = ClickSlotReason.selectPosition;
                break;
        }
    }

    private void TargetItemsSelected(UIChessboardSlot slot)
    {
        clickSlotReason = ClickSlotReason.viewBattleItem;
        //找出正在行动的角色的位置
        var actingItemID = battleManager.battleItemManager.roundBattleItemIDs[0];
        Vector2 vect = battleManager.battleItemManager.pos_uibattleItemDic.First(x =>
            x.Value.itemID == actingItemID).Key;
        if (GameUtil.Instance.GetTargetRangeList(vect, clickedStoreItem.equipModel.targetRange).Contains(slot.position))
        {
            //todo temp one target
            if (battleManager.battleItemManager.pos_uibattleItemDic.Keys.Contains(slot.position))
            {
                //选择的位置有角色,符合条件
                ItemUseManager.Instance.targetIDs = new List<string>() { battleManager.battleItemManager.pos_uibattleItemDic[slot.position].itemID };
                clickSlotReason = ClickSlotReason.move;
            }
            else
            {
                //选择的位置没有角色，装备使用被打断
                ItemUseManager.Instance.targetChooseBreakFlag = true;
                clickSlotReason = ClickSlotReason.move;
                ShowMovePath(actingItemID);
            }
        }
        else
        {
            //选择的位置超出装备使用范围，装备使用被打断
            ItemUseManager.Instance.targetChooseBreakFlag = true;
            clickSlotReason = ClickSlotReason.move;
            ShowMovePath(actingItemID);
        }
    }

    private void TargetPositionSelected(UIChessboardSlot slot)
    {
        clickSlotReason = ClickSlotReason.viewBattleItem;
        var actingItemID = battleManager.battleItemManager.roundBattleItemIDs[0];
        //找出正在行动的角色的位置
        Vector2 vect = battleManager.battleItemManager.pos_uibattleItemDic.First(x =>
            x.Value.itemID == actingItemID).Key;
        if (GameUtil.Instance.GetTargetRangeList(vect, clickedStoreItem.equipModel.targetRange).Contains(slot.position))
        {
            if (!battleManager.battleItemManager.pos_uibattleItemDic.Keys.Contains(slot.position))
            {
                //选择的位置没有角色,符合条件
                ItemUseManager.Instance.targetPos = slot.position;
                clickSlotReason = ClickSlotReason.move;
            }
            else
            {
                //选择的位置有item，装备使用被打断
                ItemUseManager.Instance.targetChooseBreakFlag = true;
                clickSlotReason = ClickSlotReason.move;
                ShowMovePath(actingItemID);
            }
        }
        else
        {
            //选择的位置超出装备使用范围，装备使用被打断
            ItemUseManager.Instance.targetChooseBreakFlag = true;
            clickSlotReason = ClickSlotReason.move;
            ShowMovePath(actingItemID);
        }
    }
}
