using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UITownHint : UIHintBase
{
    public TextMeshProUGUI nameText;
    public List<Image> townActionImages;
    public GameObject battleInfoHint;

    public TownModel townModel;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        updatePostion();
    }
    public void Setup(TownModel townModel)  
    {
        this.townModel = townModel;
        nameText.text = GameUtil.Instance.GetDirectDisplayString(townModel.Name);
        switch(townModel.type)
        {
            case TownNodeType.town1:
                nameText.color = GlobalAccess.GetLevelColor(GeneralLevel.green);
                break;
            case TownNodeType.town2:
                nameText.color = GlobalAccess.GetLevelColor(GeneralLevel.blue);
                break;
            case TownNodeType.town3:
                nameText.color = GlobalAccess.GetLevelColor(GeneralLevel.red);
                break;
            case TownNodeType.king:
                nameText.color = Color.red;
                break;
        }
        
        foreach(var index in Enumerable.Range(0, townModel.townActions.Count))
        {
            townActionImages[index].overrideSprite = Resloader.LoadSprite(DataManager.Instance.TownActions[townModel.townActions[index]].iconResource, ConstValue.townActionPath);
        }
        battleInfoHint.SetActive(townModel.status == TownNodeStatus.unpassed);
        StartCoroutine(SetupComplete());
    }
}
