using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Conditions
{
    private static Dictionary<string, bool> conditions = new Dictionary<string, bool>();

    public static void Initialize(string name, bool value)
    {
        if (!Has(name)) conditions[name] = value;
    }

    public static void Set(string name, bool value)
    {
        if (!Has(name)) { Debug.LogError("Condition " + name + " not initialized."); return; }
        conditions[name] = value;
    }

    public static void Set_Debug(string name, bool value) => conditions[name] = value;

    public static bool Get(string name) { 
        if (conditions.ContainsKey(name))
        {
            return conditions[name];
        }
        else
        {
            Debug.LogError("Condition " + name + " not initialized.");
            return false;
        }
    }

    public static bool Has(string name)
    {
        return conditions.ContainsKey(name);
    }

    public static bool Evaluate(string condition)
    {
        bool result = ExpressionEvaluator.Evaluate(condition, conditions);
/*        if (result) Debug.Log("Condition " + condition + " was true");
        else Debug.Log("Condition " + condition + " was false");*/
        return result;
    }
}
