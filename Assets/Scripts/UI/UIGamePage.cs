using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class UIGamePage : MonoBehaviour
{
    public MarbleGameController marbleGame;
    private BehaviorSubject<int> gameSpentTime = new BehaviorSubject<int>(0);
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.switchPageSubject.AsObservable().Subscribe(pageType => {
            if (enabled)
            {
                var timeChanged = gameSpentTime.Value % 10 == 0 ? -(gameSpentTime.Value / 10) : -((gameSpentTime.Value / 10) + 1);
                if (timeChanged != 0)
                {
                    GameManager.Instance.TimeChanged(timeChanged, true);
                }
                gameSpentTime.OnNext(0);
            }
        }).AddTo(this);
        gameSpentTime.AsObservable().Subscribe(time => {
            if (enabled)
            {
                var timeChanged = -(time / 10);
                if (timeChanged != 0)
                {
                    GameManager.Instance.TimeChanged(timeChanged, true);
                    gameSpentTime.OnNext(time % 10);
                }
            }
        }).AddTo(this);
        marbleGame.gameBeginSubject.AsObservable().Subscribe(pageType => {
            if (enabled)
            {
                gameSpentTime.OnNext(gameSpentTime.Value + 5);
            }
        }).AddTo(this);
    }

    void OnEnable()
    {
        gameSpentTime.OnNext(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
