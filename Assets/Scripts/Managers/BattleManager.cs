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

public class BattleManager : MonoSingleton<BattleManager>
{
    public BattleItemManager battleItemManager = new BattleItemManager();
    public BattleRoundManager roundManager = new BattleRoundManager();
    public BattleMoveBarManager moveBarManager;
    public BattleChessboardManager chessboardManager;

    // UI
    public UIMoveBar moveBar;
    public UIChessboard chessBoard;
    public UICharacterPlaceBox placeBox;
    public GameObject battleItemPrefab;
    public Transform battleItemFather;
    public UIBattleItemInfo uiBattleItemInfo;
    public UIBattleBag uiBattleBag;
    public TownBattleInfoModel battleInfo;
    public UICircleProgressButton endRoundButton;
    public UICircleProgressButton restartTodayButton;
    public UICircleProgressButton quitBattleButton;
    public bool isInBattle = false;
    public int battleStartTimeLeft = -1;
    // Data
    public float difficultyExtraFactor = 0f;
    public bool isWaitQuit = false;

    // Subject
    // (string, string, int, AttackStatus, bool, bool) => (casterID, targetID, damage, attackStatus, isCritical, trigger other effect)
    public Subject<AttackDisplayResult> battleItemDamageSubject = new Subject<AttackDisplayResult>();

