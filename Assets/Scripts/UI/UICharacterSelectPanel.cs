using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;

public class UICharacterSelectPanel : MonoBehaviour
{
    public TextMeshProUGUI please_select_tip;
    public GameObject character_info_panel;

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

    private int SelectedCharacterId = -1;
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
        please_select_tip.text = DataManager.Instance.Language["select_character"];
        please_select_tip.gameObject.SetActive(true);
        character_info_panel.SetActive(false);
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
        if (DataManager.Instance.Characters.Keys.ToList().Contains(id))
        {
            MaxHP.text = DataManager.Instance.Characters[id].MaxHP.ToString();
            Strength.text = DataManager.Instance.Characters[id].Strength.ToString();
            Defense.text = DataManager.Instance.Characters[id].Defense.ToString();
            Dodge.text = DataManager.Instance.Characters[id].Dodge.ToString();
            Accuracy.text = DataManager.Instance.Characters[id].Accuracy.ToString();
            Speed.text = DataManager.Instance.Characters[id].Speed.ToString();
            Mobility.text = DataManager.Instance.Characters[id].Mobility.ToString();
            Energy.text = DataManager.Instance.Characters[id].Energy.ToString();
            this.SelectedCharacterId = id;
            please_select_tip.gameObject.SetActive(false);
            character_info_panel.SetActive(true);
        } else
        {
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip(DataManager.Instance.Language["wrong_character_selected_tip"]);
        }
        
    }

    public void GoAhead()
    {
        if (SelectedCharacterId == -1)
        {
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip(DataManager.Instance.Language["no_character_selected_tip"]);
        } else if (DataManager.Instance.Characters.Keys.ToList().Contains(SelectedCharacterId))
        {
            GlobalAccess.CurrentCharacterId = SelectedCharacterId;
            SceneManager.Instance.LoadScene("game");
        } else {
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip(DataManager.Instance.Language["wrong_character_selected_tip"]);
        }

    }
}
