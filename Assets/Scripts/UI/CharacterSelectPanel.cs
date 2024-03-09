using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterSelectPanel : MonoBehaviour
{
    public TextMeshProUGUI MaxHP;
    public TextMeshProUGUI Strength;
    public TextMeshProUGUI Defense;
    public TextMeshProUGUI Dodge;
    public TextMeshProUGUI Accuracy;
    public TextMeshProUGUI Speed;
    public TextMeshProUGUI Mobility;
    // Start is called before the first frame update
    void Start()
    {
        DataManager.Instance.Load();
    }

    // Update is called once per frame
    void Update()
    {
        
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
    }
}
