using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Pingpong : MonoBehaviour
{
    //variables
    #region
    public int ID { get => id; }
    int id;

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

    //public variables
    public bool thisBallInteractable = false;
    public float ballSpeed = 1.0f;

    //private variables
    private bool paused;

    //events
    public event Action<int, bool, bool, bool> OnExplode, OnExplodeFinished;
    public event Action<int, float, float, Vector3, int> OnPlayerHit;
    public event Action<int, float, float> OnOpponentHit;

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

        id = GameManager.Instance.AddBall(this);
    }

    protected virtual void Start()
    {
    }

    private void Update()
    {
        //debug color
        if (!thisBallInteractable)
        {
            //ballAnimation.GetComponent<SpriteRenderer>().color = Color.red;
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

        thisBallInteractable = false;

        StopAllCoroutines();

        currBallPath = GameManager.Instance.PlayerBallPath;

        //calc player ball path
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
        //CalcPlayerBallPath(playerHitHeight, playerHitLateral);

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

            var opponent = GameManager.Instance.opponent;
            //Debug.Log("move opponent");

            
            float startXOpponent = ballPathInfo.x[ballPathInfo.x.Count - 1];
            float startYOpponent = ballPathInfo.y[ballPathInfo.y.Count - 1] + normalizer;
            Vector3 opponentBallEnd = opponent.GetBallPath(id, startXOpponent, startYOpponent - normalizer, false);
            if (OnPlayerHit != null) OnPlayerHit(id, startX, startY - normalizer, opponentBallEnd, currBallPath.endFrame);

            StartCoroutine(Util.VoidCallbackTimer(currBallPath.endFrame / (24f * ballSpeed),
                () => OpponentHit(opponent, opponentBallEnd)));
        }

    }

    protected virtual void OpponentHit(Opponent opponent, Vector3 opponentBallPath)
    {
        //print("OpponentHit");
        //calc
        float startX = ballPathInfo.x[ballPathInfo.x.Count - 1];
        float startY = ballPathInfo.y[ballPathInfo.y.Count - 1] + normalizer;
        bpcOpponent.CalcBallPath(opponent.ballPath, out ballPathInfo, opponentBallPath, startX, startY);

        currBallPath = opponent.ballPath;

        //opponent misses when Z = 2
        if (opponentBallPath.z == 2.0f)
        {
            StartCoroutine(ExplodeBall(true, 0.5f));
        }
        else if (opponentBallPath.z == 3.0f)
        {
            if (OnOpponentHit != null) OnOpponentHit(id, startX, startY - normalizer);
        }
        else
        {
            //play animations
            shadow.SetTrigger("opponentHitBall");
            shadow.SetTrigger("opponentHitInBounds");
            ballAnimation.SetTrigger("opponentHitBall");
            if (OnOpponentHit != null) OnOpponentHit(id, startX, startY - normalizer);
            //opponent.OnHit(startX, startY - normalizer);

            //net ball
            if (opponentBallPath.z == -1.0f)
            {
                Debug.Log("Opponent hit net ball!");
                netBall = true;
                thisBallInteractable = false;
                SetBallPath(0, currBallPath.endFrame, false);
            }
            //opponent htis out of bounds when z = 1
            else if (opponentBallPath.z == 1.0f)
            {
                Debug.Log("Opponent hit out of bounds!");
                edgeBall = true;
                thisBallInteractable = false;
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
                    print("out of bounds explosion");
                    StartCoroutine(ExplodeBall(false, 0.6f)); //wait for explode ball animation to finish to restart
                }
                else //otherwise the player wins
                {
                    StartCoroutine(ExplodeBall(true, 0.75f));
                }
                startFrame = endFrame;
            }
            else if (startFrame == (currBallPath.endFrame / 2) && netBall)
            {
                if (playerLose) StartCoroutine(ExplodeBall(false, 0.85f));
                else StartCoroutine(ExplodeBall(true, 0.85f));
            }
            //reset lose flags
            else if (startFrame == currBallPath.endFrame - hitBackFrames && !playerLose)
            {
                netBall = false;
                edgeBall = false;
            }

            yield return new WaitForSeconds(1f / (24f * ballSpeed));
            if (paused) yield return new WaitUntil(() => !paused);
        }
    }

    private IEnumerator PlayerHitBackWindow(int startFrame, int endFrame)
    {
        frameCount = startFrame;
        while (frameCount < endFrame + hitBackLeeway)
        {
            if (frameCount == endFrame - hitBackFrames)
            {
                thisBallInteractable = true;
                //Debug.Log("Hit back window started");
            }
            frameCount++;
            yield return new WaitForSeconds(1 / (24f * ballSpeed));
            if (paused) yield return new WaitUntil(() => !paused);
        }

        if (frameCount >= endFrame + hitBackLeeway)
        {
            thisBallInteractable = false;
            Debug.Log("You're too late!");
            StartCoroutine(ExplodeBall(false));
        }
    }

    public IEnumerator WaitForOpponentServe(Vector3 servePosition)
    {
        //print("Waitforopponentserve");
        if (TransitionManager.Instance) yield return new WaitUntil(() => TransitionManager.Instance.isTransitioning == false);

        var opponent = GameManager.Instance.opponent;

        float startX = servePosition.x;
        float startY = servePosition.y;
        ballAnimation.transform.position = servePosition;
        Vector3 opponentBallPath = opponent.GetBallPath(id, startX, startY-normalizer, true);

        if (paused) yield return new WaitUntil(() => !paused);
        yield return opponent.PlayServeAnimation();
        if (paused) yield return new WaitUntil(() => !paused);

        for (int i = 0; i < tableY.Count; i++)
        {
            ballPathInfo.x[i] = startX;
            ballPathInfo.y[i] = startY;
        }
        OpponentHit(opponent, opponentBallPath);
    }

    public IEnumerator ExplodeBall(bool playerWin, float explosionScale = 1.0f, int shakeStrength = 0)
    {
        if (OnExplode != null) OnExplode(id, playerWin, edgeBall, netBall);
        explodeAnimation.transform.localScale = new Vector3(explosionScale, explosionScale);
        explodeAnimation.transform.position = ballAnimation.transform.position;
        ballAnimation.SetTrigger("explodeBall");
        explodeAnimation.SetTrigger("explodeBall");
        shadow.SetTrigger("explodeBall");
        yield return new WaitForSeconds(0.25f);

        cameraShaker.ShakeCamera(shakeStrength);
        yield return new WaitForSeconds(.75f);

        if (OnExplodeFinished != null) OnExplodeFinished(id, playerWin, edgeBall, netBall);
        
        ResetRound();
    }

    #endregion


    private void UpdateBallSpeed() //updating variables for continuing rallies
    {
        //update ball speed in case opponent changed it
        if (ballSpeed < 0) ballSpeed *= -1;
        ballAnimation.speed = ballSpeed;
        shadow.speed = ballSpeed;
    }

    private void ResetRound()
    {
        //print("resetround");
        netBall = false;
        edgeBall = false;

        //check if game over
        bool gameIsLost = GameManager.Instance.GameIsLost();
        bool gameIsWon = GameManager.Instance.GameIsWon();
        if (gameIsLost || gameIsWon)
        {
            thisBallInteractable = false;
            ballAnimation.transform.position = new Vector3(0.0f, 0.0f);
        }
    }

    public void OpponentServe(Vector3 servePosition)
    {
        ballAnimation.SetTrigger("opponentWaitServe");
        StartCoroutine(WaitForOpponentServe(servePosition));
    }

    public void PlayerServe(Vector3 playerServePos)
    {
        frameCount = tableY.Count;
        thisBallInteractable = true; //this should never happen tho
        ballAnimation.SetTrigger("playerWaitServe");
        ballAnimation.transform.position = playerServePos;
        for (int i = 0; i < tableY.Count; i++)
        {
            ballPathInfo.x[i] = playerServePos.x;
            ballPathInfo.y[i] = playerServePos.y;
        }
    }

    public void Pause()
    {
        ballAnimation.speed = 0f;
        shadow.speed = 0f;
        paused = true;
        //Debug.Log("game paused");
    }

    public void Resume()
    {
        paused = false;
        ballAnimation.speed = ballSpeed;
        shadow.speed = ballSpeed;
        SetBallPath(frameCount, currBallPath.endFrame, false);
        //Debug.Log("game resumed");
    }
}
