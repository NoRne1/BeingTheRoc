using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System.Linq;
using System.Data;
using System;

public class MarbleGameController : MonoBehaviour
{
    public SpringMechanism springMechanism;
    public Button startButton;
    public Button resetButton;
    public Button restartButton;
    public Button inputButton;
    public Button outputButton;
    public UICoinChangeButton scoreArea;
    public UIEnterTriggle enterTriggle;
    public UIExitTriggle exitTriggle;
    public UIFruitLuckyControl luckyControl;
    
    private List<UIBarrierPoint> uIBarrierPoints;

    private Subject<List<FruitType>> barrierPointSubject = new Subject<List<FruitType>>();
    private BehaviorSubject<int> scoreSubject = new BehaviorSubject<int>(0);

    private BehaviorSubject<Dictionary<FruitType, int>> gameResult = new BehaviorSubject<Dictionary<FruitType, int>>(new Dictionary<FruitType, int>());
    private Dictionary<FruitType, UIFruitResultBase> uiFruitResultDic = new Dictionary<FruitType, UIFruitResultBase>();
    
    private BehaviorSubject<int> remainLaunchCount = new BehaviorSubject<int>(0);
    private BehaviorSubject<List<int>> gamingBallInstanceIDsSubject = new BehaviorSubject<List<int>>(new List<int>());
    public Subject<Unit> gameBeginSubject = new Subject<Unit>();

