using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    [System.Serializable]
    public struct DebugCondition
    {
        [SerializeField] public string name;
        [SerializeField] public bool value;
    }

    [SerializeField] List<DebugCondition> conditions;

    private void Start()
    {
        conditions.ForEach(i =>
        {
            Conditions.Set_Debug(i.name, i.value);
        });
    }
}
