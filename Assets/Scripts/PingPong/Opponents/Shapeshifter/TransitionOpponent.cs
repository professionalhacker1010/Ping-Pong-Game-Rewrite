using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionOpponent : ShapeShifterPhase
{
    [Header("Transition Opponent")]
    [SerializeField] private List<Vector3> transitionCircleTransforms; //ordered from lowest X to highest X
    private int currTransitionCircleTransform = 1;
    private int direction = 1;

    public override IEnumerator ChangeOpponentPosition(float startX, float startY)
    {
        if (shouldHit)
        {
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
        Vector3 hit = new Vector3(X, Y);

        //the transition circle moves to the opposite side of the ball - ball on left side of table -> circle on right side and vice versa
        //y value is randomly picked from predeterminedHits
        if (currTransitionCircleTransform == 0)
        {
            direction = 1;
        }
        else if (currTransitionCircleTransform == transitionCircleTransforms.Count - 1)
        {
            direction = -1;
        }
        currTransitionCircleTransform += direction;
        transform.position = transitionCircleTransforms[currTransitionCircleTransform];

        //check if player hit circle collider
        if (Vector2.Distance(hit, transform.position) < 1.0f)
        {
            isDefeated = true;
            return missHit;
        }

        return base.GetOpponentBallPath(X, Y, isServing);
    }
}
