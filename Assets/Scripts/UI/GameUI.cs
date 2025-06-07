using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    private GameSprites currSprites;

    [Header("Score")]
    [SerializeField] private SpriteRenderer playerScore;
    [SerializeField] private SpriteRenderer playerBar;
    [SerializeField] private SpriteRenderer YOU;

    [SerializeField] private SpriteRenderer opponentScore;
    [SerializeField] private SpriteRenderer opponentBar;
    [SerializeField] private SpriteRenderer THEM;

    [SerializeField] private GameObject playerScoreUI, opponentScoreUI;

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
    }

    public void SetSprites(GameSprites sprites)
    {
        currSprites = sprites;

        playerScore.sprite = currSprites.numbers[0];
        opponentScore.sprite = currSprites.numbers[0];
        playerBar.sprite = currSprites.bars[0];
        opponentBar.sprite = currSprites.bars[0];
        YOU.sprite = currSprites.YOU;
        THEM.sprite = currSprites.THEM;
    }

    public void AddPlayerScoreboard()
    {
        //print("player score +1");
        int score = GameManager.Instance.PlayerWins;
        playerScore.sprite = currSprites.numbers[score % 10];
        playerBar.sprite = currSprites.bars[score % 5];

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
        opponentScore.sprite = currSprites.numbers[score % 10];
        opponentBar.sprite = currSprites.bars[score % 5];

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
        number.sprite = currSprites.numbers[num];
        roundNum.SetActive(true);
        yield return new WaitForSeconds(time);

        roundNum.SetActive(false);
    }

    public void SwapToPixellated()
    {
        playerScore.sortingLayerName = "Opponent Paddle";
        opponentScore.sortingLayerName = "Opponent Paddle";
        playerBar.sortingLayerName = "Opponent Paddle";
        opponentBar.sortingLayerName = "Opponent Paddle";
        YOU.sortingLayerName = "Opponent Paddle";
        THEM.sortingLayerName = "Opponent Paddle";
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
