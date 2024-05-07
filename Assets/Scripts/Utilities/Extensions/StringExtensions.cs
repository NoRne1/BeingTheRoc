using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringExtensions
{
    public static string ReplaceNewLines(this string input)
    {
        return input.Replace("\\n", Environment.NewLine);
    }
}