    private void Awake() {
        startButton.gameObject.SetActive(true);
        uIBarrierPoints = GetComponentsInChildren<UIBarrierPoint>().ToList();

        foreach(var result in GetComponentsInChildren<UIFruitResult>().ToList())
        {
            uiFruitResultDic.Add(result.type, result);
        }
        foreach(var result in GetComponentsInChildren<UIFruitLuckyResult>().ToList())
        {
            uiFruitResultDic.Add(result.type, result);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        luckyControl.ToggleControl(true);
        enterTriggle.SetSubject(gamingBallInstanceIDsSubject);
        exitTriggle.SetSubject(gamingBallInstanceIDsSubject);
        foreach(var point in uIBarrierPoints)
        {
            point.SetSubject(barrierPointSubject);
        }

        scoreArea.SetSubject(scoreSubject);

        startButton.OnClickAsObservable().Subscribe(_=>{
            StartGame();
        }).AddTo(this);
        resetButton.OnClickAsObservable().Subscribe(_=>{
            springMechanism.ResetBalls();
            //只是重启小球位置，已经经过起点的小球也应该计入
            var tempCount = remainLaunchCount.Value;
            tempCount -= gamingBallInstanceIDsSubject.Value.Count;
            remainLaunchCount.OnNext(tempCount);
            gamingBallInstanceIDsSubject.OnNext(new List<int>());
        }).AddTo(this);
        restartButton.OnClickAsObservable().Subscribe(_=>{
            GameOver();
        }).AddTo(this);
        inputButton.OnClickAsObservable().Subscribe(_=>{
            InputMoney();
        }).AddTo(this);
        outputButton.OnClickAsObservable().Subscribe(_=>{
            OutputMoney();
        }).AddTo(this);

        barrierPointSubject.Subscribe(fruitTypes=>{
            ProcessBarrierTriggle(fruitTypes);
        }).AddTo(this);

        gameResult.Subscribe(resultDic => {
            ProcessResultDic(resultDic);
        }).AddTo(this);

        remainLaunchCount.Skip(1).DistinctUntilChanged().Subscribe(count=>{
            if (count <= 0)
            {
                GameOver();
            }
        }).AddTo(this);
        gamingBallInstanceIDsSubject.Pairwise()
            .Subscribe(pair =>
            {
                var tempCount = remainLaunchCount.Value;
                //多少个小球曾经过起点，现在经过了终点
                tempCount -= pair.Previous.Except(pair.Current).ToList().Count;
                remainLaunchCount.OnNext(tempCount);
            }).AddTo(this);
        gamingBallInstanceIDsSubject.CombineLatest(remainLaunchCount, (ids, remainCount)=>(ids, remainCount)).Subscribe(pair =>
            {
                if (pair.ids.Count < pair.remainCount)
                {
                    springMechanism.enable = true;
                } else {
                    springMechanism.enable = false;
                }
            }).AddTo(this);
    }

    public void StartGame()
    {
        if (ScoreChanged(-GlobalAccess.marbleGamePrice))
        {
            //reset
            var result = gameResult.Value;
            foreach(int index in Enumerable.Range(0, (int)FruitType.MAX))
            {
                result[(FruitType)index] = 0;
            }
            gameResult.OnNext(result);
            foreach(var point in uIBarrierPoints)
            {
                point.ResetToggle();
            }
            

            //new game
            remainLaunchCount.OnNext(6);
            startButton.gameObject.SetActive(false);

            luckyControl.ToggleControl(false);
            gameBeginSubject.OnNext(Unit.Default);
        }
    }

    public void InputMoney()
    {
        if (GlobalAccess.marbleGamePrice > GameManager.Instance.featherCoin.Value)
        {
            //钱不够买
            UITip tip = UIManager.Instance.Show<UITip>();
            //todo
            tip.UpdateTip("game_no_money_input");
        } else {
            GameManager.Instance.FeatherCoinChanged(-GlobalAccess.marbleGamePrice);
            ScoreChanged(GlobalAccess.marbleGamePrice);
        }
    }

    public void OutputMoney()
    {
        if (scoreSubject.Value > 0)
        {
            GameManager.Instance.FeatherCoinChanged(scoreSubject.Value);
            ScoreChanged(-scoreSubject.Value);
        }
    }

    public bool ScoreChanged(int change)
    {
        if (scoreSubject.Value + change < 0)
        {
            //错误请求(扣成负的了)
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip("game_start_no_point");
            return false;
        }
        scoreSubject.OnNext(scoreSubject.Value + change);
        return true;
    }

    private void ProcessBarrierTriggle(List<FruitType> fruitTypes)
    {
        foreach(var type in fruitTypes)
        {
            gameResult.Value[type] += 1;
        }
        gameResult.OnNext(gameResult.Value);
    }

    private void ProcessResultDic(Dictionary<FruitType, int> resultDic)
    {
        foreach(var resultPair in resultDic)
        {
            uiFruitResultDic[resultPair.Key].SetResult(resultPair.Value);
        }
    }

    private void SettleResult()
    {
        var score = 0;
        foreach(var resultPair in gameResult.Value)
        {
            switch (resultPair.Key)
            {
                case FruitType.pear:
                case FruitType.mango:
                case FruitType.apple:
                case FruitType.banana:
                case FruitType.cherry:
                case FruitType.watermelon:
                    if (resultPair.Value == 3)
                    {
                        score += GlobalAccess.GetFruitTypePoint(resultPair.Key);
                    } else if (resultPair.Value >= 4){
                        score += GlobalAccess.GetFruitTypePoint(resultPair.Key) * 2;
                    }
                    break;
                case FruitType.L:
                case FruitType.U:
                case FruitType.C:
                case FruitType.K:
                case FruitType.Y:
                    break;
                default:
                    Debug.LogError("MarbleGameController SettleResult unknown fruitType");
                    break;
            }
        }
        if (gameResult.Value[FruitType.L] == 1 &&
            gameResult.Value[FruitType.U] == 1 &&
            gameResult.Value[FruitType.C] == 1 &&
            gameResult.Value[FruitType.K] == 1 &&
            gameResult.Value[FruitType.Y] == 1)
            {
                score += GlobalAccess.GetFruitTypePoint(FruitType.L);
            }
        ScoreChanged(score);
    }

    private void GameOver()
    {
        springMechanism.ResetBalls();
        SettleResult();
        startButton.gameObject.SetActive(true);
        gamingBallInstanceIDsSubject.OnNext(new List<int>());
        luckyControl.ToggleControl(true);
    }
}
