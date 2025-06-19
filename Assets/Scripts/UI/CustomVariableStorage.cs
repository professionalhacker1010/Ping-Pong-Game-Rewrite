using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Yarn.Unity;

public class CustomVariableStorage : Yarn.Unity.VariableStorageBehaviour
{
    [SerializeField] private InMemoryVariableStorage variableStorage;

    public override void Clear()
    {

    }

    public override bool Contains(string variableName)
    {
        return false;
    }

    public override (Dictionary<string, float> FloatVariables, Dictionary<string, string> StringVariables, Dictionary<string, bool> BoolVariables) GetAllVariables()
    {
        return (new Dictionary<string, float>(), new Dictionary<string, string>(), new Dictionary<string, bool>());
    }

    public override void SetAllVariables(Dictionary<string, float> floats, Dictionary<string, string> strings, Dictionary<string, bool> bools, bool clear = true)
    {
        
    }

    public override void SetValue(string variableName, string stringValue)
    {
        
    }

    public override void SetValue(string variableName, float floatValue)
    {
        
    }

    public override void SetValue(string variableName, bool boolValue)
    {
        
    }

    public override bool TryGetValue<T>(string variableName, out T result)
    {
        result = default(T);
        return false;
    }

    /*    // Store a value into a variable
        public override void SetValue(string variableName, Yarn.Value value)
        {
            // 'variableName' is the name of the variable that 'value' 
            // should be stored in.
            foreach (var item in variableStorage.defaultVariables)
            {
                if ("$" + item.name == variableName)
                {
                    item.value = value.AsString;
                }
            }
        }

        // Return a value, given a variable name
        public override Yarn.Value GetValue(string variableName)
        {
            foreach (var item in variableStorage.defaultVariables)
            {
                if ("$" + item.name == variableName)
                {
                    return new Yarn.Value(item.value);
                }
            }
            return new Yarn.Value();
        }

        // Return to the original state
        public override void ResetToDefaults()
        {
            return;
        }*/
}
