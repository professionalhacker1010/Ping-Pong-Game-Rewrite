using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//virtual functions:
//override protected void Start() : Setup subscriptions
//override protected void StartGame() : Game-related behavior for starting
//override public Vector3 GetBallPath(float X, float Y, bool isServing) : Logic for where opponent hits next, if they hit successfully or not. Called by Pingpong
//override public void OnPlayerHit(int ballId, float startX, float startY, Vector3 end, int hitFrame) : Most likely for animating opponent hitting the ball
//override public void OnHit(int ballId, float X, float Y) : Invoked by Pinpong after player hits and ball reaches opponent, or when Pingpong::OpponentServe called
//override protected void OnBallFinishedExploding(int ballId, bool playerWin, bool edgeBall, bool netBall) : Determine scoring and win states
//override protected void OnDestroy() : Unsubscribe
//
//sandbox functions:
//IEnumerator TweenPositionX(float startX, float endX, int frames, float frameRate = 1 / 24f)

public class Opponent : MonoBehaviour
{
    
    [Header("Base Animation")]
    //animation
    [SerializeField] protected Animator animator;
    [Tooltip("frame that opponent hits in the animation")]
    [SerializeField] public int oppHitFrame;
    [SerializeField] protected GameObject hitFlashPrefab;

    //data on opponent behavior
    [Header("Base Behavior")]
    [SerializeField] public BallPath ballPath;
    [SerializeField] protected HitPattern hitPattern;
    [SerializeField] public Vector3 servePosition;
    [SerializeField] protected float serveTime = 2.0f;
    [SerializeField] protected bool playerServing = true;
    [SerializeField] protected Vector3 playerServePosition;

    [Header("Optional")]
    [SerializeField] protected GameMode gameModeOverride;

    protected virtual void Start()
    {
        if (GameManager.Instance)
        {
            var gameManager = GameManager.Instance;
            if (gameModeOverride) gameManager.SetGameMode(gameModeOverride);
            gameManager.OnGameLost += PlayWinAnimation;
            gameManager.OnGameWon += PlayLoseAnimation;
            gameManager.balls.ForEach(b => b.OnPlayerHit += OnPlayerHit);
            gameManager.balls.ForEach(b => b.OnOpponentHit += OnHit);
            gameManager.balls.ForEach(b => b.OnExplodeFinished += OnBallFinishedExploding);
        }


        StartGame();
    }

    virtual protected void StartGame()
    {
        if (GameManager.Instance)
        {
            var gameManager = GameManager.Instance;
            if (!playerServing)
            {
                gameManager.balls[0].OpponentServe(servePosition);
            }
            else
            {
                gameManager.balls[0].PlayerServe(playerServePosition);
            }
        }
    }

    //defines behavior of where opponent hits based on where player hits - make custom behavior for each opponent
    virtual public Vector3 GetBallPath(int ballId, float X, float Y, bool isServing)
    {
        int i = (int) Random.Range(0, hitPattern[0].size);
        return hitPattern[0][i];
    }

    virtual public void OnPlayerHit(int ballId, float startX, float startY, Vector3 end, int hitFrame)
    {
        transform.position = new Vector3(startX, 0f);
    }

    virtual public void OnHit(int ballId, float X, float Y)
    {
        var hitFlash = Instantiate(hitFlashPrefab);
        hitFlash.transform.position = new Vector3(X + 0.5f, Y);
        Destroy(hitFlash, 6 / 24f);
    }

    virtual protected void OnBallFinishedExploding(int ballId, bool playerWin, bool edgeBall, bool netBall)
    {
        var gameManager = GameManager.Instance;
        if (playerWin)
        {
            gameManager.AddPlayerWin();

            //play lose reaction between rounds
            if (!gameManager.GameIsWon())
            {
                PlayLoseRoundAnimation();
            }
        }
        else
        {
            gameManager.AddOpponentWin();
        }

        //check if game over
        bool gameIsLost = gameManager.GameIsLost();
        bool gameIsWon = gameManager.GameIsWon();
        if (gameIsLost || gameIsWon)
        {
            if (gameIsLost)
            {
                Debug.Log("You Lose!");
                gameManager.GameOver(false);
            }
            else if (gameIsWon)
            {
                Debug.Log("You Win!");
                gameManager.GameOver(true);
            }
        }
        //game continues
        else
        {
            //switch server every round
            playerServing = !playerServing;
            if (!playerServing)
            {
                gameManager.balls[ballId].OpponentServe(servePosition);
            }
            else
            {
                gameManager.balls[ballId].PlayerServe(playerServePosition);
            }

        }
    }

    virtual public IEnumerator PlayServeAnimation() 
    {
        yield return new WaitForSeconds(serveTime);
    }

    virtual protected void PlayWinAnimation()
    {
        if (animator) animator.SetTrigger("Win");
    }

    virtual public void PlayLoseAnimation()
    {
        if (animator) animator.SetTrigger("Lose");
    }

    virtual public IEnumerator PlayLoseRoundAnimation()
    {
        yield return new WaitForSeconds(0f);
    }

    protected IEnumerator TweenPositionX(float startX, float endX, int frames, float frameRate = 1 / 24f)
    {
        float incX = (endX - startX) / (float) frames;   
        for (int i = 0; i < frames; i++)
        {
            if (animator) animator.transform.position = new Vector3(animator.transform.position.x + incX, animator.transform.position.y);
            yield return new WaitForSeconds(frameRate);
        }
    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance)
        {
            var gameManager = GameManager.Instance;
            gameManager.OnGameLost -= PlayWinAnimation;
            gameManager.OnGameWon -= PlayLoseAnimation;
            gameManager.balls.ForEach(b => b.OnPlayerHit -= OnPlayerHit);
            gameManager.balls.ForEach(b => b.OnOpponentHit -= OnHit);
            gameManager.balls.ForEach(b => b.OnExplodeFinished -= OnBallFinishedExploding);
        }

    }
}
