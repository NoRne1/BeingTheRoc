using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;


public class UITeamInfoPage : MonoBehaviour
{
    public Image Character_icon;

    public UIPanelSelector panelSelector;
    public UIPropertyPanel propertyPanel;
    public UIBuffsPanel buffsPanel;

    public TextMeshProUGUI descText;

    public System.IDisposable disposable;
    public CharacterModel character;
    public BattleItem battleItem;
    // Start is called before the first frame update
    void Awake()
    {
        panelSelector.OnToggleValueChanged += PanelSelector_OnToggleValueChanged;
    }

    private void PanelSelector_OnToggleValueChanged(NorneToggle obj)
    {
        switch (obj.toggleType)
        {
            case TogglePanelType.Panel0:
                propertyPanel.gameObject.SetActive(obj.isOn);
                break;
            case TogglePanelType.Panel1:
                buffsPanel.gameObject.SetActive(obj.isOn);
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
        this.character = character;
        this.battleItem = null;
        if (character != null)
        {
            disposable.IfNotNull(dis => { dis.Dispose(); });
            disposable = NorneStore.Instance.ObservableObject<CharacterModel>(character)
                .AsObservable().TakeUntilDestroy(this).Subscribe(cm =>
            {
                descText.text = cm.Desc;
                Character_icon.overrideSprite = Resloader.LoadSprite(cm.Resource, ConstValue.playersPath);
            });
        } else
        {
            Debug.Log("UITeamInfoPage setup character is null");
        }
        propertyPanel.Setup(character);
    }

    public void UpdateBattleItem(BattleItem battleItem, TogglePanelType type = TogglePanelType.Panel0)
    {
        panelSelector.PanelInit(type);
        panelSelector.gameObject.SetActive(true);
        this.character = null;
        this.battleItem = battleItem;
        if (battleItem != null)
        {
            disposable.IfNotNull(dis => { dis.Dispose(); });
            disposable = NorneStore.Instance.ObservableObject<BattleItem>(battleItem)
                .AsObservable().TakeUntilDestroy(this).Subscribe(bi =>
                {
                    descText.text = bi.Desc;
                    Character_icon.overrideSprite = Resloader.LoadSprite(bi.Resource, ConstValue.playersPath);
                });
        }
        else
        {
            Debug.Log("UITeamInfoPage setup battleItem is null");
        }
        propertyPanel.Setup(battleItem);
        buffsPanel.Setup(battleItem);
    }
}
