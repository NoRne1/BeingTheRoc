using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeatherEffectType
{
    none = -1,
    battleBuff = 0,
    GeneralBuff = 1,
}

public class WeatherEffect
{
    public WeatherEffectType? effectType;
    public int buffID;
}

public class WeatherDefine
{
    public int ID { get; set; }
    public string Resource { get; set; }
    public string title { get; set; }
    public string desc { get; set; }
    public WeatherEffect effect { get; set; }
}
