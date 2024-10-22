using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class UIPropertyPanel : MonoBehaviour
{
    public Image jobIcon;
    public TextMeshProUGUI jobDesc;
    public Image raceIcon;
    public TextMeshProUGUI raceDesc;
    public ToggleGroup toggleGroup;
    private Dictionary<AttributeType, UIPropertyDisplay> propertyDisplays = new Dictionary<AttributeType, UIPropertyDisplay>();
    public UIPropertyChangeButton changeButton;
    public System.IDisposable disposable;
    // Start is called before the first frame update
    void Awake()
    {
        foreach (var propertyDisplay in GetComponentsInChildren<UIPropertyDisplay>())
        {
            propertyDisplays.Add(propertyDisplay.attributeType, propertyDisplay);
            propertyDisplay.SetupKey(propertyDisplay.attributeType.ToString());
        }
    }

    void Start() 
    {
        changeButton.expandedSubject.AsObservable().Subscribe(expanded => {
            //取消选中
            toggleGroup.GetFirstActiveToggle().IfNotNull(toggle => { toggle.isOn = false; });
            //设置toggle可交互
            foreach(var propertyDisplay in propertyDisplays.Values) 
            {
                propertyDisplay.SetToggleActive(expanded);
            }
        }).AddTo(this);

        foreach(var propertyDisplay in propertyDisplays.Values) 
        {
            propertyDisplay.selfToggle.OnValueChangedAsObservable().Subscribe(selected => {
                changeButton.SetSelectedAttributeType(selected ? propertyDisplay.attributeType : AttributeType.None);
            }).AddTo(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(CharacterModel character)
    {
        if (character != null)
        {
            disposable.IfNotNull(dis => { dis.Dispose(); });
            disposable = NorneStore.Instance.ObservableObject<CharacterModel>(character)
                .AsObservable().TakeUntilDestroy(this).Subscribe(cm =>
                {
                    jobIcon.overrideSprite = Resloader.LoadSprite(cm.Resource, ConstValue.battleItemsPath);
                    raceIcon.overrideSprite = Resloader.LoadSprite(cm.Resource, ConstValue.battleItemsPath);
                    jobDesc.text = "援御";
                    raceDesc.text = GameUtil.Instance.GetDisplayString(cm.Desc);
                    changeButton.remainPoints.text = cm.attributes.RemainPropertyPoints.ToString();
                    foreach (var propertyDisplay in propertyDisplays.Values)
                    {
                        propertyDisplay.SetupValue(cm.attributes.getFinalPropertyValue(propertyDisplay.attributeType).ToString());
                    }
                });
        }
        else
        {
            Debug.Log("UITeamInfoPage setup character is null");
        }
    }

    public void Setup(BattleItem battleItem)
    {
        if (battleItem != null)
        {
            disposable.IfNotNull(dis => { dis.Dispose(); });
            disposable = NorneStore.Instance.ObservableObject<BattleItem>(battleItem)
                .AsObservable().TakeUntilDestroy(this).Subscribe(bi =>
                {
                    jobIcon.overrideSprite = Resloader.LoadSprite(bi.Resource, ConstValue.battleItemsPath);
                    raceIcon.overrideSprite = Resloader.LoadSprite(bi.Resource, ConstValue.battleItemsPath);
                    jobDesc.text = "援御";
                    raceDesc.text = GameUtil.Instance.GetDisplayString(bi.Desc);
                    foreach (var propertyDisplay in propertyDisplays.Values) 
                    {
                        propertyDisplay.SetupValue(bi.attributes.getFinalPropertyValue(propertyDisplay.attributeType).ToString());
                    }
                });
        }
        else
        {
            Debug.Log("UITeamInfoPage setup battleItem is null");
        }
    }
}
