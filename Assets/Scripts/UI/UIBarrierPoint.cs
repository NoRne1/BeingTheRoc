using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public enum FruitType
{
    none = -1,
    pear = 0,
    mango = 1,
    apple = 2,
    banana = 3,
    cherry = 4,
    watermelon = 5,
    L = 6,
    U = 7,
    C = 8,
    K = 9,
    Y = 10,
    MAX
}

public class UIBarrierPoint : MonoBehaviour
{
    public List<FruitType> fruitTypes;
    public Image fruit1;
    public Image fruit2;
    public Image fruit3;
    public TextMeshProUGUI text;
    public UIScorePoint scorePoint;
    private Subject<List<FruitType>> barrierPointSubject;
    private Subject<Unit> enterScorePoint = new Subject<Unit>();
    // Start is called before the first frame update
    void Start()
    {
        fruit1.gameObject.SetActive(false);
        fruit2.gameObject.SetActive(false);
        fruit3.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
        if (fruitTypes.Count == 1)
        {
            setFruitType(fruitTypes[0], fruit3);
        } else if (fruitTypes.Count == 2)
        {
            setFruitType(fruitTypes[0], fruit1);
            setFruitType(fruitTypes[1], fruit2);
        } else if (fruitTypes.Count == 3)
        {
            setFruitType(fruitTypes[0], fruit1);
            setFruitType(fruitTypes[1], fruit2);
            setFruitType(fruitTypes[2], fruit3);
        } else {
            Debug.LogError("UIBarrierPoint fruitTypes error count");
        } 

        scorePoint.SetSubject(enterScorePoint);
        enterScorePoint.Subscribe(_ => {
            barrierPointSubject?.OnNext(this.fruitTypes);
        }).AddTo(this);
    }

    public void setFruitType(FruitType fruitType, Image image)
    {
        switch(fruitType)
        {
            case FruitType.pear:
            case FruitType.mango:
            case FruitType.apple:
            case FruitType.banana:
            case FruitType.cherry:
            case FruitType.watermelon:
                image.overrideSprite = Resloader.LoadSprite(fruitType.ToString(), ConstValue.fruitsPath);
                image.gameObject.SetActive(true);
                break;
            case FruitType.L:
            case FruitType.U:
            case FruitType.C:
            case FruitType.K:
            case FruitType.Y:
                text.text = fruitType.ToString();
                text.gameObject.SetActive(true);
                break;
            default:
                Debug.LogError("UIBarrierPoint setFruitType unknown fruitType");
                break;
        }
    }

    public void SetSubject(Subject<List<FruitType>> subject)
    {
        barrierPointSubject = subject;
    }

    public void ResetToggle()
    {
        scorePoint.SetToggle(false);
    }
}
