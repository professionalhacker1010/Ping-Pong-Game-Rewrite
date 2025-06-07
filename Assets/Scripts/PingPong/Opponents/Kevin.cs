using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kevin : Opponent
{
    //if 0 player wins, follow the circular pattern, which should be the first 0-5 items
    //if 1 player wins, follow the left/right pattern, which should be items 6-10
    //if 2 player wins, follow the up/down pattern, which should be items 11-14
    //if 3 player wins, follow the psuedo-random pattern, which should be items 15-29..?
    //the last hit (30) is the HARD HIT!, which goes right down the middle

    private int currHit = 0, currPattern = 0;
    private int prevKevinWins = 0;

    [Header("Kevin")]
    [SerializeField] private Vector3 kevinMisses;
    [SerializeField] private GameObject tutorialUI, holdSpace, releaseToSlam;

    public override Vector3 GetOpponentBallPath(float X, float Y, bool isServing)
    {
        //reset pattern if player lost
        if (playerLost())
        {
            currHit = 0;
        }
        //if currHit greater than the size of the Pattern, then the player has successfully played through the current pattern
        //Kevin should miss and currHit should reset to 0
        else if (currHit == hitPattern[currPattern].size)
        {
            currHit = 0;
            currPattern++;
            return kevinMisses;
        }
        return hitPattern[currPattern][currHit++]; //get the current hit, then increment
    }

    //we need a way to know when to reset the pattern - this checks if the player has lost and also updates Kevin's variables accordingly
    private bool playerLost()
    {
        if (prevKevinWins < GameManager.Instance.OpponentWins)
        {
            prevKevinWins++;
            return true;
        }
        return false;
    }

    public override void ChangeOpponentPosition(float startX, float startY, Vector3 end, int hitFrame)
    {
        //determine which animation to play
        string animation;
        if (end.z != 0) //hesitate and return out of function when end Z is not zero
        {
            animator.SetTrigger("Hesitate");
            StartCoroutine(Util.VoidCallbackTimer(2.0f, 
                () => { animator.SetTrigger("Idle"); }
                ));
            return;
        }
        else if (animator.transform.position.x <= end.x) //fronthand called when current X is less than or equal to endX
        {
            animation = "Backhand";
        }
        else //backhand called when current X is greater than endX
        {
            animation = "Fronthand";
        }

        StartCoroutine(ChangePositionRoutine(animator.transform.position.x, startX, hitFrame, animation));
    }

    private IEnumerator ChangePositionRoutine(float startX, float endX, int hitFrame, string animation)
    {
        StartCoroutine(TweenPositionX(startX, endX, 4));

        float framesUntilAnimationStart = hitFrame - oppHitFrame;
        yield return new WaitForSeconds(framesUntilAnimationStart / 24f);

        animator.SetTrigger(animation);
    }

    public override IEnumerator PlayServeAnimation()
    {
        yield return ChangePositionRoutine(animator.transform.position.x, servePosition.x, (int)(serveTime * 24), "Fronthand");
        yield return new WaitForSeconds(oppHitFrame / 24f);
    }
}
