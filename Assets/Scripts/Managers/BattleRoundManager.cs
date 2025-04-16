using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System.Linq;

public class BattleRoundManager
{
    //只是为了简化BattleManager.Instance
    private BattleManager battleManager;

    public BehaviorSubject<float> timePass = new BehaviorSubject<float>(-1);
    public System.IDisposable timePassDispose;
    public BehaviorSubject<(string, RoundTime)> roundTime = new BehaviorSubject<(string, RoundTime)>((null, RoundTime.prepare));
    public System.IDisposable roundRelayDispose;
    public int extraRound = 0;
    public bool isInExtraRound = false;

    ~BattleRoundManager()
    {
        timePassDispose.IfNotNull(dispose => { dispose.Dispose(); });
        roundRelayDispose.IfNotNull(dispose => { dispose.Dispose(); });
        timePassDispose = null;
        roundRelayDispose = null;
    }

    public void Init()
    {
        //这里会重复赋值，但是没找到其他方法，也影响不大
        battleManager = BattleManager.Instance;
        timePassDispose.IfNotNull(dispose => { dispose.Dispose(); });
        roundRelayDispose.IfNotNull(dispose => { dispose.Dispose(); });
        timePassDispose = null;
        roundRelayDispose = null;
        extraRound = 0;
        isInExtraRound = false;

        timePass.OnNext(-1);
        roundTime.OnNext((null, RoundTime.prepare));
    }

    public void StartBattle()
    {
        timePassDispose = timePass.AsObservable().TakeUntilDestroy(battleManager).Subscribe(time =>
        {
            battleManager.moveBarManager.CalcBattleItemAndShow(time);
        });
        //手动执行一次排序和显示(-999是特殊指令值)
        timePass.OnNext(-999);
        //正式开始回合流程
        roundRelayDispose = roundTime.AsObservable().TakeUntilDestroy(battleManager).Subscribe(time =>
        {
            battleManager.StartCoroutine(ProcessRound(time.Item1, time.Item2));
        });
    }

