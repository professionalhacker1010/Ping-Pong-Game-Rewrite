using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameMode", menuName = "GameMode")]

public class GameMode : ScriptableObject
{
    public int winRounds;
    public BallPath playerBallPath;
}
