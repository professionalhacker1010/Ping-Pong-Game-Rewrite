using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuteCat : ShapeShifterPhase
{
    [Header("Cat")]
    [SerializeField] Animator hitAnimator;

    public override IEnumerator PlayServeAnimation()
    {
        //need to wait a bit longer if serving right after a shapeshift...
        yield return new WaitForSeconds(serveTime - (oppHitFrame) / 24f);
        hitAnimator.SetTrigger("hitRight");
    }

    public override IEnumerator ChangeOpponentPosition(float startX, float startY)
    {
        if (shouldHit)
        {
            if (startX <= 0) StartCoroutine(HitBack("hitLeft", hitAnimator)); //play swipe animation
            else StartCoroutine(HitBack("hitRight", hitAnimator));
        }
        else
        {
            shouldHit = true;
        }

        yield return null;
    }

    public override Vector3 GetBallPath(float X, float Y, bool isServing)
    {
        Vector3 hit = new Vector3(X, Y);

        if (Overlaps(currWeakPoint, hit))
        {
            shouldHit = false;
            currWeakPoint++;
            StartCoroutine(WeakSpotAnimation(currWeakPoint.ToString())); //update animation
            if (currWeakPoint == 3) //player has booped 3 times = lose
            {
                isDefeated = true;
                return missHit;
            }
        }

        return base.GetBallPath(X, Y, isServing);
    }
}