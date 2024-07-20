using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;

public class UITreasureItem : MonoBehaviour
{
    private string buttonHexColor = "FFEC00";
    
    public Image bg;
    public Image icon;
    public Button button;
    public GameObject numObject;
    public TextMeshProUGUI numText;

    public StoreItemModel model;
    private bool colorBlink = false;


    void Start()
    {
        button.OnClickAsObservable().Subscribe(_ => {
            if (this.model != null)
            {
                GameManager.Instance.treasureManager.InvokeTreasureEffect(this.model.ID);
            }
        });
    }

    void Update()
    {
        if (colorBlink)
        {
            var alpha = Mathf.PingPong(Time.time * 0.3f, 0.3f) + 0.5f;
            bg.color = GameUtil.Instance.hexToColor("FFEC00", alpha);
        }
        else
        {
            bg.color = Color.black; // Black with alpha 1
        }
    }

    public void Setup(StoreItemModel model, int num)
    {
        this.model = model;
        switch (model.type)
        {
            case ItemType.treasure:
                icon.overrideSprite = Resloader.LoadSprite(model.iconResource, ConstValue.equipsPath);
                switch (model.treasureDefine.invokeType)
                {
                    case TreasureInvokeType.battleUse:
                        button.interactable = true;
                        colorBlink = true;
                        break;
                    default:
                        button.interactable = false;
                        colorBlink = false;
                        break;
                }

                numText.text = num.ToString();
                numObject.SetActive(num > 1);
                break;
            default:
                Debug.LogError("UITreasureItem Setup not a treasure");
                break;
        }
    }

    public void Reset()
    {
        model = null;
        icon.overrideSprite = null;
    }
}
