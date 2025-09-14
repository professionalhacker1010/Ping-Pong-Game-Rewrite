using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleControls : MonoBehaviour
{
    //for movement
    [SerializeField] private float normalSpeed;

    //scaling factors. for factorPaddleY/X, enter desired max adjustment in inspector (as fraction of max), code will create true factor from that
    [SerializeField] private float factorPaddleY = 0.6f, factorPaddleX = 0.4f;

    //lock
    public static bool LockedInputs
    {
        get
        {
            return locks > 0;
        }
    }
    private static int locks = 0;

    //input
    protected bool playerHitHeld = false, playerHitDown = false, playerHitUp = false;
    protected float horizontalInput, verticalInput;
    [HideInInspector] public bool inverted = false;

    //refs
    [SerializeField] private Animator swipeAnimation;
    [SerializeField] private int hitFrame = 5;
    private Animator paddlePreviewAnimation;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;

    protected virtual void Awake()
    {
        paddlePreviewAnimation = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();

        //creating true factors
        factorPaddleY = factorPaddleY * (4.85f + 5.86f) / circleCollider.radius;
        factorPaddleX = factorPaddleX * 9.5f / circleCollider.radius; //9.5 is Unity's distance from origin to sides of view
    }

    protected virtual void Start()
    {
        if (TransitionManager.Instance)
        {
            TransitionManager.Instance.OnTransitionOut += LockInputs;
            TransitionManager.Instance.OnTransitionIn += UnlockInputs;
        }
    }

    protected virtual void Update()
    {
        DetectInput();

        if (inverted)
        {
            horizontalInput *= -1;
            verticalInput *= -1;
        }

        ProcessMovement();

        if (PauseMenu.gameIsPaused || LockedInputs) return;

        ProcessInput();
    }

    protected void DetectInput()
    {
        //move paddle with keyboard
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        playerHitDown = false;
        playerHitUp = false;

        //hit ball with spacebar
        if (KeyCodes.Hit())
        {
            //Debug.Log("inputs locked: " + lockedInputs.ToString() + " locks: " + locks.ToString());
            playerHitDown = true;
            playerHitHeld = true;
        }

        if (KeyCodes.HitGetUp())
        {
            playerHitUp = true;
            playerHitHeld = false;
        }
    }

    protected void MovePaddle(float horizontalInput, float verticalInput, float speed)
    {
        // velocity calculation method to prevent bugginess while running into walls
        rb.velocity = new Vector2(horizontalInput * speed, verticalInput * speed);
    }

    protected virtual void ProcessMovement()
    {
        MovePaddle(horizontalInput, verticalInput, normalSpeed);
    }

    protected virtual void ProcessInput()
    {
        var gameManager = GameManager.Instance;
        if (playerHitDown && gameManager)
        {
            //check which ball is interactable, hit the first one and exit loop
            foreach (var ball in gameManager.balls)
            {
                if (ball.thisBallInteractable)
                {
                    TryHitBall(ball, false);
                }
            }
        }
    }

    protected void TryHitBall(Pingpong ball, bool explodeOnMiss)
    {
        if (!ball.thisBallInteractable) return;
        if (ball.circleCollider.IsTouching(circleCollider))
        {
            //Debug.Log("normal hit");

            Vector2 hitPos = ball.transform.position - circleCollider.bounds.center;
            float playerHitHeight = hitPos.y * factorPaddleY;
            float playerHitLateral = hitPos.x * factorPaddleX;

            ball.PlayerHit(playerHitHeight, playerHitLateral);

            //animations
            if (hitPos.x >= 0) HitRight();
            else HitLeft();
            StartCoroutine(WaitForHitAnimation()); //lock inputs until animation is done - prevents player from spamming space
        }
        else
        {
            //Debug.Log("You missed the ball!");
            HitLeft();
            if (explodeOnMiss) StartCoroutine(ball.ExplodeBall(false));
        }
    }

    //input locks
    #region
    public static void LockInputs()
    {
        //Debug.Log("lock inputs");
        locks++;
        //Debug.Log("locks " + locks.ToString());
    }

    public static void UnlockInputs()
    {
        //Debug.Log("unlock inputs");
        locks = Mathf.Max(locks - 1, 0);
    }

    public static void ResetLockInputs()
    {
        Debug.Log("reset inputs");
        locks = 0;
    }
    #endregion

    //hit animations
    #region
    protected void HitRight()
    {
        swipeAnimation.SetTrigger("hitRight");
        //StartCoroutine(HidePaddlePreview());
    }

    protected void HitLeft()
    {
        swipeAnimation.SetTrigger("hitLeft");
        //StartCoroutine(HidePaddlePreview());
    }

    private IEnumerator HidePaddlePreview()
    {
        yield return new WaitForSeconds(7 / 24f);
        paddlePreviewAnimation.SetTrigger("hitDone");
    }

    protected IEnumerator WaitForHitAnimation()
    {
        //Debug.Log("wait for hit");
        LockInputs();
        yield return new WaitForSeconds(hitFrame/24f); //swipe animation lasts 5 frames
        UnlockInputs();
        //Debug.Log("hit done");
    }
    #endregion

    public Vector2 GetCenter()
    {
        return circleCollider.bounds.center;
    }

    public float GetRadius()
    {
        return circleCollider.radius;
    }

    private void OnDestroy()
    {
        if (TransitionManager.Instance)
        {
            TransitionManager.Instance.OnTransitionOut -= LockInputs;
            TransitionManager.Instance.OnTransitionIn -= UnlockInputs;
        }
    }
}
