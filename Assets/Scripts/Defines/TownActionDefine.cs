using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 城镇行为定义
    /*"1": {"ID":1,
     * "Name":"大红",
     * "MaxHP":300,
     * "Strength":20,
     * "Defense":20,
     * "Dodge":0,
     * "Accuracy":0,
     * "Speed":100,
     * "Mobility":2
     * "Energy":2
     * }...
    */
/// </summary>
public class TownActionDefine
{
    //ID
    public int ID { get; set; }
    //type
    public TownActionType townActionType { get; set; }
    //type
    public string iconResource { get; set; }
    //type
    public string titleIndex { get; set; }
    //type
    public string descIndex { get; set; }
}

