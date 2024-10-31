using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodProperty
{
    public PropertyType type;
    public int value;
    //define中时浮动范围，model中时具体的浮动值
    public int floatFactor;
    public int trulyValue 
    {
        get { return value + floatFactor; }
    }
}

public class FoodDefine
{
    public int ID { get; set; }
    public string title { get; set; }
    public FoodProperty property1 { get; set; }
    public FoodProperty property2 { get; set; }
    public FoodProperty property3 { get; set; }
    public FoodProperty property4 { get; set; }
    public int priceFloatFactor;
}
