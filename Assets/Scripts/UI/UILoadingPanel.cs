using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class UILoadingPanel : MonoBehaviour
{ 
    public TextMeshProUGUI loading_tip;

    // Start is called before the first frame update
    void Start()
    {
       DataManager.Instance.DataLoaded.AsObservable().Subscribe(loaded =>
       {
           if(loaded)
           {
               this.Init();
           }
       });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {
        loading_tip.text = DataManager.Instance.Language["loading_tip"];
    }
}
