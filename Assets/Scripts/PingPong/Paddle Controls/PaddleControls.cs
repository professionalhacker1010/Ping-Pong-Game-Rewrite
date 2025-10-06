using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

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
    protected CircleCollider2D circleCollider;

    //custom behavior
    [SerializeField] bool closestHitOnly;
    [SerializeField] int maxClosestHits;

    public event Action<IHittable> OnHit;

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

        if (closestHitOnly) ProcessClosestHitOnly();
        else ProcessInput();
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
        if (playerHitDown)
        {   
            bool hit = false;
            List<Collider2D> contacts = new List<Collider2D>();
            circleCollider.GetContacts(contacts);

            contacts.ForEach(contact =>
            {
                IHittable hittable = contact.gameObject.GetComponent<IHittable>();
                if (hittable != null)
                {
                    hit = true;

                    Vector2 hitPos = contact.transform.position - circleCollider.bounds.center;
                    float playerHitHeight = hitPos.y * factorPaddleY;
                    float playerHitLateral = hitPos.x * factorPaddleX;

                    hittable.OnHit(playerHitLateral, playerHitHeight);
                    if (OnHit != null) OnHit(hittable);

                    //animations
                    if (hitPos.x >= 0) HitRight();
                    else HitLeft();
                    StartCoroutine(WaitForHitAnimation()); //lock inputs until animation is done - prevents player from spamming space
                }
            });

            if (!hit)
            {
                HitLeft();
                StartCoroutine(WaitForHitAnimation());
            }
        }
    }

    void ProcessClosestHitOnly()
    {

        if (playerHitDown)
        {
            List<Collider2D> contacts = new List<Collider2D>();
            circleCollider.GetContacts(contacts);

           //filter out contacts that are overlapped by a higher layer sprite
            List<Collider2D> contactsCopy = new List<Collider2D>(contacts);
            contacts.RemoveAll(new Predicate<Collider2D>(target =>
            {
                bool overlapped = false;
                var targetSprite = target.GetComponent<SpriteRenderer>();
                targetSprite.color = Color.white;
                contactsCopy.ForEach(contact =>
                {
                    var contactSprite = contact.GetComponent<SpriteRenderer>();
                    if (contactSprite.sortingOrder > targetSprite.sortingOrder && contact.OverlapPoint(target.bounds.center))
                    {
                        overlapped = true;
                        targetSprite.color = Color.red;
                    }
                });
                return overlapped;
            }));

            //closest hittable that is not overlapped by a higher layer sprite is targeted
            contacts.Sort((a, b) =>
            {
                float distA = Vector3.Distance(circleCollider.bounds.center, a.bounds.center);
                float distB = Vector3.Distance(circleCollider.bounds.center, b.bounds.center);
                if (distA < distB) return -1;
                return 1;
            });

            int count = 0;
            for (int i = 0; i < contacts.Count; i++)
            {
                IHittable hittable = contacts[i].GetComponent<IHittable>();
                if (hittable==null) continue;

                Vector2 hitPos = contacts[i].transform.position - circleCollider.bounds.center;
                float playerHitHeight = hitPos.y * factorPaddleY;
                float playerHitLateral = hitPos.x * factorPaddleX;

                hittable.OnHit(playerHitLateral, playerHitHeight);
                if (OnHit != null) OnHit(hittable);

                //animations
                if (hitPos.x >= 0) HitRight();
                else HitLeft();
                StartCoroutine(WaitForHitAnimation()); //lock inputs until animation is done - prevents player from spamming space

                count++;
                if (count == maxClosestHits) break;
            }

            if (count == 0)
            {
                HitLeft();
                StartCoroutine(WaitForHitAnimation());
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

            ball.PlayerHit(playerHitLateral, playerHitHeight);

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
