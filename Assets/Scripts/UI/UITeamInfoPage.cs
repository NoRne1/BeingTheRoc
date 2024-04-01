using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITeamInfoPage : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI MaxHP_key;
    public TextMeshProUGUI Strength_key;
    public TextMeshProUGUI Defense_key;
    public TextMeshProUGUI Dodge_key;
    public TextMeshProUGUI Accuracy_key;
    public TextMeshProUGUI Speed_key;
    public TextMeshProUGUI Mobility_key;
    public TextMeshProUGUI Energy_key;

    public Image Character_icon;

    public TextMeshProUGUI descText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;

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
        MaxHP_key.text = DataManager.Instance.Language["health"];
        Strength_key.text = DataManager.Instance.Language["strength"];
        Defense_key.text = DataManager.Instance.Language["defense"];
        Dodge_key.text = DataManager.Instance.Language["dodge"];
        Accuracy_key.text = DataManager.Instance.Language["accuracy"];
        Speed_key.text = DataManager.Instance.Language["speed"];
        Mobility_key.text = DataManager.Instance.Language["mobility"];
        Energy_key.text = DataManager.Instance.Language["energy"];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateCharacter(CharacterModel character)
    {
        descText.text = character.Desc;
        levelText.text = "Lv: " + character.level.ToString();
        expText.text = character.remainExp.ToString() + "/" + GlobalAccess.levelUpExp.ToString();
        title.text = character.Name;
        MaxHP.text = character.MaxHP.ToString();
        Strength.text = character.Strength.ToString();
        Defense.text = character.Defense.ToString();
        Dodge.text = character.Dodge.ToString();
        Accuracy.text = character.Accuracy.ToString();
        Speed.text = character.Speed.ToString();
        Mobility.text = character.Mobility.ToString();
        Energy.text = character.Energy.ToString();
        Character_icon.overrideSprite = Resloader.Load<Sprite>(ConstValue.spritePath + character.Resource);
    }
}
