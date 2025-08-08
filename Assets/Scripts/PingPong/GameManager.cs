using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.ComponentModel;


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

    [SerializeField] private bool DEBUG = false;

    public int WinRounds { get => gameMode.winRounds; }
    public BallPath PlayerBallPath { get => gameMode.playerBallPath; }
    [SerializeField] GameMode gameMode;

    public List<Pingpong> balls;

    public PaddleControls PaddleControls { get => paddleControls; }
    PaddleControls paddleControls;

    public GameUI GameUI { get => gameUI; }
    [SerializeField] GameObject gameUIPrefab;
    GameUI gameUI;

    [SerializeField] private SpriteRenderer BG;
    [SerializeField] private SpriteRenderer table;

    public PolygonCollider2D TableCollider { get => tableCollider; }
    [SerializeField] private PolygonCollider2D tableCollider;

    public int OpponentWins { get => opponentWins; }
    public int PlayerWins { get => playerWins; }

    int playerWins = 0, opponentWins = 0;

    public static string GameScene { get => gameScene; }
    static string gameScene = "Game";

    [HideInInspector] public Opponent opponent;

    public event Action OnGameWon, OnGameLost, OnOpponentScore, OnPlayerScore;

    private void Awake()
    {
        _instance = this;

        paddleControls = Instantiate(gameMode.paddleControls).GetComponent<PaddleControls>();
        MoveToGameScene(paddleControls.gameObject);

        gameUI = Instantiate(gameUIPrefab).GetComponent<GameUI>();
        MoveToGameScene(gameUI.gameObject);
        gameUI.SetSprites(gameMode.gameSprites);
    }

    private void Start()
    {
        TransitionManager.Instance.activeScene = gameScene;
        StartGame();
    }

    public void StartGame()
    {
        StartCoroutine(RoundNumberIntro());

        var opponentObj = LevelManager.Instance.CreateChosenOpponent();
        MoveToGameScene(opponentObj);
        opponent = opponentObj.GetComponent<Opponent>();
    }

    //ROUND# INTRO
    private IEnumerator RoundNumberIntro()
    {
        yield return new WaitForSeconds(0.25f);

        yield return GameUI.ShowRoundNumber(LevelManager.chosenOpponent + 1, 1.5f);

        yield return new WaitForSeconds(0.25f);

        TransitionManager.Instance.QuickIn();
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
            LevelManager.SetLevelWon(LevelManager.chosenOpponent);
            if (OnGameWon != null) OnGameWon();
        }
        else
        {
            if (OnGameLost != null) OnGameLost();
        }
    }

    public void AddOpponentWin()
    {
        opponentWins++;

        OnOpponentScore();
        //Debug.Log("OPPONENT WINS: " + opponentWins.ToString());
    }

    public void AddPlayerWin()
    {
        playerWins++;

        OnPlayerScore();
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

        BG.sprite = gameMode.gameSprites.background;
        table.sprite = gameMode.gameSprites.table;

        if (paddleControls && gameMode.paddleControls)
        {
            Destroy(paddleControls.gameObject);

            paddleControls = Instantiate(gameMode.paddleControls).GetComponent<PaddleControls>();
            MoveToGameScene(paddleControls.gameObject);
        }

        gameUI.SetSprites(gameMode.gameSprites);
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
