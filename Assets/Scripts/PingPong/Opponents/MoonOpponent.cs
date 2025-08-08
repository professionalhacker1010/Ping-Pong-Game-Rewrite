using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static HealthBar;

public class MoonOpponent : Opponent
{
    [Header("Spider")]

    [SerializeField] private float ballSpeed;
    [SerializeField] List<Vector2> servePositions;
    [SerializeField] List<int> damageThresholds;
    [SerializeField] List<HitPatternInfo> hitPatternInfos;

    [System.Serializable]
    public struct HitPatternInfo
    {
        public int numBalls;
        public int framesBetweenBalls;
        public int ballsBetweenPatterns;
        public Vector2 bigStarPosition;
    }

    [Header("Health Bar")]
    [SerializeField] HealthBar opponentHealth;
    [SerializeField] HealthBar playerHealth;
    [SerializeField] private int maxOpponentHealth, maxPlayerHealth, smallStarDamage, largeStarDamage, hitOutDamage;

    private int currPattern = 0, numFinishedBalls = 0, currVolley = 0;
    private bool damageMode = false; //all successfully hit balls deal damage to opponent

    private Dictionary<int, BallInfo> ballInfo;
    class BallInfo
    {
        public bool hitByPlayer = false;
        public bool active = true;
        public int volleyId = -1;
        public bool isBigStar = false;
    }

    private void Awake()
    {
        opponentHealth.Initialize(maxOpponentHealth, smallStarDamage, largeStarDamage, hitOutDamage);
        playerHealth.Initialize(maxPlayerHealth, smallStarDamage, largeStarDamage, hitOutDamage);
    }

    protected override void StartGame()
    {
        if (GameManager.Instance)
        {
            ballInfo = new Dictionary<int, BallInfo>();
            GameManager.Instance.balls.ForEach(ball => {
                ball.OnExplode += OnBallExplode;
                var info = new BallInfo();
                info.active = false;
                ballInfo[ball.ID] = info;
                });


            //fun intro thing then start the game by calling OpponentServe on all the balls
            StartCoroutine(Intro());
        }
    }

    private IEnumerator Intro()
    {
        if (TransitionManager.Instance) yield return new WaitUntil(() => TransitionManager.Instance.isTransitioning == false);
        yield return ServeBalls();
    }

    private void Update()
    {

    }

    private IEnumerator ServeBalls()
    {
        //set up local storage of ball info
        var gameManager = GameManager.Instance;
        HitPatternInfo pattern = hitPatternInfos[currPattern];
        
        Vector3 servePos = servePositions[Random.Range(0, servePositions.Count)];
        int count = 0;
        //bool hasBigStar = (currVolley == hitPattern.patterns[currPattern].size - 1);
        bool hasBigStar = (currVolley == 1);

        for (int i = 0; i < gameManager.balls.Count; i++)
        {
            var ball = gameManager.balls[i];
            var info = ballInfo[ball.ID];

            if (!info.active && count < pattern.numBalls)
            {
                ball.ballSpeed = ballSpeed;
                BallInfo newInfo = new BallInfo();
                newInfo.volleyId = currVolley;

                int waitFrames = 0;
                if (hasBigStar)
                {

                    newInfo.isBigStar = true;
                    ball.ballAnimation.GetComponent<SpriteRenderer>().color = Color.blue;
                    waitFrames = pattern.framesBetweenBalls * 3;
                    hasBigStar = false;
                }
                else
                {
                    ball.ballAnimation.GetComponent<SpriteRenderer>().color = Color.red;
                    waitFrames = pattern.framesBetweenBalls;
                    count++;
                }

                ballInfo[ball.ID] = newInfo;

                ball.gameObject.SetActive(true);
                ball.OpponentServe(servePos);

                yield return new WaitForSeconds(waitFrames / (24f * ball.ballSpeed));
            }
        }

        yield return null;
    }

