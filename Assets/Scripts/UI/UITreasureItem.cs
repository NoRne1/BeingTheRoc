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
    private bool canClick = false;


    void Start()
    {
        button.OnClickAsObservable().Subscribe(_ => {
            if (this.model != null && canClick)
            {
                switch (model.treasureDefine.invokeType)
                {
                    case TreasureInvokeType.battleUse:
                        if (BattleManager.Instance.isInBattle)
                        {
                            GameManager.Instance.treasureManager.InvokeTreasureEffect(this.model.ID);
                        }
                        break;
                    case TreasureInvokeType.normalUse:
                        if (!BattleManager.Instance.isInBattle)
                        {
                            GameManager.Instance.treasureManager.InvokeTreasureEffect(this.model.ID);
                        }
                        break;
                    default:
                        Debug.LogError("UITreasureItem clicked but invokeType can not click");
                        break;
                }
            }
        });
    }

    void Update()
    {
        if (canClick)
        {
            //Time.time * 0.7f,自增值,0.7控制速率
            var alpha = Mathf.PingPong(Time.time * 0.7f, 0.7f) + 0.3f;
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
                        canClick = true;
                        break;
                    case TreasureInvokeType.normalUse:
                        canClick = true;
                        break;
                    default:
                        canClick = false;
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
