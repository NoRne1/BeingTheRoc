using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

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

public class BattleManager : MonoSingleton<BattleManager>
{
    public UIMoveBar moveBar;
    public UIChessboard chessBoard;
    public GameObject battleItemPrefab;
    public Transform battleItemFather;
    public UICharacterPlaceBox placeBox;
    public TownBattleInfoModel battleInfo;

    public BehaviorSubject<int> timePass = new BehaviorSubject<int>(-1);
    public System.IDisposable? timePassDispose;
    public List<BattleItem> battleItems = new List<BattleItem>();
    public BehaviorSubject<RoundTime> roundTime = new BehaviorSubject<RoundTime>(RoundTime.prepare);
    public System.IDisposable? roundRelayDispose;

    private BehaviorSubject<int> currentPlaceIndex = new BehaviorSubject<int>(-1);

    public Subject<UIChessboardSlot> clickedSlot = new Subject<UIChessboardSlot>();

    public Dictionary<Vector2, UIBattleItem> battleItemDic = new Dictionary<Vector2, UIBattleItem>();

    private ClickSlotReason clickSlotReason = ClickSlotReason.none;

    // Use this for initialization
    void Start()
    {
        moveBar.Init();
        currentPlaceIndex.AsObservable().TakeUntilDestroy(this).Subscribe(index =>
        {
            if (index < 0) { return; }
            if (index >= battleItems.Count)
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
            placeBox.Setup(battleItems[currentPlaceIndex.Value].Resource);
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
                    Debug.Log("clickedSlot selectTarget");
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
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
                //在放置角色阶段
                if (hit.collider != null &&
                    (hit.collider.CompareTag("ChessBoard") || hit.collider.CompareTag("ChessBoardSlot")))
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
                }
                else
                {
                    placeBox.gameObject.SetActive(false);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero,
                    Mathf.Infinity, LayerMask.GetMask("ChessBoardSlot"));
                if (hit.collider != null && hit.collider.CompareTag("ChessBoardSlot"))
                {
                    UIChessboardSlot slot = hit.collider.GetComponent<UIChessboardSlot>();
                    clickedSlot.OnNext(slot);
                }
            }
        }
    }

    ~BattleManager()
    {
        timePassDispose?.Dispose();
        roundRelayDispose?.Dispose();
        timePassDispose = null;
        roundRelayDispose = null;
    }

    public void StartBattle(List<int> characterIDs, TownBattleInfoModel battleInfo)
    {
        timePassDispose?.Dispose();
        roundRelayDispose?.Dispose();
        battleItems.Clear();
        chessBoard.ResetColors();

        this.battleInfo = battleInfo;

        for (int i = 0; i < characterIDs.Count; i++)
        {
            battleItems.Add(
                (BattleItem)NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(characterIDs[i])).Value
            );
        }

        timePassDispose = timePass.AsObservable().TakeUntilDestroy(this).Subscribe(time =>
        {
            CalcBattleItemAndShow(time);
        });
        //手动执行一次排序和显示(-999手是特殊指令值)
        timePass.OnNext(-999);
        //正式开始回合流程
        roundRelayDispose = roundTime.AsObservable().TakeUntilDestroy(this).Subscribe(time =>
        {
            StartCoroutine(ProcessRound(time));
        });
    }

    public IEnumerator ProcessRound(RoundTime time)
    {
        switch (time)
        {
            case RoundTime.begin:
                Debug.Log("round begin");
                UIBattleItem item = battleItemDic.Values.FirstOrDefault(item => { return item.item.uuID == battleItems[0].uuID; });
                battleItemDic.Values.FirstOrDefault(item => { return item.item.uuID == battleItems[0].uuID; }).IfNotNull(item =>
                {
                    item.roundActive = true;
                });
                yield return new WaitForSeconds(1f);
                roundTime.OnNext(RoundTime.acting);
                break;
            case RoundTime.acting:
                Debug.Log("round acting");
                break;
            case RoundTime.end:
                Debug.Log("round end");
                battleItemDic.Values.First(item => { return item.item.uuID == battleItems[0].uuID; }).roundActive = false;
                int temp = Mathf.CeilToInt(GlobalAccess.roundDistance / battleItems[0].Speed);
                battleItems[0].remainActingTime = temp + battleItems[1].remainActingTime; // 因为后续还会timePass一次
                timePass.OnNext(battleItems[1].remainActingTime);
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

    public void CalcBattleItemAndShow(int time)
    {
        if (time >= 0)
        {
            foreach (BattleItem item in battleItems)
            {
                item.remainActingTime -= time;
            }
            ResortBattleItems();
            moveBar.Show(battleItems);
        }
        else if (time == -999)
        {
            foreach (BattleItem item in battleItems)
            {
                item.remainActingTime = Mathf.CeilToInt(GlobalAccess.roundDistance / item.Speed);
            }
            ResortBattleItems();
            moveBar.Show(battleItems);
        }
    }

    public void ResortBattleItems()
    {
        battleItems.Sort((a, b) => a.remainActingTime.CompareTo(b.remainActingTime));
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
                BattleItem item = battleItems[currentPlaceIndex.Value];
                //放置成功
                Debug.Log("Character Place Success! " + item.Name + ": " + slot.position);
                GameObject temp = Instantiate(battleItemPrefab, this.battleItemFather);
                UIBattleItem battleItem = temp.GetComponent<UIBattleItem>();
                battleItem.Setup(item);
                temp.transform.position = slot.transform.position;
                battleItemDic.Add(slot.position, battleItem);
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

    public void ShowMovePath(UIChessboardSlot chessboardSlot)
    {
        if (HasBattleItem(chessboardSlot))
        {
            chessBoard.ResetColors();
            Dictionary<Vector2, ChessboardSlotColor> dic = new Dictionary<Vector2, ChessboardSlotColor>();
            foreach (var slot in chessBoard.slots.Values)
            {
                if (GameUtil.Instance.CanMoveTo(chessboardSlot.position, slot.position, battleItemDic[chessboardSlot.position].item.Mobility))
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
        if (lastClickedSlot != null && battleItemDic[lastSelectedPos].item.uuID == battleItems[0].uuID && !HasBattleItem(slot) && HasBattleItem(lastClickedSlot) &&
            GameUtil.Instance.CanMoveTo(lastSelectedPos, slot.position, battleItemDic[lastSelectedPos].item.Mobility))
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
            Debug.Log(battleItemDic[lastSelectedPos].item.uuID + " " + battleItems[0].uuID + " " + !HasBattleItem(slot) + " " +
                HasBattleItem(lastClickedSlot) + " " + GameUtil.Instance.CanMoveTo(lastSelectedPos, slot.position, battleItemDic[lastSelectedPos].item.Mobility));
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
            string uuID = battleItem.uuID;
            try
            {
                Vector2 vect = battleItemDic.First(x => x.Value.item.uuID == uuID).Key;
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
        lastSelectedPos = Vector2.positiveInfinity;
    }
}