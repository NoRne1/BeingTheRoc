using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEntryDesc : MonoBehaviour
{
    public Image bg;
    public List<Sprite> bgSprites;
    public TextMeshProUGUI desc;
    // Start is called before the first frame update
    public void Setup(EquipExtraEntryModel model, bool anim)
    {
        bg.overrideSprite = bgSprites[(int)model.level];
        if (model.effect.value == 0)
        {
            desc.text = GameUtil.Instance.GetDisplayString(model.descString);
        } else {
            desc.text = string.Format(GameUtil.Instance.GetDisplayString(model.descString), model.effect.value).ReplaceNewLines();
        }
        Color startColor = bg.color;
        startColor.a = 0;
        bg.color = startColor;
        if (anim)
        {
            
            // 创建闪光效果的动画
            bg.DOFade(1f, 0.3f).OnComplete(() => 
            {
                // 完成后再将透明度设置回0
                bg.DOFade(0f, 0.5f);
            });
        }
    }
}
