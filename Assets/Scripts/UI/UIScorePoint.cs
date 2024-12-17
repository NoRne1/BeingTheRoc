using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIScorePoint : MonoBehaviour
{
    private Toggle toggle;
    private Subject<Unit> enterScorePoint;
    private void Awake() {
        toggle = GetComponent<Toggle>();
    }
    private void Start() {
        toggle.onValueChanged.AddListener(isOn =>
        {
            ColorBlock cb = toggle.colors;
            if (isOn)
            {
                cb.disabledColor = Color.red;
            }
            else
            {
                cb.disabledColor = Color.grey;
            }
            toggle.colors = cb;
        });
    }

    public void SetSubject(Subject<Unit> subject)
    {
        enterScorePoint = subject;
    }

    public void SetToggle(bool isOn)
    {
        toggle.isOn = isOn;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Ball")) {
            // 判断当前物体是否是标签为 "BarrierPoint" 的点
            if (!toggle.isOn)
            {
                SetToggle(true);
                enterScorePoint.OnNext(Unit.Default);
            }
        }
    }
}
