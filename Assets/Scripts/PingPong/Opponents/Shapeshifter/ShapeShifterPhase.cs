using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeShifterPhase : Opponent
{
    [Header("ShapeShifter Phase")]
    [SerializeField] protected Vector3 missHit;
    [SerializeField] protected List<Collider2D> weakPointColliders;

    [HideInInspector] public BallPath playerBallPath; //needed for timing animations
    [HideInInspector] public bool isDefeated = false; //defeated - should trigger next shapeshifter phase

    protected int currWeakPoint = 0;
    protected bool shouldHit = true; //should the opponent hit the player's ball or not

    public Vector3 MissHit { get => missHit; }
    public List<Collider2D> Colliders { get => weakPointColliders; }

    #region Sandbox methods
    public bool Overlaps(int collider, Vector2 hit)
    {
        return weakPointColliders[collider].OverlapPoint(hit);
    }

    protected IEnumerator WeakSpotAnimation(string trigger)
    {
        yield return new WaitForSeconds(playerBallPath.endFrame / 24f);
        animator.SetBool(trigger, true);
    }
    #endregion

    protected virtual IEnumerator HitBack(string trigger, Animator anim)
    {
        yield return new WaitForSeconds((playerBallPath.endFrame - oppHitFrame) / 24f);
        anim.SetTrigger(trigger);
    }

    public virtual IEnumerator ChangeOpponentPosition(float startX, float startY)
    {
        yield return null;
    }

    public override IEnumerator PlayServeAnimation()
    {
        yield return null;
    }

    public override Vector3 GetOpponentBallPath(float X, float Y, bool isServing)
    {
        return base.GetOpponentBallPath(X, Y, isServing);
    }
}