using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using TMPro;

public class UIFeatherButton : MonoBehaviour
{
    public TextMeshProUGUI featherCoinText;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.featherCoin.AsObservable().DistinctUntilChanged().Subscribe(coin =>
        {
            featherCoinText.text = coin.ToString();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
