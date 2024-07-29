using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System;

public class Vector2IntConverter : JsonConverter
{
     public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Vector2Int vector = (Vector2Int)value;
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
        int x = jo["x"] != null ? (int)jo["x"] : 0;
        int y = jo["y"] != null ? (int)jo["y"] : 0;
        return new Vector2Int(x, y);
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector2Int);
    }
}
