using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldSpeedy : OverworldCharacter
{
    protected override void Awake()
    {
        base.Awake();
        Conditions.Initialize(conditionPrefix + "_hasDrink", false);
    }
}
