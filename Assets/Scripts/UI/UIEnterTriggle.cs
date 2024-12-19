using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class UIEnterTriggle : MonoBehaviour
{
    private BehaviorSubject<List<int>> gamingBallInstanceIDsSubject;
    
    public void SetSubject(BehaviorSubject<List<int>> subject)
    {
        this.gamingBallInstanceIDsSubject = subject;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Ball") && !gamingBallInstanceIDsSubject.Value.Contains(other.gameObject.GetInstanceID())) {
            //小球经过过起点，现在经过终点
            List<int> temp = new List<int>(gamingBallInstanceIDsSubject.Value);
            temp.Add(other.gameObject.GetInstanceID());
            gamingBallInstanceIDsSubject.OnNext(temp);
        }
    }
}
