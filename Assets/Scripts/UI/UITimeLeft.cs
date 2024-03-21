using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class UITimeLeft : MonoBehaviour
{
    public TextMeshProUGUI timeLeftText;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.timeLeft.AsObservable().DistinctUntilChanged().Subscribe(time =>
        {
            timeLeftText.text = time.ToString();
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
