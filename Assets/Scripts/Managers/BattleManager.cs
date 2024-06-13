using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using Unity.VisualScripting;
using static UnityEditor.Progress;
using UnityEditor;

public enum RoundTime
{
    begin = 0,
    acting = 1,
    end = 2,
    prepare = 3,
}

public enum ClickSlotReason
{
    none = 0,
    placeCharacter = 1,
    viewCharacter = 2,
    move = 3,
    selectTarget = 4,
}

public enum AttackStatus
{
    normal = 1,
    miss = 2,
    toDeath = 3,
}

public class BattleManager : MonoSingleton<BattleManager>
{
    public float difficultyExtraFactor = 0f;
    public UIMoveBar moveBar;
    public UIChessboard chessBoard;
    public GameObject battleItemPrefab;
    public Transform battleItemFather;
    public UICharacterPlaceBox placeBox;
    public UIBattleItemInfo uiBattleItemInfo;
    public UIBattleBag uiBattleBag;
    public TownBattleInfoModel battleInfo;

    public BehaviorSubject<float> timePass = new BehaviorSubject<float>(-1);
    public System.IDisposable timePassDispose;
    public List<string> battleItemIDs = new List<string>();
    public List<string> PlayerItemIDs = new List<string>();
    public BehaviorSubject<RoundTime> roundTime = new BehaviorSubject<RoundTime>(RoundTime.prepare);
    public System.IDisposable roundRelayDispose;

    private BehaviorSubject<int> currentPlaceIndex = new BehaviorSubject<int>(-1);

    public Subject<UIChessboardSlot> clickedSlot = new Subject<UIChessboardSlot>();

    public Dictionary<Vector2, UIBattleItem> battleItemDic = new Dictionary<Vector2, UIBattleItem>();

    private ClickSlotReason clickSlotReason = ClickSlotReason.none;

