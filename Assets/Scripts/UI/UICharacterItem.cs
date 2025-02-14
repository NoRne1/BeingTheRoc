using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICharacterItem : MonoBehaviour
{
    public string id;
    public Image iconBG;
    public Image icon;
    public TextMeshProUGUI nameText;
    public Toggle toggle;

    // Start is called before the first frame update
    void Start()
    {
        toggle.onValueChanged.AddListener(delegate { Selected(toggle.isOn); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(string id)
    {
        this.id = id;
        var cm = GlobalAccess.GetCharacterModel(id);
        icon.sprite = Resloader.LoadSprite(cm.Resource, ConstValue.battleItemsPath);
        nameText.text = cm.Name;
    }

    public void Selected(bool selected)
    {
        iconBG.color = selected ? GameUtil.Instance.hexToColor("FFADAD") : Color.white;
    }
}
