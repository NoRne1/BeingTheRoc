using System.Collections.Generic;

public static class ObjectExtensions
{
    public static Dictionary<string, object> ToDictionary(this object obj)
    {
        var dictionary = new Dictionary<string, object>();
        foreach (var property in obj.GetType().GetProperties())
        {
            if (property.CanRead)
            {
                dictionary[property.Name] = property.GetValue(obj);
            }
        }
        return dictionary;
    }
}