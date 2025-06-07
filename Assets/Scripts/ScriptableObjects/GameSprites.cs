using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSprites", menuName = "GameSprites")]
public class GameSprites : ScriptableObject
{
    [Header("UI")]
    public List<Sprite> numbers;
    public List<Sprite> bars;
    public Sprite YOU;
    public Sprite THEM;
    public Sprite round;

    [Header("Environment")]
    public Sprite table;
    public Sprite background;
}
