using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameMode", menuName = "GameMode")]

public class GameMode : ScriptableObject
{
    public GameSprites gameSprites;
    public int winRounds;
    public GameObject paddleControls;
    public BallPath playerBallPath;
}
