using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBall : Pingpong
{
    [SerializeField] private int starType = 0;
    [SerializeField] public HealthBar.DamageType damageType;

    //defining some custom behaviour for level 5
    private SpriteRenderer spriteRenderer;
    [HideInInspector] public Vector3 currArm;

    //store places the ball hits to
    [HideInInspector] public List<Vector3> ballPositions;
    public int currPosition = 0;

    //DEBUG
    private bool colored = false;

    //keeping the hit times consistent
    public bool patternStarted = false, patternEnded = true;
    protected float lastOpponentHitTime, defaultSpeed = 1f;

    //health bars
    private HealthBar playerHealth, opponentHealth;

    private bool endExplosionPlaying = false;
    protected bool opponentShouldHit = false;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        ballPositions = new List<Vector3>();
        if (starType == 0) damageType = HealthBar.DamageType.SMALL;
        else if (starType == 1) damageType = HealthBar.DamageType.LARGE;
    }

    protected override void Start()
    {
        base.Start();
        defaultSpeed = ballSpeed;
    }

    public void SetHealthBars(HealthBar playerH, HealthBar opponentH)
    {
        playerHealth = playerH; opponentHealth = opponentH;
    }

    private void Update()
    {
        if (thisBallInteractable && colored)
        {
            spriteRenderer.color = Color.white;
            colored = false;
        }
        else if (!thisBallInteractable && !colored)
        {
            colored = true;
            if (damageType == HealthBar.DamageType.SMALL) spriteRenderer.color = Color.red;
            else spriteRenderer.color = Color.blue;
        }

        HitBehaviour();
    }

    protected virtual void HitBehaviour()
    {
        if (Time.time - lastOpponentHitTime >= 2.0f / defaultSpeed)
        {
            if (patternStarted)
            {
                lastOpponentHitTime = Time.time;

                transform.position = currArm;
                opponentShouldHit = true;
                OpponentHit(GameManager.Instance.opponent, ballPositions[currPosition]);
                currPosition = (currPosition + 1) % ballPositions.Count;
            }
            else if (!patternEnded)
            {
                patternEnded = true;
                endExplosionPlaying = true;
                StartCoroutine(ExplodeBall(false, 0.5f, 1));
            }
        }
    }

    public void SendToOrder(int i)
    {
        spriteRenderer.sortingOrder = i;
    }

    private IEnumerator WaitForPlayIn(float waitTime)
    {
        yield return new WaitForSeconds(0.2f / ballSpeed);

        cameraShaker.ShakeCamera(0);

        //play some animation of spider girl grabbing a star to put into play, then waits for command from opponent script to throw it into play
        ballAnimation.SetTrigger("opponentWaitServe");
        ballAnimation.transform.position = currArm;

        yield return new WaitForSeconds((waitTime - .2f) / ballSpeed);

/*        startX = currArm.x;
        startY = currArm.y + 5.6f;*/
        edgeBall = false;
        netBall = false;
    }

    //every time setBall Path is called from the opponent, check how much time has elapsed.. temporarily adjust the ball speed so that the spacing between hits is the same
/*    protected override void SetBallPath(int startFrame, int endFrame, bool playerLose)
    {
        //print("Set ball path");
        float elapsedTime = Time.time - lastOpponentHitTime;
        if ((elapsedTime < 1f / defaultSpeed && elapsedTime > 0.2f)|| elapsedTime > 1f / defaultSpeed) //called for player hit only = they might hit a few frames late / early. late = faster ball, early = slower ball
        {
            float desiredTime = (2f / defaultSpeed) - elapsedTime;;
            float tempSpeed = defaultSpeed / desiredTime;
            ballSpeed = tempSpeed;

            //print("player hit after time: " + elapsedTime.ToString());
        }

        base.SetBallPath(startFrame, endFrame, playerLose);
    }*/

    protected void SetBallPath2(int startFrame, int endFrame, bool playerLose)
    {
        base.SetBallPath(startFrame, endFrame, playerLose);
    }

    protected override void OpponentHit(Opponent opponent, Vector3 opponentBallPath)
    {
        if (opponentShouldHit) base.OpponentHit(opponent, opponentBallPath);
        opponentShouldHit = false;
    }
}
