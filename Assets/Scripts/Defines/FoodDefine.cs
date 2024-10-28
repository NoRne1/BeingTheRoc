using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodProperty
{
    public PropertyType type;
    public int value;
    public int floatFactor;
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
