using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Vector2Converter: JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Vector2 vector = (Vector2)value;
        JObject jo = new JObject
        {
            { "x", vector.x },
            { "y", vector.y }
        };
        jo.WriteTo(writer);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        float x = jo["x"] != null ? (float)jo["x"] : 0f;
        float y = jo["y"] != null ? (float)jo["y"] : 0f;
        return new Vector2(x, y);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector2);
    }
}
