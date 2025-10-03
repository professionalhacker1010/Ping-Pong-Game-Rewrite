using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Rendering.Universal;


public class GameManager : MonoBehaviour
{
    //singleton stuff
    #region
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null) Debug.Log("The GameManager is NULL");

            return _instance;
        }
    }
    #endregion

    public string GameScene { get => gameScene; }
    [SerializeField] string gameScene = "Game";

    public int WinRounds { get => gameMode.winRounds; }
    public BallPath PlayerBallPath { get => gameMode.playerBallPath; }
    [SerializeField] GameMode gameMode;

    public List<Pingpong> balls;

    public PaddleControls PaddleControls { get => paddleControls; }
    [SerializeField] PaddleControls paddleControls;

    public GameObject GameUI { get => gameUI; }
    [SerializeField] GameObject gameUI;

    public PolygonCollider2D TableCollider { get => tableCollider; }
    [SerializeField] private PolygonCollider2D tableCollider;

    public int OpponentWins { get => opponentWins; }
    public int PlayerWins { get => playerWins; }

    int playerWins = 0, opponentWins = 0;

    public Opponent opponent;

    public event Action OnGameWon, OnGameLost, OnOpponentScore, OnPlayerScore;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        TransitionManager.Instance.activeScene = gameScene;
    }

    private void Update()
    {
/*        if (DEBUG && Input.GetKeyDown(KeyCode.Space))
        {
            LevelManager.currOpponent++;
            TransitionManager.Instance.QuickOut("LevelSelect");
        }*/
    }

    public void GameOver(bool playerWon)
    {
        LevelManager.SetLevelPlayed(LevelManager.chosenOpponent);

        if (playerWon)
        {
            playerWins = gameMode.winRounds;
            LevelManager.SetLevelWon(LevelManager.chosenOpponent);
            if (OnGameWon != null) OnGameWon();
        }
        else
        {
            opponentWins = gameMode.winRounds;
            if (OnGameLost != null) OnGameLost();
        }
    }

    public void AddOpponentWin()
    {
        opponentWins++;

        if (OnOpponentScore != null) OnOpponentScore();
        //Debug.Log("OPPONENT WINS: " + opponentWins.ToString());
    }

    public void AddPlayerWin()
    {
        playerWins++;

        if (OnPlayerScore != null) OnPlayerScore();
       // Debug.Log("PLAYER WINS: " + playerWins.ToString());
    }

    //use these checks in some UI script that manages the control access and animations
    public bool GameIsWon()
    {
        return playerWins == gameMode.winRounds;
    }

    public bool GameIsLost()
    {
        return opponentWins == gameMode.winRounds;
    }

    public void ResetGame()
    {
        playerWins = 0;
        opponentWins = 0;
    }

    public void SetGameMode(GameMode mode)
    {
        gameMode = mode;
    }

    public void MoveToGameScene(GameObject go)
    {
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(gameScene));
    }

    public int AddBall(Pingpong ball)
    {
        balls.Add(ball);
        return balls.Count - 1;
    }
}