    public IEnumerator ProcessRound(string uuid, RoundTime time)
    {
        var battleItem0 = GlobalAccess.GetBattleItem(battleManager.battleItemManager.roundBattleItemIDs[0]);
        var battleItem1 = GlobalAccess.GetBattleItem(battleManager.battleItemManager.roundBattleItemIDs[1]);
        switch (time)
        {
            case RoundTime.begin:
                Debug.Log("round begin");
                if (extraRound > 0)
                {
                    extraRound--;
                    isInExtraRound = true;
                }
                battleItem0.RoundBegin();

                switch (battleItem0.type)
                {
                    case BattleItemType.character:
                        if (battleItem0.isPlayer)
                        {
                            battleManager.battleItemManager.pos_uibattleItemDic.FirstOrDefault(pair => {
                                return pair.Value.itemID == battleItem0.uuid;
                            }).IfNotNull(pair =>
                            {
                                pair.Value.roundActive = true;
                                // auto viewCharacter
                                battleManager.chessboardManager.clickSlotReason = ClickSlotReason.viewBattleItem;
                                battleManager.chessboardManager.clickedSlot.OnNext(battleManager.chessBoard.slots[pair.Key]);
                            });
                        } else if (battleItem0.isEnemy)
                        {
                            battleManager.battleItemManager.pos_uibattleItemDic.FirstOrDefault(pair => {
                                return pair.Value.itemID == battleItem0.uuid;
                            }).IfNotNull(pair =>
                            {
                                pair.Value.roundActive = true;
                            });
                        }
                        break;
                    case BattleItemType.time:
                    case BattleItemType.quitTime:
                        break;
                    case BattleItemType.sceneItem:
                        battleManager.battleItemManager.pos_uibattleItemDic.FirstOrDefault(pair => {
                            return pair.Value.itemID == battleItem0.uuid;
                        }).IfNotNull(pair =>
                        {
                            pair.Value.roundActive = true;
                        });
                        break;
                    case BattleItemType.granary:
                        Debug.LogError("granary have no round begin!");
                        break;
                }
                yield return new WaitForSeconds(0.5f);
                roundTime.OnNext((uuid, RoundTime.acting));
                break;
            case RoundTime.acting:
                Debug.Log("round acting");
                //battleItem0.buffCenter.TurnActing();
                switch (battleItem0.type)
                {
                    case BattleItemType.character:
                        if (battleItem0.isPlayer)
                        {
                            if (!battleItem0.canActing)
                            {
                                roundTime.OnNext((uuid, RoundTime.end));
                                battleItem0.canActing = true;
                                GlobalAccess.SaveBattleItem(battleItem0);
                            }
                        } else if (battleItem0.isEnemy)
                        {
                            if (!battleItem0.canActing)
                            {
                                roundTime.OnNext((uuid, RoundTime.end));
                                battleItem0.canActing = true;
                                GlobalAccess.SaveBattleItem(battleItem0);
                            }
                            else
                            {
                                yield return battleManager.StartCoroutine(battleItem0.enemyAI.TurnAction(battleItem0.uuid));
                            }
                        }
                        break;
                    case BattleItemType.time:
                        GameManager.Instance.TimeChanged(-1, true);
                        yield return new WaitForSeconds(1f);
                        roundTime.OnNext((uuid, RoundTime.end));
                        break;
                    case BattleItemType.quitTime:
                        battleManager.BattleEnd(false);
                        break;
                    case BattleItemType.sceneItem:
                        if (!battleItem0.canActing)
                        {
                            roundTime.OnNext((uuid, RoundTime.end));
                            battleItem0.canActing = true;
                            GlobalAccess.SaveBattleItem(battleItem0);
                        }
                        else
                        {
                            Debug.Log("sceneItem round acting");
                            yield return new WaitForSeconds(1f);
                            roundTime.OnNext((uuid, RoundTime.end));
                        }
                        break;
                    case BattleItemType.granary:
                        Debug.LogError("granary have no round acting!");
                        roundTime.OnNext((uuid, RoundTime.end));
                        break;
                }
                break;
            case RoundTime.end:
                Debug.Log("round end");
                battleItem0.RoundEnd();

                switch (battleItem0.type)
                {
                    case BattleItemType.character:
                    case BattleItemType.sceneItem:
                        battleManager.battleItemManager.pos_uibattleItemDic.Values.First(item => { return item.itemID == battleItem0.uuid; }).roundActive = false;
                        break;
                    case BattleItemType.time:
                    case BattleItemType.quitTime:
                        break;
                    case BattleItemType.granary:
                        Debug.LogError("granary have no round end!");
                        break;
                }
                float passedTime;
                if (extraRound > 0)
                {
                    battleItem0.remainActingDistance = 0;
                    passedTime = Mathf.Min(battleItem1.remainActingDistance / battleItem1.attributes.Speed,
                        battleItem0.remainActingDistance / battleItem0.attributes.Speed);
                    battleItem0.remainActingDistance += passedTime * battleItem0.attributes.Speed;// 因为后续还会timePass一次
                }
                else
                {
                    //先计算行动提前效果
                    battleItem0.remainActingDistance = Mathf.Max(0, GlobalAccess.roundDistance - battleItem0.moveAdvancedDistance);
                    //重置行动提前
                    battleItem0.moveAdvancedDistance = 0;

                    passedTime = Mathf.Min(battleItem1.remainActingDistance / battleItem1.attributes.Speed,
                        battleItem0.remainActingDistance / battleItem0.attributes.Speed);
                    battleItem0.remainActingDistance += passedTime * battleItem0.attributes.Speed;// 因为后续还会timePass一次
                }
                if (isInExtraRound && extraRound <= 0)
                {
                    isInExtraRound = false;
                }

                timePass.OnNext(passedTime);
                yield return new WaitForSeconds(1f);
                roundTime.OnNext((uuid, RoundTime.begin));
                break;
            case RoundTime.prepare:
                //开始放置角色
                Dictionary<Vector2, ChessboardSlotColor> dic = battleManager.battleInfo.initPlaceSlots.ToDictionary(vec => vec, vec => ChessboardSlotColor.yellow);
                battleManager.chessBoard.SetColors(dic);
                battleManager.chessboardManager.currentPlaceIndex.OnNext(0);
                battleManager.chessboardManager.clickSlotReason = ClickSlotReason.placeCharacter;
                break;
        }
        yield return null;
    }
}
