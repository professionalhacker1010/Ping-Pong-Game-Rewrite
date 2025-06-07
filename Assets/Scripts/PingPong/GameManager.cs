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

    [SerializeField] GameMode gameMode;
    [SerializeField] Pingpong pingPong;
    [SerializeField] GameObject gameUIPrefab;

    [SerializeField] private SpriteRenderer BG;
    [SerializeField] private SpriteRenderer table;

    [HideInInspector] public Opponent opponent;

    int playerWins = 0, playedRounds = 0, opponentWins = 0;
    bool playerHasWon = false;
    string gameScene = "Game";
    PaddleControls paddleControls;
    GameUI gameUI;

    public int OpponentWins { get => opponentWins; }
    public int PlayerWins { get => playerWins; }
    public int WinRounds { get => gameMode.winRounds; }
    public GameMode GameMode { get => gameMode; }
    public GameSprites GameSprites { get => gameMode.gameSprites; }
    public Pingpong Pingpong { get => pingPong; }
    public PaddleControls PaddleControls { get => paddleControls; }
    public string GameScene { get => gameScene; }
    [HideInInspector] public Player Player;
    public GameUI GameUI { get => gameUI; }

    public event Action OnGameWon, OnGameLost, OnOpponentScore, OnPlayerScore;

    private void Awake()
    {
        _instance = this;

        paddleControls = Instantiate(GameMode.paddleControls).GetComponent<PaddleControls>();
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
        if (playerWon)
        {
            playerHasWon = true;
            OnGameWon();
        }
        else
        {
            OnGameLost();
        }
    }

    public void AddOpponentWin()
    {
        opponentWins++;
        playedRounds++;

        OnOpponentScore();
        //Debug.Log("OPPONENT WINS: " + opponentWins.ToString());
    }

    public void AddPlayerWin()
    {
        playerWins++;
        playedRounds++;

        OnPlayerScore();
       // Debug.Log("PLAYER WINS: " + playerWins.ToString());
    }

    //use these checks in some UI script that manages the control access and animations
    public bool GameIsWon()
    {
        return playerWins == gameMode.winRounds || playerHasWon;
    }

    public bool GameIsLost()
    {
        return opponentWins == gameMode.winRounds || playerHasWon;
    }

    public void ResetGame()
    {
        playerWins = 0;
        playedRounds = 0;
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

        gameUI.SetSprites(GameSprites);
    }

    public void MoveToGameScene(GameObject go)
    {
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(gameScene));
    }
}
