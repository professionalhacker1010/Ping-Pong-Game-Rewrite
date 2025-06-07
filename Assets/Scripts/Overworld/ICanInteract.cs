using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanInteract
{
    int InteractPriority { get; }
    Vector2 InteractPos { get; }
    void OnInteract();
    void OnSelect();
    void OnDeselect();
}