    // Use this for initialization
    void Start()
    { 
        moveBarManager = new BattleMoveBarManager(moveBar);
        chessboardManager = new BattleChessboardManager(chessBoard, placeBox, battleItemPrefab, battleItemFather, uiBattleItemInfo, uiBattleBag);
        MapManager.Instance.battleResultSubject.AsObservable().TakeUntilDestroy(this).Subscribe(result =>
        {
            if (result)
            {
                difficultyExtraFactor += 0.2f;
            }
        });
        
        endRoundButton.buttonCheck = () => { 
            var currentBattleItem = GlobalAccess.GetBattleItem(battleItemManager.roundBattleItemIDs[0]);
            return currentBattleItem.type == BattleItemType.player;
        };
        endRoundButton.progressCheck = () => { 
            var currentBattleItem = GlobalAccess.GetBattleItem(battleItemManager.roundBattleItemIDs[0]);
            return (currentBattleItem.type == BattleItemType.player && currentBattleItem.attributes.currentEnergy > 0);
        };
        endRoundButton.onProgressCompleteAction = () => {
            RoundEnd();
        };
        endRoundButton.onImmediateAction = () => {
            RoundEnd();
        };
        //restartTodayButton不存在onImmediateAction
        restartTodayButton.onProgressCompleteAction = () => {
            Debug.Log("restartTodayButton.onProgressCompleteAction");
        };

        quitBattleButton.onProgressCompleteAction = () => {
            QuitBattleButtonClick();
        };

        GameManager.Instance.timeLeft.DistinctUntilChanged().Subscribe(timeleft => {
            if (isInBattle && battleStartTimeLeft != -1 && 
                (battleStartTimeLeft - timeleft) / 3 != 0 && (battleStartTimeLeft - timeleft) % 3 == 0)
            {
                //战斗中，距离战斗开始每过一天触发一次
                EnermySupport();
            }
        }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.currentPageType == PageType.battle)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (chessboardManager.currentPlaceIndex.Value != -1)
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
                        mousePositionWorld.z = placeBox.transform.position.z;

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

            if (Input.GetMouseButtonUp(0) && !UIManager.Instance.HasActiveUIWindow())
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
                    chessboardManager.clickedSlot.OnNext(slot);
                }
            }
        }
    }

    ~BattleManager()
    {
        
    }

    private void BattleInit(List<string> characterIDs, TownBattleInfoModel battleInfo)
    {
        GameUtil.Instance.DetachChildren(battleItemFather);
        roundManager.Init();
        moveBarManager.Init();
        chessboardManager.Init();
        battleItemManager.Init(characterIDs, battleInfo);
    }

    public void StartBattle(List<string> characterIDs, TownBattleInfoModel battleInfo)
    {
        isInBattle = true;
        battleStartTimeLeft = GameManager.Instance.timeLeft.Value;
        BattleInit(characterIDs, battleInfo);
        
        this.battleInfo = battleInfo;
        
        roundManager.StartBattle();
        GameManager.Instance.treasureManager.BattleStart();
    }

    public void RoundEnd()
    {
        roundManager.roundTime.OnNext((battleItemManager.roundBattleItemIDs[0], RoundTime.end));
    }

    public void EnermySupport()
    {
        if(battleItemManager.granaryItemID != "")
        {
            var granaryItem = GlobalAccess.GetBattleItem(battleItemManager.granaryItemID);
            if(granaryItem.attributes != null && granaryItem.attributes.currentHP > granaryItem.attributes.MaxHP / 2)
            {
                //粮仓余量大于一半，则触发增援
                var enermyModel = battleInfo.PopSupportEnermy();
                if (enermyModel != null)
                {
                    battleItemManager.AddSupportEnermy(enermyModel);
                }
            }
        }
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
            chessboardManager.clickSlotReason = ClickSlotReason.viewBattleItem;
            string uuID = battleItem.uuid;
            try
            {
                Vector2 vect = battleItemManager.pos_uibattleItemDic.First(x => x.Value.itemID == uuID).Key;
                chessboardManager.clickedSlot.OnNext(chessBoard.slots[vect]);
            }
            catch (InvalidOperationException e)
            {
                Debug.Log("MoveBarItemClicked InvalidOperationException: " + e.Message);
            }
        }
    }

    public void EquipClicked(string uuid, UIEquipItem equipItem) {
        var battleItem0 = GlobalAccess.GetBattleItem(battleItemManager.roundBattleItemIDs[0]);
        if (uuid == battleItem0.uuid && battleItem0.type == BattleItemType.player)
        {
            //只处理正在行动的玩家角色的装备点击
            StartCoroutine(ItemUseManager.Instance.Use(uuid, equipItem.storeItem));
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

    public void QuitBattleButtonClick()
    {
        if (!isWaitQuit)
        {
            isWaitQuit = true;
            battleItemManager.AddQuitTimeItem();
        } else {
            BlackBarManager.Instance.AddMessage("催催催，只能跑这么快啦！");
        }
    }

    public void CharacterDie(string uuid)
    {
        //清除数据
        battleItemManager.battleItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
            });
        battleItemManager.playerItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
            });
        battleItemManager.enemyItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
            });
        var pairs = battleItemManager.pos_uibattleItemDic.Where(item => item.Value.itemID == uuid);
        foreach(var pair in pairs)
        {
            Destroy(pair.Value.gameObject);
            battleItemManager.pos_uibattleItemDic.Remove(pair.Key);
        }
        GameManager.Instance.RemoveCharacter(uuid);

        //判断战斗结束
        var diedItem = GlobalAccess.GetBattleItem(uuid);
        if (diedItem.type == BattleItemType.granary) 
        {
            BattleEnd(true);
            return;
        }
        if (battleItemManager.enemyItemIDs.Count == 0) 
        {
            BattleEnd(true);
            return;
        }
        if (battleItemManager.playerItemIDs.Count == 0) 
        {
            BattleEnd(false);
            return;
        }
    }

    //离开战场（非死亡）
    public void CharacterLeave(string uuid)
    {
        battleItemManager.battleItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
        });
        battleItemManager.playerItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
        });
        var pairs = battleItemManager.pos_uibattleItemDic.Where(item => item.Value.itemID == uuid);
        foreach (var pair in pairs)
        {
            Destroy(pair.Value.gameObject);
            battleItemManager.pos_uibattleItemDic.Remove(pair.Key);
        }
    }

    public void BattleEnd(bool result)
    {
        isInBattle = false;
        battleStartTimeLeft = -1;
        SkillManager.Instance.BattleEnd();
        battleItemManager.BattleEnd();
        if (result) 
        {
            MapManager.Instance.BattleSuccess();
        } else 
        {
            MapManager.Instance.BattleFail();
        }
        isWaitQuit = false;
    }
}