using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opponent : MonoBehaviour
{
    
    [Header("Base Animation")]
    //animation
    [SerializeField] protected Animator animator;
    [Tooltip("frame that opponent hits in the animation")]
    [SerializeField] public int oppHitFrame;
    [SerializeField] protected GameObject hitFlashPrefab;

    //data on opponent behavior
    [Header("Base Behavior")]
    [SerializeField] public BallPath ballPath;
    [SerializeField] protected HitPattern hitPattern;
    [SerializeField] public Vector3 servePosition;
    [SerializeField] protected float serveTime = 2.0f;

    [Header("Optional")]
    [SerializeField] GameMode gameModeOverride;

    protected virtual void Start()
    {
        if (GameManager.Instance)
        {
            if (gameModeOverride) GameManager.Instance.SetGameMode(gameModeOverride);
            GameManager.Instance.OnGameLost += PlayWinAnimation;
            GameManager.Instance.OnGameWon += PlayLoseAnimation;
            GameManager.Instance.balls.ForEach(ball => ball.OnPlayerHit += OnPlayerHit);
        }

    }

    //defines behavior of where opponent hits based on where player hits - make custom behavior for each opponent
    virtual public Vector3 GetOpponentBallPath(float X, float Y, bool isServing)
    {
        int i = (int) Random.Range(0, hitPattern[0].size);
        return hitPattern[0][i];
    }

    virtual public void OnPlayerHit(float startX, float startY, Vector3 end, int hitFrame)
    {
        transform.position = new Vector3(startX, 0f);
    }

    virtual public void HitFlash(float X, float Y)
    {
        var hitFlash = Instantiate(hitFlashPrefab);
        hitFlash.transform.position = new Vector3(X + 0.5f, Y);
        Destroy(hitFlash, 6 / 24f);
    }

    virtual public IEnumerator PlayServeAnimation() 
    {
        yield return new WaitForSeconds(serveTime);
    }

    virtual public void PlayWinAnimation()
    {
        animator.SetTrigger("Win");
    }

    virtual public void PlayLoseAnimation()
    {
        animator.SetTrigger("Lose");
    }

    virtual public IEnumerator PlayLoseRoundAnimation()
    {
        yield return new WaitForSeconds(0f);
    }

    protected IEnumerator TweenPositionX(float startX, float endX, int frames, float frameRate = 1 / 24f)
    {
        float incX = (endX - startX) / (float) frames;   
        for (int i = 0; i < frames; i++)
        {
            animator.transform.position = new Vector3(animator.transform.position.x + incX, animator.transform.position.y);
            yield return new WaitForSeconds(frameRate);
        }
    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.OnGameLost -= PlayWinAnimation;
            GameManager.Instance.OnGameWon -= PlayLoseAnimation;
        }

    }
}
