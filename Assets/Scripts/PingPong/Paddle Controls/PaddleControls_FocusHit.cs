using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleControls_FocusHit : PaddleControls
{
    [SerializeField] private float focusHitSpeed;
    [SerializeField] private int focusHitMaxFrames;

    //for indicating hard hit
    [SerializeField] private GameObject hardHitIndicatorPrefab;
    HardHitIndicator hardHitIndicator;

    private float hitDownTime;
    private int hitHeldFrames = 0;

    protected override void Awake()
    {
        base.Awake();
        hitDownTime = Time.time;
        hardHitIndicator = Instantiate(hardHitIndicatorPrefab).GetComponent<HardHitIndicator>();
        GameManager.Instance.MoveToGameScene(hardHitIndicator.gameObject);
    }

    protected override void Start()
    {
        base.Start();
        GameManager.Instance.balls[0].OnExplode += ResetFocusHit;
    }

    protected override void ProcessMovement()
    {
        if (playerHitHeld) MovePaddle(horizontalInput, verticalInput, focusHitSpeed);
        else base.ProcessMovement();
    }

    protected override void ProcessInput()
    {
        var pingPong = GameManager.Instance.balls[0];
        //hit the ball
        if (playerHitUp || hitHeldFrames >= focusHitMaxFrames)
        {
            //reset any hard hit stuff
            //Debug.Log("Hard hit reset after frames: " + holdSpaceFrames.ToString());

            pingPong.ResumeGame();

            hitHeldFrames = 0;
            hardHitIndicator.StopAllCoroutines();
            hardHitIndicator.FadeToOpaque();

            TryHitBall(pingPong, true);
            StartCoroutine(WaitForHitAnimation());
        }
        else if (playerHitDown)
        {
            hitDownTime = Time.time;
            pingPong.PauseGame();
            hardHitIndicator.FadeToBlack(focusHitMaxFrames);
        }
        //focus hit: only for SPEEDY oppponent. fade to black lasts 24 frames
        else if (playerHitHeld && (Time.time - hitDownTime) > (1 / 24f))
        {
            hitDownTime = Time.time;
            hitHeldFrames++;
        }
    }

    public void ResetFocusHit()
    {
        playerHitHeld = false;
        hitHeldFrames = 0;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance && GameManager.Instance.balls[0]) GameManager.Instance.balls[0].OnExplode -= ResetFocusHit;
    }
}
