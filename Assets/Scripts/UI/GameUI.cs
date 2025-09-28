using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private SpriteRenderer playerScore;
    [SerializeField] private SpriteRenderer playerBar;
    [SerializeField] private SpriteRenderer YOU;

    [SerializeField] private SpriteRenderer opponentScore;
    [SerializeField] private SpriteRenderer opponentBar;
    [SerializeField] private SpriteRenderer THEM;

    [SerializeField] private GameObject playerScoreUI, opponentScoreUI;

    [SerializeField] private List<Sprite> numbers;
    [SerializeField] private List<Sprite> bars;

    [Header("Game Result")]
    [SerializeField] private GameObject gameLost;
    [SerializeField] private GameObject gameWon;
    [SerializeField] private GameObject spaceToCont;

    [Header("Round Number")]
    [SerializeField] private UnityEngine.UI.Image number;
    [SerializeField] private GameObject roundNum;

    [Header("Animation")]
    [SerializeField] private int enlargeScoreFrames;
    private Vector3 defaultPosition, defaultPositionOpponent;
    private Vector3 defaultScale, defaultScaleOpponent;

    public GameObject PlayerScoreUI { get => playerScoreUI; }
    public GameObject OpponentScoreUI { get => opponentScoreUI; }

    private void Start()
    {
        defaultPosition = playerScoreUI.transform.localPosition;
        defaultPositionOpponent = opponentScoreUI.transform.localPosition;
        defaultScale = playerScoreUI.transform.localScale;
        defaultScaleOpponent = opponentScoreUI.transform.localScale;

        GameManager.Instance.OnGameWon += ShowGameWon;
        GameManager.Instance.OnGameLost += ShowGameLost;
        GameManager.Instance.OnOpponentScore += AddOpponentScoreboard;
        GameManager.Instance.OnPlayerScore += AddPlayerScoreboard;

        StartCoroutine(ShowRoundNumber(LevelManager.chosenOpponent + 1, 1.5f));
    }

    public void AddPlayerScoreboard()
    {
        //print("player score +1");
        int score = GameManager.Instance.PlayerWins;
        playerScore.sprite = numbers[score % 10];
        playerBar.sprite = bars[score % 5];

        //animation
        playerScoreUI.transform.localScale *= 1.2f;
        StartCoroutine(
            Util.VoidCallbackTimer(enlargeScoreFrames / 24f, 
            () => { playerScoreUI.transform.localScale = defaultScale; }));
    }

    public void AddOpponentScoreboard()
    {
        //print("opponent score +1");
        int score = GameManager.Instance.OpponentWins;
        opponentScore.sprite = numbers[score % 10];
        opponentBar.sprite = bars[score % 5];

        //animation
        opponentScoreUI.transform.localScale *= 1.2f;
        StartCoroutine(
            Util.VoidCallbackTimer(enlargeScoreFrames / 24f, 
            () => { opponentScoreUI.transform.localScale = defaultScaleOpponent; }));
    }

    public void ShowGameWon()
    {
        gameWon.SetActive(true);
        spaceToCont.SetActive(true);
    }


    public void ShowGameLost()
    {
        gameLost.SetActive(true);
        //try again or exit to menu
        spaceToCont.SetActive(true);
    }

    public IEnumerator ShowRoundNumber(int num, float time)
    {
        yield return new WaitForSeconds(0.25f);

        number.sprite = numbers[num];
        roundNum.SetActive(true);
        yield return new WaitForSeconds(time);

        roundNum.SetActive(false);

        yield return new WaitForSeconds(0.25f);

        TransitionManager.Instance.QuickIn();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.OnGameWon -= ShowGameWon;
            GameManager.Instance.OnGameLost -= ShowGameLost;
            GameManager.Instance.OnOpponentScore -= AddOpponentScoreboard;
            GameManager.Instance.OnPlayerScore -= AddPlayerScoreboard;
        }
    }
}
