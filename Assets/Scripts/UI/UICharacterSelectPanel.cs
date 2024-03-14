using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

public class UICharacterSelectPanel : MonoBehaviour
{
    public TextMeshProUGUI MaxHP_key;
    public TextMeshProUGUI Strength_key;
    public TextMeshProUGUI Defense_key;
    public TextMeshProUGUI Dodge_key;
    public TextMeshProUGUI Accuracy_key;
    public TextMeshProUGUI Speed_key;
    public TextMeshProUGUI Mobility_key;
    public TextMeshProUGUI Energy_key;

    public TextMeshProUGUI Go_ahead_key;

    public TextMeshProUGUI MaxHP;
    public TextMeshProUGUI Strength;
    public TextMeshProUGUI Defense;
    public TextMeshProUGUI Dodge;
    public TextMeshProUGUI Accuracy;
    public TextMeshProUGUI Speed;
    public TextMeshProUGUI Mobility;
    public TextMeshProUGUI Energy;
    // Start is called before the first frame update
    void Start()
    {
        DataManager.Instance.DataLoaded.AsObservable().Subscribe(loaded =>
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
        MaxHP_key.text = DataManager.Instance.Language["health"];
        Strength_key.text = DataManager.Instance.Language["strength"];
        Defense_key.text = DataManager.Instance.Language["defense"];
        Dodge_key.text = DataManager.Instance.Language["dodge"];
        Accuracy_key.text = DataManager.Instance.Language["accuracy"];
        Speed_key.text = DataManager.Instance.Language["speed"];
        Mobility_key.text = DataManager.Instance.Language["mobility"];
        Energy_key.text = DataManager.Instance.Language["energy"];

        Go_ahead_key.text = DataManager.Instance.Language["go_ahead"];
    }

    public void SelectedCharacter(int id)
    {
        MaxHP.text = DataManager.Instance.Characters[id].MaxHP.ToString();
        Strength.text = DataManager.Instance.Characters[id].Strength.ToString();
        Defense.text = DataManager.Instance.Characters[id].Defense.ToString();
        Dodge.text = DataManager.Instance.Characters[id].Dodge.ToString();
        Accuracy.text = DataManager.Instance.Characters[id].Accuracy.ToString();
        Speed.text = DataManager.Instance.Characters[id].Speed.ToString();
        Mobility.text = DataManager.Instance.Characters[id].Mobility.ToString();
        Energy.text = DataManager.Instance.Characters[id].Energy.ToString();
    }
}
