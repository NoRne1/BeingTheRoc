using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class UIMainMenu : MonoBehaviour
{
    public TextMeshProUGUI startGame;
    public TextMeshProUGUI options;
    public TextMeshProUGUI exit;
    // Start is called before the first frame update
    void Start()
    {
        DataManager.Instance.DataLoaded.AsObservable().TakeUntilDestroy(this).Subscribe(loaded =>
        {
            if (loaded)
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
        startGame.text = DataManager.Instance.Language["start_game"];
        options.text = DataManager.Instance.Language["options"];
        exit.text = DataManager.Instance.Language["exit"];
    }
}
