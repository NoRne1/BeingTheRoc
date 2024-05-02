using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System.Linq;
using UnityEngine.TextCore.Text;
using UnityEngine.EventSystems;
using System;

public enum RoundTime
{
    begin = 0,
    acting = 1,
    end = 2,
}

public class BattleManager : MonoSingleton<BattleManager>
{
    public UIMoveBar moveBar;
    public UIChessboard chessBoard;
    public BehaviorSubject<int> timePass = new BehaviorSubject<int>(-1);
    public System.IDisposable? timePassDispose;
    public List<NorneRelay<CharacterModel>> characters = new List<NorneRelay<CharacterModel>>();
    public BehaviorSubject<RoundTime> roundTime = new BehaviorSubject<RoundTime>(RoundTime.end);
    public System.IDisposable? roundRelayDispose;

    public UICharacterPlaceBox placeBox;

    private BehaviorSubject<int> currentPlaceIndex = new BehaviorSubject<int>(-1);

    public Subject<UIChessboardSlot> clickedSlot = new Subject<UIChessboardSlot>();

    public TownBattleInfoModel battleInfo;

    // Use this for initialization
    void Start()
    {
        moveBar.Init();
        currentPlaceIndex.AsObservable().TakeUntilDestroy(this).Subscribe(index =>
        {
            if (index == -1) { return; }
            if (index >= characters.Count || index < 0)
            {
                currentPlaceIndex.OnNext(-1);
                return;
            }
            placeBox.Setup(characters[currentPlaceIndex.Value].Value.Resource);
        });
        clickedSlot.AsObservable().TakeUntilDestroy(this).Subscribe(slot =>
        {
            if (currentPlaceIndex.Value != -1 && battleInfo.initPlaceSlots.Contains(slot.postion))
            {
                //放置成功
                Debug.Log("Character Place Success! " + characters[currentPlaceIndex.Value].Value.Name + ": " + slot.postion);
                currentPlaceIndex.OnNext(currentPlaceIndex.Value + 1);
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
        characters.Clear();

        this.battleInfo = battleInfo;

        for (int i = 0; i < characterIDs.Count; i++)
        {
            characters.Add(
                NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(characterIDs[i]))
            );
        }

        //开始放置角色
        currentPlaceIndex.OnNext(0);

        timePassDispose = timePass.AsObservable().TakeUntilDestroy(this).Subscribe(time => {
            CalcCharacterAndShow(time);
        });
        //手动执行一次排序和显示(-999手是特殊指令值)
        timePass.OnNext(-999);
        //正式开始回合流程
        roundRelayDispose = roundTime.AsObservable().TakeUntilDestroy(this).Subscribe(time => {
            StartCoroutine(ProcessRound(time));
        });
    }

    public IEnumerator ProcessRound(RoundTime time)
    {
        switch(time)
        {
            case RoundTime.begin:
                Debug.Log("round begin");
                yield return new WaitForSeconds(1f);
                roundTime.OnNext(RoundTime.acting);
                break;
            case RoundTime.acting:
                Debug.Log("round acting");
                break;
            case RoundTime.end:
                Debug.Log("round end");
                int temp = Mathf.CeilToInt(GlobalAccess.roundDistance / characters[0].Value.Speed);
                characters[0].Value.remainActingTime = temp + characters[1].Value.remainActingTime; // 因为后续还会timePass一次
                timePass.OnNext(characters[1].Value.remainActingTime);
                yield return new WaitForSeconds(1f);
                roundTime.OnNext(RoundTime.begin);
                break;
        }
        yield return null;
    }
    public void CalcCharacterAndShow(int time)
    {
        if(time >= 0)
        {
            foreach (NorneRelay<CharacterModel> character in characters)
            {
                character.Value.remainActingTime -= time;
            }
            ResortCharacter();
            moveBar.Show(characters.Select(c => (BattleItem)c.Value).ToList());
        } else if (time == -999)
        {
            foreach (NorneRelay<CharacterModel> character in characters)
            {
                character.Value.remainActingTime = Mathf.CeilToInt(GlobalAccess.roundDistance / character.Value.Speed);
            }
            ResortCharacter();
            moveBar.Show(characters.Select(c => (BattleItem)c.Value).ToList());
        }
    }

    public void ResortCharacter()
    {
        characters.Sort((a, b) => a.Value.remainActingTime.CompareTo(b.Value.remainActingTime));
    }

    public void RoundEnd()
    {
        roundTime.OnNext(RoundTime.end);
    }
}
