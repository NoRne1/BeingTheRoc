using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class UIGamePage : UITownActionPage
{
    public MarbleGameController marbleGame;
    // Start is called before the first frame update
    protected override void Start()
    {
        DayTime = 10;
        base.Start();
        marbleGame.gameBeginSubject.AsObservable().Subscribe(pageType => {
            if (enabled)
            {
                spentTime.OnNext(spentTime.Value + 5);
            }
        }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
