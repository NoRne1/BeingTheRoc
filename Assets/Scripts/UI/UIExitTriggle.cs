using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class UIExitTriggle : MonoBehaviour
{
    private BehaviorSubject<int> remainLaunchCount;
    
    public void SetSubject(BehaviorSubject<int> subject)
    {
        this.remainLaunchCount = subject;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Ball") && remainLaunchCount.Value > 0) {
            remainLaunchCount.OnNext(remainLaunchCount.Value - 1);
        }
    }
}