    public override Vector3 GetBallPath(int ballId, float X, float Y, bool isServing)
    {
        //if serving, return the volley location
        //otherwise signal for this script to custom handle what happens to the ball in OnHit, by setting z = 3
        if (isServing)
        {
            if (ballInfo[ballId].isBigStar) return hitPatternInfos[currPattern].bigStarPosition;
            return hitPattern.patterns[currPattern].list[currVolley];
        }
        else {
            return new Vector3(0, 0, 3);
        }
    }

    public override void OnPlayerHit(int ballId, float startX, float startY, Vector3 end, int hitFrame)
    {
        //track that the ball has been hit by player
        ballInfo[ballId].hitByPlayer = true;
    }

    public override void OnHit(int ballId, float X, float Y)
    {
        //if hit by player, disappear ball and return to pool
        if (ballInfo[ballId].hitByPlayer)
        {
            var ball = GameManager.Instance.balls[ballId];

            if (ballInfo[ballId].isBigStar || damageMode)
            {
                StartCoroutine(ball.ExplodeBall(true));
            }
            else
            {
                ball.gameObject.SetActive(false);
                ballInfo[ballId].active = false;
                if (ballInfo[ballId].volleyId == currVolley) numFinishedBalls++;
                CheckProgress();
            }

        }
        else
        {
            base.OnHit(ballId, X, Y);
        }
    }

    void OnBallExplode(int ballId, bool playerWin, bool edgeBall, bool netBall)
    {
        StartCoroutine(Util.VoidCallbackTimer(0.25f, () =>
        {
            //deal damage
            if (!playerWin)
            {
                if (edgeBall) playerHealth.Damage(DamageType.BADHIT);
                else if (ballInfo[ballId].isBigStar) playerHealth.Damage(DamageType.LARGE);
                else playerHealth.Damage(DamageType.SMALL);
            }
            else
            {
                if (ballInfo[ballId].isBigStar)
                {
                    opponentHealth.Damage(DamageType.LARGE);
                    damageMode = true;

                }
                else opponentHealth.Damage(DamageType.SMALL);
            }
        }));

    }

    protected override void OnBallFinishedExploding(int ballId, bool playerWin, bool edgeBall, bool netBall)
    {
        ballInfo[ballId].active = false;
        if (ballInfo[ballId].volleyId == currVolley && !ballInfo[ballId].isBigStar) numFinishedBalls++;

        bool gameOver = false;

        if (playerHealth.Points <= 0)
        {
            //trigger lose game stuff... or reset to last checkpoint
            print("player health is ZERO");
            GameManager.Instance.GameOver(false);
            gameOver = true;
        }
        else if (opponentHealth.Points <= 0)
        {
            //trigger win game stuff
            print("opponent health is ZERO");
            GameManager.Instance.GameOver(true);
            gameOver = true;
        }

        if (gameOver)
        {
            GameManager.Instance.balls.ForEach(ball => ball.gameObject.SetActive(false));
            return;
        }

        CheckProgress();
    }

    void CheckProgress()
    {
        //go to next volley in the pattern
        if (numFinishedBalls == hitPatternInfos[currPattern].ballsBetweenPatterns)
        {
            List<Vector3> positions = hitPattern.patterns[currPattern].list;
            currVolley = (currVolley + 1) % positions.Count;
            numFinishedBalls = 0;
            damageMode = false;
            StartCoroutine(ServeBalls());
        }

        //start next pattern
        else if (currPattern < damageThresholds.Count && opponentHealth.Points <= damageThresholds[currPattern])
        {
            currPattern++;
            currVolley = 0;
            numFinishedBalls = 0;
            damageMode = false;
            StartCoroutine(ServeBalls());
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (GameManager.Instance)
        {
            GameManager.Instance.balls.ForEach(ball => ball.OnExplode -= OnBallExplode);
        }
    }

    #region

    protected override void PlayWinAnimation()
    {
    }

    public override void PlayLoseAnimation()
    {
    }

    public override IEnumerator PlayLoseRoundAnimation()
    {
        yield return new WaitForSeconds(0f);
    }

    public override IEnumerator PlayServeAnimation()
    {
        yield return new WaitForSeconds(serveTime);
    }

    #endregion
}
