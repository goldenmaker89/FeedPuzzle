using UnityEngine;
using Coplay.Controllers.Functions;
using System;

public class InspectElementType
{
    public static string Execute()
    {
        string result = "ElementType members:\n";
        foreach (var name in Enum.GetNames(typeof(ElementType)))
        {
            result += name + "\n";
        }
        return result;
    }
}
