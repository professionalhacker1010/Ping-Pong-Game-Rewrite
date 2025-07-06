using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Pingpong : MonoBehaviour
{
    //variables
    #region
    //colliders
    [SerializeField] public CircleCollider2D circleCollider;

    //animations
    [SerializeField] public Animator ballAnimation;
    [SerializeField] public Animator shadow;
    [SerializeField] protected Animator explodeAnimation;
    protected CameraShake cameraShaker;

    //list of tableY-trueMaxY ranges
    [SerializeField] private List<float> trueMaxY;
    [SerializeField] private List<float> tableY;
    [SerializeField] private float normalizer;

    //out of bounds values
    [SerializeField] private float edgeNet;
    //[SerializeField] private PolygonCollider2D tableCollider;
    protected bool netBall = false, edgeBall = false;

    //window of frames player has to hit back
    [SerializeField] private int hitBackFrames, hitBackLeeway;
    private int frameCount = 0; //keeps track of the frames from the opponent's hit only
    [SerializeField] private Vector3 playerServePosition;

    //public variables
    public bool playerServing = true;
    public bool thisBallInteractable = false;
    public float ballSpeed = 1.0f;

    //events
    public event Action OnExplode;

    //BPC
    protected BallPath currBallPath;
    BPC_Player bpcPlayer;
    BPC_Opponent bpcOpponent;
    BallPathInfo ballPathInfo;
    #endregion

    protected virtual void Awake()
    {
        //normalize the heights...
        for (int i = 0; i < trueMaxY.Count; i++)
        {
            trueMaxY[i] += (normalizer);
            trueMaxY[i] -= 0.3f; //moved the table down 0.3 units
        }
        for (int i = 0; i < tableY.Count; i++) tableY[i] += normalizer;
        edgeNet += normalizer;

        List<Vector2> rangeY = new List<Vector2>();
        for (int i = 0; i < tableY.Count; i++)
        {
            rangeY.Add(new Vector2(tableY[i], trueMaxY[i]));
        }
        bpcPlayer = new BPC_Player(rangeY, normalizer);
        rangeY.Reverse();
        bpcOpponent = new BPC_Opponent(rangeY, normalizer);

        ballPathInfo.x = new List<float>();
        ballPathInfo.y = new List<float>();
        for (int i = 0; i < tableY.Count; i++)
        {
            ballPathInfo.x.Add(transform.position.x);
            ballPathInfo.y.Add(transform.position.y);
        }

        cameraShaker = FindObjectOfType<CameraShake>();
        circleCollider = GetComponent<CircleCollider2D>();
    }

    protected virtual void Start()
    {
        GameManager.Instance.balls.Add(this);

        //static animation based on whos serving
        if (playerServing)
        {
            ballAnimation.SetTrigger("playerWaitServe");
        }
        else
        {
            ballAnimation.SetTrigger("opponentWaitServe");
            StartCoroutine(Util.VoidCallbackTimer(2.0f, 
                () => StartCoroutine(WaitForOpponentServe())
                ));
        }
    }

    private void Update()
    {
        //debug color
        if (!thisBallInteractable)
        {
            ballAnimation.GetComponent<SpriteRenderer>().color = Color.black;
        }
        else
        {
            ballAnimation.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    /// <summary>
    /// Hit the ball as the player
    /// </summary>
    /// <param name="playerHitHeight">How much height should the ball gain/lose</param>
    /// <param name="playerHitLateral">How much lateral adjustment to ball's end pos (other side of table) </param>
    public void PlayerHit(float playerHitHeight, float playerHitLateral)
    {
        if (!gameObject.activeInHierarchy) return;

        ballAnimation.speed = ballSpeed;
        shadow.speed = ballSpeed;

        //print("opponent X: " + opponentX.ToString() + " Y: " + opponentY.ToString());

        //PaddleControls.LockInputs();
        thisBallInteractable = false;

        StopAllCoroutines();

        currBallPath = GameManager.Instance.PlayerBallPath;
        CalcPlayerBallPath(playerHitHeight, playerHitLateral);

        //start ball animations, updating positions while animation is playing with setballpath coroutine
        shadow.SetTrigger("hitBall");
        ballAnimation.SetTrigger("hitBall");
        bool playerLose = true;
        if (netBall)
        {
            //Debug.Log("You hit a net ball!");
            SetBallPath(0, currBallPath.endFrame, playerLose);
        }
        else if (edgeBall)
        {
            //Debug.Log("You hit out of bounds!");
            SetBallPath(0, currBallPath.endFrame, playerLose);
        }
        else {
            //Debug.Log("successful hit");
            playerLose = false;
            SetBallPath(0, currBallPath.endFrame, playerLose);
            StartCoroutine(MoveOpponent(false));
        }

    }

    //Coroutines
    #region
    protected virtual void SetBallPath(int startFrame, int endFrame, bool playerLose)
    {
        StartCoroutine(SetBallPathHelper(startFrame, endFrame, playerLose));
    }
    private IEnumerator SetBallPathHelper(int startFrame, int endFrame, bool playerLose)
    {
        //print("Set ball path");
        while (startFrame < endFrame)
        {
            //translate the ball
            ballAnimation.transform.position = new Vector3(ballPathInfo.x[startFrame], ballPathInfo.y[startFrame]);
            shadow.transform.position = new Vector3(ballPathInfo.x[startFrame], shadow.transform.position.y);
            startFrame++;

            //if player hit out of bounds, explode the ball on the frame that it bounces on
            if (startFrame == currBallPath.bounceFrame && edgeBall)
            {
                //scaling size of explosion based on who hit
                if (playerLose) //then the opponent wins
                {
                    //print("out of bounds explosion");
                    explodeAnimation.transform.localScale = new Vector3(0.6f, 0.6f);
                    ExplodeBall(false); //wait for explode ball animation to finish to restart
                }
                else //otherwise the player wins
                {
                    explodeAnimation.transform.localScale = new Vector3(0.75f, 0.75f);
                    ExplodeBall(true);
                }
                startFrame = endFrame;
            }
            else if (startFrame == (currBallPath.endFrame / 2) && netBall)
            {
                explodeAnimation.transform.localScale = new Vector3(0.85f, 0.85f);
                if (playerLose) ExplodeBall(false);
                else ExplodeBall(true);
            }
            //reset lose flags
            else if (startFrame == currBallPath.endFrame - hitBackFrames && !playerLose)
            {
                netBall = false;
                edgeBall = false;
            }

            yield return new WaitForSeconds(1f / (24f * ballSpeed));
        }
    }

    private IEnumerator MoveOpponent(bool ballOut) //move opponent and call calculation of opponent ball
    {
        var opponent = GameManager.Instance.opponent;
        //Debug.Log("move opponent");

        //this is called after player hits no matter what. Opponent class will take care of what animations to play and when
        float startX = ballPathInfo.x[ballPathInfo.x.Count - 1];
        float startY = ballPathInfo.y[ballPathInfo.y.Count - 1] + normalizer;
        Vector3 opponentBallEnd = opponent.GetOpponentBallPath(startX, startY - normalizer, false);
        opponent.ChangeOpponentPosition(startX, startY - normalizer, opponentBallEnd, currBallPath.endFrame);

        yield return new WaitForSeconds(currBallPath.endFrame / (24f * ballSpeed));
        
        bool opponentHasHit = true;
        CalcOpponentBallPath(opponent, opponentHasHit, opponentBallEnd);
    }

    private IEnumerator PlayerHitBackWindow(int startFrame, int endFrame)
    {
        frameCount = startFrame;
        while (frameCount < endFrame + hitBackLeeway)
        {
            if (frameCount == endFrame - hitBackFrames)
            {
                //PaddleControls.UnlockInputs();
                thisBallInteractable = true;
                Debug.Log("Hit back window started");
            }
            frameCount++;
            yield return new WaitForSeconds(1 / (24f * ballSpeed));
        }

        if (frameCount >= endFrame + hitBackLeeway)
        {
            //PaddleControls.LockInputs();
            thisBallInteractable = false;
            Debug.Log("You're too late!");
            ExplodeBall(false);
        }
    }

    public IEnumerator WaitForOpponentServe()
    {
        print("Waitforopponentserve");
        if (TransitionManager.Instance) yield return new WaitUntil(() => TransitionManager.Instance.isTransitioning == false);

        var opponent = GameManager.Instance.opponent;

        float startX = opponent.servePosition.x;
        float startY = opponent.servePosition.y + normalizer;
        ballAnimation.transform.position = opponent.servePosition;
        Vector3 opponentBallPath = opponent.GetOpponentBallPath(startX, startY-normalizer, true);

        yield return opponent.PlayServeAnimation();

        for (int i = 0; i < tableY.Count; i++)
        {
            ballPathInfo.x[i] = startX;
            ballPathInfo.y[i] = startY;
        }
        CalcOpponentBallPath(opponent, true, opponentBallPath);
    }

    public virtual void ExplodeBall(bool playerWin)
    {
        StartCoroutine(ExplodeBallHelper(playerWin));
        if (OnExplode != null) OnExplode();
    }

    private IEnumerator ExplodeBallHelper(bool playerWin)
    {
        //Debug.Log("Explode ball");
        explodeAnimation.transform.position = ballAnimation.transform.position;
        ballAnimation.SetTrigger("explodeBall");
        explodeAnimation.SetTrigger("explodeBall");
        shadow.SetTrigger("explodeBall");
        yield return new WaitForSeconds(0.25f);

        cameraShaker.ShakeCamera(0);
        yield return new WaitForSeconds(.75f);

        //this is after the ball explosion animation is finished
        if (playerWin)
        {
            GameManager.Instance.AddPlayerWin();

            //play lose reaction between rounds
            if (!GameManager.Instance.GameIsWon())
            {
                var opponent = GameManager.Instance.opponent;
                StartCoroutine(opponent.PlayLoseRoundAnimation());
            }
        }
        else
        {
            GameManager.Instance.AddOpponentWin();
        }
        ResetRound();
        explodeAnimation.transform.localScale = new Vector3(1.0f, 1.0f); //reset explosion scale
    }

    #endregion

    public virtual void CalcOpponentBallPath(Opponent opponent, bool opponentHasHit, Vector3 opponentBallPath)
    {
        //calc
        float startX = ballPathInfo.x[ballPathInfo.x.Count - 1];
        float startY = ballPathInfo.y[ballPathInfo.y.Count - 1] + normalizer;
        bpcOpponent.CalcBallPath(opponent.ballPath, out ballPathInfo, opponentBallPath, startX, startY);

        currBallPath = opponent.ballPath;

        //opponent misses when Z = 2
        if (opponentBallPath.z == 2.0f)
        {
            explodeAnimation.transform.localScale = new Vector3(0.5f, 0.5f);
            ExplodeBall(true);
        }
        else
        {
            //play animations
            shadow.SetTrigger("opponentHitBall");
            shadow.SetTrigger("opponentHitInBounds");
            ballAnimation.SetTrigger("opponentHitBall");
            opponent.HitFlash(startX, startY - normalizer);

            //net ball
            if (opponentBallPath.z == -1.0f)
            {
                Debug.Log("Opponent hit net ball!");
                netBall = true;
                SetBallPath(0, currBallPath.endFrame, false);
            }
            //opponent htis out of bounds when z = 1
            else if (opponentBallPath.z == 1.0f)
            {
                Debug.Log("Opponent hit out of bounds!");
                edgeBall = true;
                SetBallPath(0, currBallPath.endFrame, false);
            }
            //successful hit
            else
            {
                //print("normal hit");
                SetBallPath(0, currBallPath.endFrame, true);
                StartCoroutine(PlayerHitBackWindow(0, currBallPath.endFrame));
                //update for next hit
                UpdateBallSpeed();
                
            }
        }
    }

    private void CalcPlayerBallPath(float playerHitHeight, float playerHitLateral)
    {
        //calc
        float startX = ballPathInfo.x[ballPathInfo.x.Count - 1];
        float startY = ballPathInfo.y[ballPathInfo.y.Count - 1] + normalizer;//GetScaledY(0, finalX.Count - 1, finalY[finalY.Count - 1]+normalizer);

        //update opponent X and Y values
        float opponentX = startX; //todo: does X need scaling?
        float opponentY = startY; 

        bpcPlayer.CalcBallPath(currBallPath, out ballPathInfo, playerHitHeight, playerHitLateral, startX, startY, opponentX, opponentY);

        //if ball bounces out of bounds on x2 frame
        if (!GameManager.Instance.TableCollider.OverlapPoint(new Vector2(ballPathInfo.x[currBallPath.bounceFrame], ballPathInfo.y[currBallPath.bounceFrame])))
        {
            //Debug.Log("edgeball is true");
            edgeBall = true;
        }
    }

    private void UpdateBallSpeed() //updating variables for continuing rallies
    {
        //update ball speed in case opponent changed it
        if (ballSpeed < 0) ballSpeed *= -1;
        ballAnimation.speed = ballSpeed;
        shadow.speed = ballSpeed;
    }

    //checks if anyone's won or lost the game, then triggers appropriate results. resets variables and determines server. aOnly called after someone's scored a point
    public void ResetRound()
    {
        //print("resetround");
        netBall = false;
        edgeBall = false;

        //check if game over
        bool gameIsLost = GameManager.Instance.GameIsLost();
        bool gameIsWon = GameManager.Instance.GameIsWon();
        if (gameIsLost || gameIsWon)
        {
            //PaddleControls.LockInputs();
            thisBallInteractable = false;
            ballAnimation.transform.position = new Vector3(0.0f, 0.0f);
            if (gameIsLost)
            {
                Debug.Log("You Lose!");
                GameManager.Instance.GameOver(false);
            }
            else if (gameIsWon)
            {
                Debug.Log("You Win!");
                GameManager.Instance.GameOver(true);
            }
        }

        //game continues
        else
        {
            //switch server every round
            playerServing = !playerServing;
            if (!playerServing)
            {
                ballAnimation.SetTrigger("opponentWaitServe");
                StartCoroutine(WaitForOpponentServe());
            }
            else
            {
                frameCount = tableY.Count;
                //PaddleControls.UnlockInputs();
                thisBallInteractable = true; //this should never happen tho
                ballAnimation.SetTrigger("playerWaitServe");
                ballAnimation.transform.position = playerServePosition;
                for (int i = 0; i < tableY.Count; i++)
                {
                    ballPathInfo.x[i] = playerServePosition.x;
                    ballPathInfo.y[i] = playerServePosition.y;
                }
            }
        
        }
    }

    public void Pause()
    {
        StopAllCoroutines();
        ballAnimation.speed = 0f;
        shadow.speed = 0f;
        //Debug.Log("game paused");
    }

    public void Resume()
    {
        ballAnimation.speed = ballSpeed;
        shadow.speed = ballSpeed;
        SetBallPath(frameCount, currBallPath.endFrame, false);

        //Debug.Log("game resumed");
    }
}
