using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoodModel
{
    public int ID { get; set; }
    public string title { get; set; }
    public List<FoodProperty> foodPropertys = new List<FoodProperty>();
    public int priceFloatFactor;

    public FoodModel(FoodDefine define)
    {
        this.ID = define.ID;
        this.title = define.title;
        ProcessFoodProperty(define.property1);
        ProcessFoodProperty(define.property2);
        ProcessFoodProperty(define.property3);
        ProcessFoodProperty(define.property4);
        this.priceFloatFactor = GameUtil.Instance.GetTrulyFloatFactor(define.priceFloatFactor);
    }

    public void ProcessFoodProperty(FoodProperty foodProperty)
    {
        if (foodProperty == null) 
        {
            return;
        }
        FoodProperty temp = GameUtil.Instance.DeepCopy(foodProperty);
        temp.floatFactor = GameUtil.Instance.GetTrulyFloatFactor(temp.floatFactor);
        foodPropertys.Add(temp);
    }

    public List<Effect> GetEffects()
    {
        return foodPropertys.Select(property => 
            new Effect(EffectType.property, EffectInvokeType.useInstant, property.type, 1, 
            "", property.trulyValue)).ToList();
    }
}
