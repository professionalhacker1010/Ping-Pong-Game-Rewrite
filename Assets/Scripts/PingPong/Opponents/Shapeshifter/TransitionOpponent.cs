using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionOpponent : ShapeShifterPhase
{
    [Header("Transition Opponent")]
    [SerializeField] private List<Vector3> transitionCircleTransforms; //ordered from lowest X to highest X
    private int currTransitionCircleTransform = 1;

    public override IEnumerator ChangeOpponentPosition(float startX, float startY)
    {
        if (shouldHit)
        {
            Debug.Log("set trigger in");
            animator.SetTrigger("In");
            animator.SetTrigger("Out");
        }
        else
        {
            shouldHit = true;
        }

        yield return null;
    }

    public override IEnumerator PlayServeAnimation()
    {
        yield return new WaitForSeconds(serveTime - (oppHitFrame) / 24f);
        animator.SetTrigger("In");
        animator.SetTrigger("Out");
    }

    public override Vector3 GetOpponentBallPath(float X, float Y, bool isServing)
    {
        int prevTransform = currTransitionCircleTransform;
        Vector2 hit = new Vector2(X, Y);

        transform.position = transitionCircleTransforms[prevTransform];

        //check if player hit circle collider
        if (Overlaps(currWeakPoint, hit))
        {
            return missHit;
        }

        //the transition circle moves to the opposite side of the ball - ball on left side of table -> circle on right side and vice versa
        //y value is randomly picked from predeterminedHits
        else if (X <= 0f)
        {
            currTransitionCircleTransform++;
            if (currTransitionCircleTransform >= transitionCircleTransforms.Count) currTransitionCircleTransform = transitionCircleTransforms.Count - 1;
        }
        else
        {
            currTransitionCircleTransform--;
            if (currTransitionCircleTransform < 0) currTransitionCircleTransform = 0;
        }

        return base.GetOpponentBallPath(X, Y, isServing);
    }
}