    // Use this for initialization
    void Start()
    {
        moveBar.Init();
        MapManager.Instance.battleResultSubject.AsObservable().TakeUntilDestroy(this).Subscribe(result =>
        {
            if (result)
            {
                difficultyExtraFactor += 0.2f;
            }
        });

        currentPlaceIndex.AsObservable().TakeUntilDestroy(this).Subscribe(index =>
        {
            if (index < 0) { return; }
            if (index >= PlayerItemIDs.Count)
            {
                //放置完成，回合正式开始
                //需要手动把placebox隐藏
                placeBox.gameObject.SetActive(false);
                chessBoard.ResetColors();
                clickSlotReason = ClickSlotReason.viewCharacter;
                roundTime.OnNext(RoundTime.end);
                currentPlaceIndex.OnNext(-1);
                return;
            }
            placeBox.Setup(GlobalAccess.GetBattleItem(PlayerItemIDs[currentPlaceIndex.Value]).Resource);
        });
        clickedSlot.AsObservable().TakeUntilDestroy(this).Subscribe(slot =>
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

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.currentPageType == PageType.battle)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (currentPlaceIndex.Value != -1)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
                bool flag = false;
                // 处理每个射线碰撞到的对象
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null && hit.collider.CompareTag("ChessBoardSlot"))
                    {
                        //鼠标在棋盘内
                        placeBox.gameObject.SetActive(true);
                        // 获取鼠标当前的屏幕坐标位置
                        Vector3 mousePositionScreen = Input.mousePosition;

                        // 将鼠标屏幕坐标位置转换为世界坐标位置
                        Vector3 mousePositionWorld = Camera.main.ScreenToWorldPoint(mousePositionScreen);
                        mousePositionWorld.z = 0f; // 将 z 轴位置设为 0，使其与 2D 平面对齐

                        // 将自身 GameObject 的位置设置为鼠标的世界坐标位置
                        placeBox.transform.position = mousePositionWorld;

                        flag = true;
                        break;
                    }
                }
                if (!flag)
                { 
                    placeBox.gameObject.SetActive(false);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
                bool flag = false;
                UIChessboardSlot slot = null;
                // 处理每个射线碰撞到的对象
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null && hit.collider.CompareTag("ChessBoardSlot"))
                    {
                        flag = true;
                        slot = hit.collider.GetComponent<UIChessboardSlot>();
                    }
                    if (hit.collider != null && hit.collider.CompareTag("TopUI"))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag && slot != null)
                {
                    clickedSlot.OnNext(slot);
                }
            }
        }
    }

    ~BattleManager()
    {
        timePassDispose.IfNotNull(dispose => { dispose.Dispose(); });
        roundRelayDispose.IfNotNull(dispose => { dispose.Dispose(); });
        timePassDispose = null;
        roundRelayDispose = null;
    }

    private void BattleInit()
    {
        timePassDispose.IfNotNull(dispose => { dispose.Dispose(); });
        roundRelayDispose.IfNotNull(dispose => { dispose.Dispose(); });
        timePassDispose = null;
        roundRelayDispose = null;

        timePass.OnNext(-1);
        roundTime.OnNext(RoundTime.prepare);
        currentPlaceIndex.OnNext(-1);

        uiBattleItemInfo.Setup(null);
        uiBattleBag.Setup(null);
        battleItemIDs.Clear();
        PlayerItemIDs.Clear();
        GameUtil.Instance.DetachChildren(battleItemFather);
        battleItemDic.Clear();
        chessBoard.ResetColors();
    }

    public void StartBattle(List<string> characterIDs, TownBattleInfoModel battleInfo)
    {
        BattleInit();
        this.battleInfo = battleInfo;
        var timeItem = new BattleItem(BattleItemType.time);
        GlobalAccess.SaveBattleItem(timeItem);
        battleItemIDs.Add(timeItem.uuid);

        for (int i = 0; i < characterIDs.Count; i++)
        {
            var battleItem = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(characterIDs[i])).Value.ToBattleItem();
            GlobalAccess.SaveBattleItem(battleItem);
            battleItemIDs.Add(battleItem.uuid);
            PlayerItemIDs.Add(battleItem.uuid);
        }

        foreach (var pair in battleInfo.enermys)
        {
            var battleItem = pair.Value.ToBattleItem(difficultyExtraFactor + battleInfo.battleBaseDifficulty);
            GlobalAccess.SaveBattleItem(battleItem);
            battleItemIDs.Add(battleItem.uuid);
            PlaceBattleItem(battleItem.uuid, chessBoard.slots[pair.Key]);
        }

        timePassDispose = timePass.AsObservable().TakeUntilDestroy(this).Subscribe(time =>
        {
            CalcBattleItemAndShow(time);
        });
        //手动执行一次排序和显示(-999是特殊指令值)
        timePass.OnNext(-999);
        //正式开始回合流程
        roundRelayDispose = roundTime.AsObservable().TakeUntilDestroy(this).Subscribe(time =>
        {
            StartCoroutine(ProcessRound(time));
        });
    }

    public IEnumerator ProcessRound(RoundTime time)
    {
        var battleItem0 = GlobalAccess.GetBattleItem(battleItemIDs[0]);
        var battleItem1 = GlobalAccess.GetBattleItem(battleItemIDs[1]);
        switch (time)
        {
            case RoundTime.begin:
                Debug.Log("round begin");
                switch (battleItem0.battleItemType)
                {
                    case BattleItemType.player:
                        battleItemDic.FirstOrDefault(pair => {
                            return pair.Value.itemID == battleItem0.uuid;
                        }).IfNotNull(pair =>
                        {
                            pair.Value.roundActive = true;
                            // auto viewCharacter
                            clickSlotReason = ClickSlotReason.viewCharacter;
                            clickedSlot.OnNext(chessBoard.slots[pair.Key]);
                        });
                        break;
                    case BattleItemType.enemy:
                        battleItemDic.FirstOrDefault(pair => {
                            return pair.Value.itemID == battleItem0.uuid;
                        }).IfNotNull(pair =>
                        {
                            pair.Value.roundActive = true;
                        });
                        break;
                    case BattleItemType.time:
                        GameManager.Instance.TimeChanged(-1);
                        yield return new WaitForSeconds(1f);
                        roundTime.OnNext(RoundTime.end);
                        break;
                    case BattleItemType.sceneItem:
                        battleItemDic.FirstOrDefault(pair => {
                            return pair.Value.itemID == battleItem0.uuid;
                        }).IfNotNull(pair =>
                        {
                            pair.Value.roundActive = true;
                        });
                        break;
                }
                
                yield return new WaitForSeconds(1f);
                roundTime.OnNext(RoundTime.acting);
                break;
            case RoundTime.acting:
                Debug.Log("round acting");
                switch (battleItem0.battleItemType)
                {
                    case BattleItemType.player:
                        break;
                    case BattleItemType.enemy:
                        StartCoroutine(battleItem0.enemyAI.TurnAction(battleItem0.uuid));
                        break;
                    case BattleItemType.time:
                        GameManager.Instance.TimeChanged(-1);
                        yield return new WaitForSeconds(1f);
                        roundTime.OnNext(RoundTime.end);
                        break;
                    case BattleItemType.sceneItem:
                        Debug.Log("sceneItem round acting");
                        yield return new WaitForSeconds(1f);
                        roundTime.OnNext(RoundTime.end);
                        break;
                }
                break;
            case RoundTime.end:
                Debug.Log("round end");
                switch (battleItem0.battleItemType)
                {
                    case BattleItemType.player:
                    case BattleItemType.enemy:
                    case BattleItemType.sceneItem:
                        battleItemDic.Values.First(item => { return item.itemID == battleItem0.uuid; }).roundActive = false;
                        break;
                    case BattleItemType.time:
                        break;
                }
                
                int temp = Mathf.CeilToInt(GlobalAccess.roundDistance / battleItem0.Speed);
                float passedTime = battleItem1.remainActingDistance / battleItem1.Speed;
                battleItem0.remainActingDistance = GlobalAccess.roundDistance + passedTime * battleItem0.Speed; // 因为后续还会timePass一次
                timePass.OnNext(passedTime);
                yield return new WaitForSeconds(1f);
                roundTime.OnNext(RoundTime.begin);
                break;
            case RoundTime.prepare:
                //开始放置角色
                Dictionary<Vector2, ChessboardSlotColor> dic = battleInfo.initPlaceSlots.ToDictionary(vec => vec, vec => ChessboardSlotColor.yellow);
                chessBoard.SetColors(dic);
                currentPlaceIndex.OnNext(0);
                clickSlotReason = ClickSlotReason.placeCharacter;
                break;
        }
        yield return null;
    }

    public void CalcBattleItemAndShow(float time)
    {
        if (time >= 0)
        {
            foreach (string uuid in battleItemIDs)
            {
                var item = GlobalAccess.GetBattleItem(uuid);
                item.remainActingDistance = Mathf.Max(0, item.remainActingDistance - time * item.Speed);
                GlobalAccess.SaveBattleItem(item);
            }
            ResortBattleItems();
            moveBar.Show(battleItemIDs);
        }
        else if (time == -999)
        {
            foreach (string uuid in battleItemIDs)
            {
                var item = GlobalAccess.GetBattleItem(uuid);
                item.remainActingDistance = GlobalAccess.roundDistance;
                GlobalAccess.SaveBattleItem(item);
            }
            ResortBattleItems();
            moveBar.Show(battleItemIDs);
        }
    }

    public void ResortBattleItems()
    {
        var tempBattleItems = battleItemIDs.Select(uuid => GlobalAccess.GetBattleItem(uuid)).ToList();
        tempBattleItems.Sort((itemA, itemB) =>
        {
            return Mathf.CeilToInt(itemA.remainActingDistance / itemA.Speed)
                .CompareTo(Mathf.CeilToInt(itemB.remainActingDistance / itemB.Speed));
        });
        battleItemIDs = tempBattleItems.Select(item => item.uuid).ToList();
    }

    public void RoundEnd()
    {
        roundTime.OnNext(RoundTime.end);
    }

    public void PlaceCharacter(UIChessboardSlot slot)
    {
        if (battleInfo.initPlaceSlots.Contains(slot.position))
        {
            if (!HasBattleItem(slot))
            {
                //放置成功
                PlaceBattleItem(PlayerItemIDs[currentPlaceIndex.Value], slot);
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
            tip.UpdateTip(DataManager.Instance.Language["general_error_tip"] + "0005");
        }
    }

    private void PlaceBattleItem(string uuid, UIChessboardSlot slot)
    {
        GameObject temp = Instantiate(battleItemPrefab, this.battleItemFather);
        UIBattleItem battleItem = temp.GetComponent<UIBattleItem>();
        battleItem.Setup(uuid);
        temp.transform.position = slot.transform.position;
        battleItemDic.Add(slot.position, battleItem);
    }

    public void ShowMovePath(UIChessboardSlot chessboardSlot)
    {
        if (HasBattleItem(chessboardSlot))
        {
            chessBoard.ResetColors();
            Dictionary<Vector2, ChessboardSlotColor> dic = new Dictionary<Vector2, ChessboardSlotColor>();
            foreach (var slot in chessBoard.slots.Values)
            {
                if (GameUtil.Instance.CanMoveTo(chessboardSlot.position, slot.position,
                    GlobalAccess.GetBattleItem(battleItemDic[chessboardSlot.position].itemID).Mobility))
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
        if (HasBattleItem(slot))
        {
            clickSlotReason = ClickSlotReason.viewCharacter;
            clickedSlot.OnNext(slot);
        } else if (lastClickedSlot != null && battleItemDic[lastSelectedPos].itemID == GlobalAccess.GetBattleItem(battleItemIDs[0]).uuid && HasBattleItem(lastClickedSlot) &&
            GameUtil.Instance.CanMoveTo(lastSelectedPos, slot.position,
            GlobalAccess.GetBattleItem(battleItemDic[lastSelectedPos].itemID).Mobility))
        {
            Debug.Log("move success to :" + slot.position);
            battleItemDic.Add(slot.position, battleItemDic[lastSelectedPos]);
            battleItemDic.Remove(lastSelectedPos);
            battleItemDic[slot.position].transform.position = slot.transform.position;
            ShowMovePath(slot);
            SelectItem(slot.position);
        }
        else
        {
            Debug.Log("move failure to :" + slot.position);
            chessBoard.ResetColors();
            clickSlotReason = ClickSlotReason.viewCharacter;
            UnselectItem();
        }
    }

    public bool HasBattleItem(UIChessboardSlot slot)
    {
        return battleItemDic.ContainsKey(slot.position);
    }

    public void MoveBarItemClicked(Button button)
    {
        BattleItem battleItem = null;
        if (button.GetComponent<UIMoveBarFirstItem>() != null)
        {
            battleItem = button.GetComponent<UIMoveBarFirstItem>().item;
        }
        else if (button.GetComponent<UIMoveBarOtherItem>() != null)
        {
            battleItem = button.GetComponent<UIMoveBarOtherItem>().item;
        }

        if (battleItem != null)
        {
            clickSlotReason = ClickSlotReason.viewCharacter;
            string uuID = battleItem.uuid;
            try
            {
                Vector2 vect = battleItemDic.First(x => x.Value.itemID == uuID).Key;
                clickedSlot.OnNext(chessBoard.slots[vect]);
            }
            catch (InvalidOperationException e)
            {
                Debug.Log("MoveBarItemClicked InvalidOperationException: " + e.Message);
            }
        }
    }

    private Vector2 lastSelectedPos = Vector2.positiveInfinity;
    public void SelectItem(Vector2 pos)
    {
        if (lastSelectedPos != Vector2.positiveInfinity)
        {
            battleItemDic.GetValueOrDefault(lastSelectedPos)?.IfNotNull(a =>
            {
                a.Selected = false;
            });
        }
        battleItemDic[pos].Selected = true;
        uiBattleItemInfo.Setup(battleItemDic[pos].itemID);
        uiBattleBag.Setup(battleItemDic[pos].itemID);
        chessBoard.ResetMiddle(false);
        lastSelectedPos = pos;
    }

    public void UnselectItem()
    {
        if (lastSelectedPos != Vector2.positiveInfinity)
        {
            battleItemDic.GetValueOrDefault(lastSelectedPos)?.IfNotNull(a =>
            {
                a.Selected = false;
            });
        }
        uiBattleItemInfo.Setup(null);
        uiBattleBag.Setup(null);
        chessBoard.ResetMiddle(true);
        lastSelectedPos = Vector2.positiveInfinity;
    }

    public void EquipClicked(string uuid, UIEquipItem equipItem) {
        var battleItem0 = GlobalAccess.GetBattleItem(battleItemIDs[0]);
        if (uuid == battleItem0.uuid && battleItem0.battleItemType == BattleItemType.player)
        {
            //只处理正在行动的玩家角色的装备点击
            EquipManager.Instance.Use(uuid, equipItem.storeItem, false);
        }
    }

    private StoreItemModel clickedStoreItem;
    public void SelectTargets(StoreItemModel storeItem)
    {
        //SetColors
        string uuID = GlobalAccess.GetBattleItem(battleItemIDs[0]).uuid;
        try
        {
            Vector2 vect = battleItemDic.First(x => x.Value.itemID == uuID).Key;
            List<Vector2> vectList = storeItem.GetTargetRangeList(vect);
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
        Vector2 vect = battleItemDic.First(x => x.Value.itemID == GlobalAccess.GetBattleItem(battleItemIDs[0]).uuid).Key;
        if (clickedStoreItem.GetTargetRangeList(vect).Contains(slot.position))
        {
            //todo temp one target
            if (battleItemDic.Keys.Contains(slot.position))
            {
                EquipManager.Instance.targetIDs = new List<string>() { battleItemDic[slot.position].itemID };
                clickSlotReason = ClickSlotReason.move;
                ShowMovePath(chessBoard.slots[vect]);
            }
            else
            {
                EquipManager.Instance.targetIDs = new List<string>();
                clickSlotReason = ClickSlotReason.move;
                ShowMovePath(chessBoard.slots[vect]);
            }
        } else
        {
            EquipManager.Instance.targetIDs = new List<string>();
            clickSlotReason = ClickSlotReason.move;
            ShowMovePath(chessBoard.slots[vect]);
        }
    }

    public void ShakeEnergy()
    {
        uiBattleItemInfo.ShakeEnergy();
    }

    public void BlinkEnergy()
    {
        uiBattleItemInfo.BlinkEnergy();
    }

    public AttackStatus ProcessAttack(string selfID, List<string> targetIDs, int value)
    {
        var self = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(selfID)).Value;
        List<AttackStatus> statuses = new List<AttackStatus>();
        foreach (var id in targetIDs)
        {
            var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(id)).Value;
            bool hitFlag = UnityEngine.Random.Range(0, 100) < (100 - target.Dodge + self.Accuracy);
            bool criticalFlag = UnityEngine.Random.Range(0, 100) < self.Lucky;
            if (!hitFlag)
            {
                statuses.Add(AttackStatus.miss);
            }
            int damage = (int)(value
                * (1 + self.Strength / 100.0f)
                * (1 - target.Defense / (target.Defense + 100.0f))
                * (hitFlag ? 1 : 0)
                * (criticalFlag ? 1 : 2));
            var targetItem = battleItemDic.Values.Where((item) => item.itemID == id).ToList().FirstOrDefault();
            if (targetItem != null)
            {
                statuses.Add(targetItem.Damage(damage, criticalFlag));
            } else
            {
                statuses.Add(AttackStatus.normal);
            }
        }
        if(statuses.Contains(AttackStatus.toDeath))
        {
            return AttackStatus.toDeath;
        } else {
            foreach(var status in statuses)
            {
                switch (status)
                {
                    case AttackStatus.normal:
                        return AttackStatus.normal;
                    case AttackStatus.miss:
                        continue;
                    case AttackStatus.toDeath:
                        return AttackStatus.toDeath;
                }
            }
            return AttackStatus.miss;
        }
    }

    public void ProcessHealth(string selfID, List<string> targetIDs, int value)
    {
        var self = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(selfID)).Value;
        List<AttackStatus> statuses = new List<AttackStatus>();
        foreach (var id in targetIDs)
        {
            var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(id)).Value;
            bool criticalFlag = UnityEngine.Random.Range(0, 100) < self.Lucky;
            int healthHp = (int)(value
                * (1 + self.Strength / 100.0f)
                * (criticalFlag ? 1 : 2));
            var targetItem = battleItemDic.Values.Where((item) => item.itemID == id).ToList().FirstOrDefault();
            if (targetItem != null)
            {
                targetItem.AddHP(healthHp, criticalFlag);
            }
        }
    }

    public void CharacterDie(string uuid)
    {
        battleItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
            });
        PlayerItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
            });
        var pairs = battleItemDic.Where(item => item.Value.itemID == uuid);
        foreach(var pair in pairs)
        {
            Destroy(pair.Value.gameObject);
            battleItemDic.Remove(pair.Key);
        }
    }
}