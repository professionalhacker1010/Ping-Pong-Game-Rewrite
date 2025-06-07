using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drums : ShapeShifterPhase
{
    [Header("Drums")]
    [Tooltip("drum hit length in frames")]
    [SerializeField] private int drumHitFrames;

    protected override IEnumerator HitBack(string trigger, Animator anim)
    {
        yield return base.HitBack(trigger, anim);

        yield return new WaitForSeconds(drumHitFrames / 24f);
        anim.SetTrigger("hitDone");
    }

    public override IEnumerator ChangeOpponentPosition(float startX, float startY)
    {
        if (shouldHit)
        {
            //beat the correct drum
            for (int i = 0; i < weakPointColliders.Count; i++)
            {
                Vector3 hit = new Vector3(startX, startY);
                if (Overlaps(i, hit))
                {
                    StartCoroutine(HitBack(("drum" + (i + 1).ToString()), animator));
                    break;
                }
            }
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
        animator.SetTrigger("drum4");
    }

    public override Vector3 GetOpponentBallPath(float X, float Y, bool isServing)
    {
        Vector2 hit = new Vector2(X, Y);

        if ((currWeakPoint <= 1 && (Overlaps(0, hit) || Overlaps(1, hit))) ||
                (currWeakPoint == 2 && (Overlaps(2, hit) || Overlaps(4, hit) || Overlaps(5, hit))))
        {
            currWeakPoint++;
            StartCoroutine(WeakSpotAnimation(currWeakPoint.ToString())); //update animation
        }
        else //reset drums
        {
            currWeakPoint = 0;
            animator.SetBool("3", false); animator.SetBool("2", false); animator.SetBool("1", false);
            StartCoroutine(HitBack("hitDone", animator));
        }

        if (currWeakPoint == 3)
        {
            shouldHit = false;
            isDefeated = false;
            return missHit;
        }

        return base.GetOpponentBallPath(X, Y, isServing);
    }
}
