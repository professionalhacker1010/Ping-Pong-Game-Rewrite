using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Spider : Opponent
{
    private PaddleControls paddleControls;
    //opponent data on how many balls should be on screen at a time for this pattern and where the balls should go
    [Header("Pattern Data")]
    [SerializeField] private List<MultiBall> balls; //the actual balls
    [SerializeField] private BigBall bigStar;
    [SerializeField] private List<GameObject> arms;

    [SerializeField] private List<int> ballsPerPattern; //how many balls should there be?
    [SerializeField] private List<int> offsetPerBall; //different frame between each ball for different patterns?
    [SerializeField] private List<int> patternsPerBigStar; //big star will always come at the end of a ball pattern... but how often
    [SerializeField] private List<Vector3> patternOne; //X,Y is the place spider hits the ball to, Z is the hand that hits it
    [SerializeField] private List<Vector3> patternOneBigStar;
    [SerializeField] private List<Vector3> patternTwo; //X,Y is the place spider hits the ball to, Z is the hand that hits it
    [SerializeField] private List<Vector3> patternTwoBigStar;

    private int currBall = 0, currPattern = 0;
    private List<Vector3> positions;

    [Header("Health Bar")]
    [SerializeField] private HealthBar playerHealth;
    [SerializeField] private HealthBar spiderHealth;
    [SerializeField] private float maxHealth, smallStarDamage, largeStarDamage, hitOutDamage;
    [SerializeField] private List<float> spiderHealthCheckpoints;

    [Header("Custom Ping Pong")]
    [SerializeField] private float ballSpeed;


    private void Awake()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            balls[i].ballSpeed = ballSpeed;
            balls[i].SetHealthBars(playerHealth, spiderHealth);
        }

        bigStar.ballSpeed = ballSpeed;
        bigStar.SetHealthBars(playerHealth, spiderHealth);

        //set paddle controls
        paddleControls = FindObjectOfType<PaddleControls>();
    }

    protected override void Start()
    {
        base.Start();

        //GameManager.Instance.winRounds = 1000; //so that point-based lose state never triggers
        positions = patternOne;
        bigStar.ballPositions = patternOneBigStar;

        playerHealth.Initialize(maxHealth, smallStarDamage, largeStarDamage, hitOutDamage);
        spiderHealth.Initialize(maxHealth, smallStarDamage, largeStarDamage, hitOutDamage);

        StartCoroutine(StartBalls());
    }

    private IEnumerator StartBalls()
    {
        print("start balls");
        yield return new WaitForSeconds(1f);

        //turn off all balls except the ones needed
        for (int i = 0; i < balls.Count; i++)
        {
            if (i >= ballsPerPattern[currPattern]) balls[i].gameObject.SetActive(false);
            else
            {
                balls[i].gameObject.SetActive(true);

                //pass in list of all possible locations for this ball - gonna have the multiball script handle all the ball locations...
                balls[i].ballPositions.Clear();
                for (int j = i; j < positions.Count; j+= ballsPerPattern[currPattern]) balls[i].ballPositions.Add(positions[j]);

                //make sure it's not interactable
                balls[i].thisBallInteractable = false;

                //set ball to correct arm position, animation, and layer
                int index = (int)positions[i].z - 10;
                Vector3 currArm = arms[index].transform.position;

                balls[i].currArm = currArm;
                balls[i].gameObject.transform.position = currArm;
                servePosition = currArm; //temporarily swap out serve position
                balls[i].ballAnimation.SetTrigger("opponentWaitServe");
                balls[i].SendToOrder(balls.Count - i); //set to correct ordering in layer

                //hit ball animations

                //some animation of spider charging up star ball
                StartCoroutine(ServeBall(balls[i]));

                //offset the time between hitting each ball
                yield return new WaitForSeconds(offsetPerBall[currPattern] / 24f);
                //print("ball added");
            }
        }

        //start timing the big stars
        StartCoroutine(BigStar());

        //start checking for spider damage
        StartCoroutine(CheckSpiderDamage());

        print("Start balls done");
    }

    private IEnumerator ServeBall(MultiBall ball)
    {
        yield return new WaitForSeconds(1.0f);
        StartCoroutine(ball.WaitForOpponentServe());
        ball.patternStarted = true;
    }

    private IEnumerator BigStar()
    {
        yield return new WaitForSeconds(1.0f);

        int thisPattern = currPattern;
        //set arm
        int index = (int)bigStar.ballPositions[0].z - 10;
        bigStar.currArm = arms[index].transform.position;
        bigStar.transform.position = bigStar.currArm;
        servePosition = bigStar.currArm;
        //StartCoroutine(bigStar.WaitForOpponentServe());

        while (thisPattern == currPattern)
        {
            //also play some charging up animation
            bigStar.ballAnimation.SetTrigger("opponentWaitServe");
            bigStar.patternStarted = true;
            bigStar.patternEnded = false;
            bigStar.HitBall();

            float waitTime = ((offsetPerBall[thisPattern] * ballsPerPattern[thisPattern]) + (patternsPerBigStar[thisPattern]*24f))/ 24f;
            float scaledTime = waitTime / ballSpeed;
            yield return new WaitForSeconds(scaledTime);
        }
    }

    private IEnumerator CheckSpiderDamage()
    {
        print("check spider damage");
        int thisPattern = currPattern;
        while (thisPattern == currPattern)
        {
            if (spiderHealth.Points() <= spiderHealthCheckpoints[thisPattern])
            {
                StartCoroutine(DropPaddle());
                currPattern++;
            }
            yield return new WaitForSeconds(1 / 60f);
        }
    }

    public IEnumerator DropPaddle()
    {
        print("drop paddle");
        //doesn't hit the rest of the balls left in play
        for (int i = 0; i < ballsPerPattern[currPattern]; i++)
        {
            balls[i].patternStarted = false;
            balls[i].patternEnded = false;
        }

        //drop paddle animation

        yield return new WaitForSeconds(2.0f);

        //start new pattern
        NextPattern();
        StartCoroutine(StartBalls());
    }

    public override void OnPlayerHit(float startX, float startY, Vector3 end, int hitFrame)
    {
     
    }

    private IEnumerator UpdateReorderBall(int i)
    {
        //print("update reorder ball");
        balls[i].SendToOrder(balls.Count - i); //send to back
        yield return new WaitForSeconds(ballPath.endFrame / 24f * ballSpeed);
        balls[i].SendToOrder(0 + i); //send to front
    }

    public override Vector3 GetOpponentBallPath(float X, float Y, bool isServing)
    {
        if (!isServing) StartCoroutine(UpdateReorderBall(currBall)); //set to correct ordering in layer, swap back after correct amount of time

        return base.GetOpponentBallPath(X, Y, isServing);
    }

    private void NextPattern()
    {
        foreach (var ball in balls) ball.currPosition = 0;
        bigStar.currPosition = 0;

        if (currPattern == 1)
        {
            positions = patternTwo;
            bigStar.ballPositions = patternTwoBigStar;
        }
    }

    #region
    public override void PlayLoseAnimation()
    {
    }

    public override IEnumerator PlayLoseRoundAnimation()
    {
        yield return new WaitForSeconds(0f);
    }

    public override IEnumerator PlayServeAnimation()
    {
        yield return new WaitForSecondsRealtime(0f);
    }

    public override void PlayWinAnimation()
    {
    }
    #endregion
}
