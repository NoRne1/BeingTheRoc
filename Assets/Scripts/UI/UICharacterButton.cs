using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterButton : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI race;
    public Image jobIcon;
    public List<UISkillButton> skillButtons;
    private Dictionary<AttributeType, UIPropertyIconDisplay> propertyDisplays = new Dictionary<AttributeType, UIPropertyIconDisplay>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (var propertyDisplay in GetComponentsInChildren<UIPropertyIconDisplay>())
        {
            propertyDisplays.Add(propertyDisplay.attributeType, propertyDisplay);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
