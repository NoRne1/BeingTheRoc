using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIFruitResultBase : MonoBehaviour
{
    public FruitType type;
    public abstract void Reset();
    public abstract void SetResult(int num);
}

public class UIFruitResult : UIFruitResultBase
{
    public Image icon;
    public TextMeshProUGUI pointText;
    public List<Image> countImages;
    // Start is called before the first frame update
    void Start()
    {
        icon.overrideSprite = Resloader.LoadSprite(type.ToString(), ConstValue.fruitsPath);
        pointText.text = GlobalAccess.GetFruitTypePoint(type).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void Reset()
    {
        foreach(var image in countImages) 
        {
            image.color = Color.grey;
        }
    }

    public override void SetResult(int num)
    {
        foreach(var index in Enumerable.Range(0, countImages.Count)) 
        {
            if (index < num)
            {
                countImages[index].color = Color.red;
            } else {
                countImages[index].color = Color.grey;
            }
        }
    }
}
