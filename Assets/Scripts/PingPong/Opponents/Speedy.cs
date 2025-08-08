using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speedy : Opponent
{
    [SerializeField] private List<float> ballSpeeds;
    [SerializeField] private float hitOutDistance;
    [SerializeField] private Vector3 leftOut;
    [SerializeField] private Vector3 rightOut;
    //private int rounds = 0;
    private float prevX;
    //private bool left = false;
    //private bool right = false;

    protected override void Start()
    {
        base.Start();

        prevX = servePosition.x;
    }

    //speedy hits out if you hit the ball really far apart, one end then the other
    //player can tell because their hits mirror yours at a slightly wider angle ALWAYS.
    public override Vector3 GetBallPath(int ballId, float X, float Y, bool isServing)
    {
        Debug.Log((X - prevX).ToString());

        //hit out and play lose animation when player hits wide enough
        if (X - prevX >= hitOutDistance)
        {
            return leftOut; //player hit wide to the right -> opponent hits wide to the left
        }
        else if (X - prevX <= -1 * hitOutDistance)
        {
            return rightOut; //player hit wide to the left -> opponent hits wide to the right
        }
        prevX = X;

        return new Vector3(X * -1.25f, Y);
    }

    public override void OnPlayerHit(int ballId, float startX, float startY, Vector3 end, int hitFrame)
    {
        StartCoroutine(ChangeSpeedyPosition(startX, hitFrame));
    }

    public override IEnumerator PlayServeAnimation()
    {
        
        yield return ChangeSpeedyPosition(servePosition.x, (int) (serveTime * 24));
        
    }

    private IEnumerator ChangeSpeedyPosition(float startX, float hitFrame)
    {
        PaddleControls.LockInputs();
        int moveFrames = 4;
        float speed = ballSpeeds[GameManager.Instance.PlayerWins];
        float fps = 24 * speed;

        yield return new WaitForSeconds((hitFrame / fps) - (moveFrames / fps) - (oppHitFrame / 24f));


        animator.SetTrigger("move");
        yield return TweenPositionX(animator.transform.position.x, startX, moveFrames);

        //hit the ball
        animator.SetTrigger("hit");

        yield return new WaitForSeconds(oppHitFrame / 24f);

        //increase speed if needed (ie if player's score has increased)
        GameManager.Instance.balls[0].ballSpeed = ballSpeeds[GameManager.Instance.PlayerWins];
        PaddleControls.UnlockInputs();
    }

    public override IEnumerator PlayLoseRoundAnimation()
    {
        print("speedy lose round anim");
        animator.SetTrigger("energize");
        yield return new WaitForSeconds(1.5f);

        animator.SetTrigger("move");
        yield return TweenPositionX(animator.transform.position.x, servePosition.x, 3, 3 / 24f);

        animator.SetTrigger("idle");
    }
}
