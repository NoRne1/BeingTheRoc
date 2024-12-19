using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class UIExitTriggle : MonoBehaviour
{
    private BehaviorSubject<List<int>> gamingBallInstanceIDsSubject;
    
    public void SetSubject(BehaviorSubject<List<int>> subject)
    {
        this.gamingBallInstanceIDsSubject = subject;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Ball") && gamingBallInstanceIDsSubject.Value.Contains(other.gameObject.GetInstanceID())) {
            //小球经过过起点，现在经过终点
            List<int> temp = new List<int>(gamingBallInstanceIDsSubject.Value);
            temp.Remove(other.gameObject.GetInstanceID());
            gamingBallInstanceIDsSubject.OnNext(temp);
        }
    }
}
