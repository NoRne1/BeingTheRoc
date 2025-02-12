using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UICharacterAddExpItem : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI charName;
    public Slider expSlider;
    public TextMeshProUGUI expProgressText;
    public TextMeshProUGUI expPlusText;
    private string uuid;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(string uuid)
    {
        this.uuid = uuid;
        var cm = GlobalAccess.GetCharacterModel(uuid);
        icon.overrideSprite = Resloader.LoadSprite(cm.Resource, ConstValue.battleItemsPath);
        charName.text = cm.Name;
        expSlider.maxValue = GlobalAccess.levelUpExp;
        expSlider.value = Mathf.Min(cm.attributes.remainExp, GlobalAccess.levelUpExp);
        expProgressText.text = cm.attributes.remainExp.ToString() + "/" + GlobalAccess.levelUpExp.ToString();
        expPlusText.gameObject.SetActive(false);
    }

    public void AddExp(int addExp)
    {
        var cm = GlobalAccess.GetCharacterModel(uuid);
        expPlusText.text = "+" + addExp.ToString();
        expPlusText.gameObject.SetActive(true);
        int startValue = cm.attributes.remainExp;
        int endValue = startValue + addExp;
        float AnimationDuration = 1.0f;
        DOTween.To(() => startValue, x => startValue = x, endValue, AnimationDuration).OnUpdate(() =>
            {
                expProgressText.text = startValue.ToString() + "/" + GlobalAccess.levelUpExp.ToString();
            }).OnComplete(() =>
            {
                expProgressText.text = endValue.ToString() + "/" + GlobalAccess.levelUpExp.ToString();
            });
        
        int startValue2 = (int)expSlider.value;
        int endValue2 = Mathf.Min(startValue2 + addExp, GlobalAccess.levelUpExp);
        DOTween.To(() => startValue2, x => startValue2 = x, endValue2, AnimationDuration).OnUpdate(() =>
            {
                expSlider.value = startValue2;
            }).OnComplete(() =>
            {
                expSlider.value = endValue;
            });
        cm.attributes.exp += addExp;
        GlobalAccess.SaveCharacterModel(cm);
    }
}
