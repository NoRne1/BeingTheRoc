using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System.Security.Cryptography;


public class UITeamInfoPage : MonoBehaviour
{
    public UIPanelSelector panelSelector;
    public UIBaseInfoPanel baseInfoPanel;
    public UIPropertyPanel propertyPanel;
    public UIBuffsPanel buffsPanel;
    public System.IDisposable disposable;
    public CharacterModel character;
    public BattleItem battleItem;

    public TogglePanelType currentPanel = TogglePanelType.Panel0;
    // Start is called before the first frame update
    void Awake()
    {
        panelSelector.OnToggleValueChanged += PanelSelector_OnToggleValueChanged;
    }

    void Start() 
    {
        panelSelector.PanelInit(TogglePanelType.Panel0);
        propertyPanel.changeButton.plusButton.OnClickAsObservable().Subscribe(_ => { 
            if (character != null)
            {
                character.attributes.setGrowthPropertyValue(propertyPanel.changeButton.SelectedAttributeType, 1);
            }
        }).AddTo(this);

        propertyPanel.changeButton.minusButton.OnClickAsObservable().Subscribe(_ => { 
            if (character != null)
            {
                character.attributes.setGrowthPropertyValue(propertyPanel.changeButton.SelectedAttributeType, -1);
            }
        }).AddTo(this);
    
        propertyPanel.changeButton.progressCheck = () => propertyPanel.changeButton.expandedSubject.Value;
        propertyPanel.changeButton.onImmediateAction = () => { 
            propertyPanel.changeButton.ToggleButtons(); 
        };
        propertyPanel.changeButton.onProgressCompleteAction = () => {
            if (character != null)
            {
                character.attributes.ResetGrowthProperty();
            }
        };
    }

    private void PanelSelector_OnToggleValueChanged(NorneToggle obj)
    {
        switch (obj.toggleType)
        {
            case TogglePanelType.Panel0:
                propertyPanel.gameObject.SetActive(obj.isOn);
                currentPanel = TogglePanelType.Panel0;
                break;
            case TogglePanelType.Panel1:
                buffsPanel.gameObject.SetActive(obj.isOn);
                currentPanel = TogglePanelType.Panel1;
                break;
            default:
                Debug.LogError("PanelSelector_OnToggleValueChanged unknown NorneToggle");
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateCharacter(CharacterModel character)
    {
        panelSelector.PanelInit(TogglePanelType.Panel0);
        panelSelector.gameObject.SetActive(false);
        propertyPanel.changeButton.gameObject.SetActive(true);
        this.character = character;
        this.battleItem = null;
        baseInfoPanel.Setup(character);
        propertyPanel.Setup(character);
    }

    public void UpdateBattleItem(BattleItem battleItem, TogglePanelType type)
    {
        panelSelector.PanelInit(type);
        panelSelector.gameObject.SetActive(true);
        propertyPanel.changeButton.gameObject.SetActive(false);
        this.character = null;
        this.battleItem = battleItem;
        baseInfoPanel.Setup(battleItem);
        propertyPanel.Setup(battleItem);
        buffsPanel.Setup(battleItem);
    }
}
