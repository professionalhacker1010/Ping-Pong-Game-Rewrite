using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Conditions
{
    private static Dictionary<string, bool> conditions = new Dictionary<string, bool>();

    public static void SetCondition(string name, bool value)
    {
        conditions[name] = value;
    }

    public static bool GetCondition(string name) { 
        if (conditions.ContainsKey(name))
        {
            return conditions[name];
        }
        else
        {
            Debug.LogError("Condition " + name + " not tracked.");
            return false;
        }
    }

    public static bool HasCondition(string name)
    {
        return conditions.ContainsKey(name);
    }
}
