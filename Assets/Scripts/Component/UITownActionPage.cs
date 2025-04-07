using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class UITownActionPage : MonoBehaviour
{
    public BehaviorSubject<int> spentTime = new BehaviorSubject<int>(0);
    private int dayTime = 999;
    public int DayTime
    {
        get { return dayTime; }
        set { dayTime = value; }
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        GameManager.Instance.switchPageSubject.AsObservable().Subscribe(pageType => {
            if (enabled)
            {
                var timeChanged = spentTime.Value % dayTime == 0 ? -(spentTime.Value / dayTime) : -((spentTime.Value / dayTime) + 1);
                if (timeChanged != 0)
                {
                    GameManager.Instance.TimeChanged(timeChanged, true);
                }
                spentTime.OnNext(0);
            }
        }).AddTo(this);
        spentTime.AsObservable().Subscribe(time => {
            if (enabled)
            {
                var timeChanged = -(time / dayTime);
                if (timeChanged != 0)
                {
                    GameManager.Instance.TimeChanged(timeChanged, true);
                    spentTime.OnNext(time % dayTime);
                }
            }
        }).AddTo(this);
    }

    protected virtual void OnEnable()
    {
        spentTime.OnNext(0);
    }
}