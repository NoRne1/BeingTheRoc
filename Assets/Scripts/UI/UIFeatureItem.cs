using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFeatureItem : MonoBehaviour
{
    public Image bg;
    public TextMeshProUGUI title;
    public HintComponent hintComponent;
    public void Setup(FeatureDefine define)
    {
        bg.color = GlobalAccess.GetLevelColor(define.Level);
        title.text = GameUtil.Instance.GetDirectDisplayString(define.Title);
        hintComponent.Setup(define);
    }
}
