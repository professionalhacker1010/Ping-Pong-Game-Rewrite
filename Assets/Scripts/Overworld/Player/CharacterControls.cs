using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Yarn.Unity;
using UnityEngine.Events;

public class CharacterControls : MonoBehaviour
{
    //for movement
    [SerializeField] private float normalSpeed;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D rightHitBox, leftHitBox;
    [SerializeField] public CharacterPositionAdjustment characterPositionAdjustment;
    private BoxCollider2D hitBox;

    private int movementLocks = 0;
    private bool movementHalted;

    private static float spawnPos = -4.84f;

    List<IHittable> candidateHittables;
    ICanInteract prevInteractableCandidate, currInteractableCandidate;

    public event Action OnFaceLeft, OnFaceRight, OnInteract, OnHit;
    public UnityAction OnDialogueStart;
    public UnityAction OnDialogueEnd;

    public float Velocity { get => rb.velocity.x; }

    private void Awake()
    {
        hitBox = GetComponent<BoxCollider2D>();
        candidateHittables = new List<IHittable>();
        prevInteractableCandidate = null;
        currInteractableCandidate = null;
    }
    private void Start()
    {
        //spawn at the correct table, only starts when you've played your first game
        if (Conditions.Get("intro_played")) transform.position = new Vector3(spawnPos, transform.position.y);

        OnDialogueStart += () => {
            DeselectInteractable();
        };
        OnDialogueEnd += () => {
            SelectInteractable();
        };
        if (DialogueManager.Instance)
        {
            DialogueManager.Instance.DialogueRunner.onDialogueStart.AddListener(OnDialogueStart);
            DialogueManager.Instance.DialogueRunner.onDialogueComplete.AddListener(OnDialogueEnd);
        }
    }

    private void Update()
    {
        if (PauseMenu.gameIsPaused) return;

        if (DialogueManager.Instance.DialogueRunning() || movementHalted) { 
            rb.velocity = new Vector2(0f, 0f); 
            return; 
        }

        //move character at correct velocity
        //get keycode to set direction of animations
        float horizontalInput = Input.GetAxis("Horizontal");
        if (KeyCodes.Left())
        {
            FaceLeft();
        }
        else if (KeyCodes.Right())
        {
            FaceRight();
        }

        MoveCharacter(horizontalInput, normalSpeed);

        //evaluate candidates
        UpdateCandidates();

        //interacting with things
        if (KeyCodes.Interact())
        {
            Interact();
        }

        //hitting things
        if (KeyCodes.Hit())
        {
            Hit();
        }
    }

    #region movement
    public void MoveCharacter(float horizontalInput)
    {
        MoveCharacter(horizontalInput, normalSpeed);
    }

    private void MoveCharacter(float horizontalInput, float speed)
    {
        // velocity calculation method to prevent bugginess while running into walls
        float velocity = horizontalInput * speed;
        rb.velocity = new Vector2(velocity, 0.00f);

        if (horizontalInput == 0)
        {
            rb.velocity = new Vector2(0f, 0f);
        }

    }

    public void LockCharacterControls()
    {
        movementLocks++;
        movementHalted = true;
    }

    public void UnlockCharacterControls()
    {
        movementLocks--;
        if (movementLocks == 0)
            movementHalted = false;
    }

    public void FaceLeft()
    {
        leftHitBox.gameObject.SetActive(true);
        rightHitBox.gameObject.SetActive(false);

        if (OnFaceLeft != null) OnFaceLeft();
    }

    public void FaceRight()
    {
        leftHitBox.gameObject.SetActive(false);
        rightHitBox.gameObject.SetActive(true);

        if (OnFaceRight != null) OnFaceRight();
    }
    #endregion

    private void UpdateCandidates()
    {
        BoxCollider2D activeHitBox = rightHitBox.gameObject.activeSelf ? rightHitBox : leftHitBox;

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.useTriggers = true;

        List<Collider2D> results = new List<Collider2D>();
        activeHitBox.OverlapCollider(contactFilter, results);

        //hit and interactable candidates
        candidateHittables.Clear();
        currInteractableCandidate = null;
        foreach (var item in results)
        {
            IHittable hittable = item.GetComponent<IHittable>();
            if (hittable != null)
            {
                candidateHittables.Add(hittable);
            }

            ICanInteract interactable = item.GetComponent<ICanInteract>();
            if (interactable != null && interactable.IsInteractable)
            {            
                if (currInteractableCandidate == null ||
                    (interactable.InteractPriority > currInteractableCandidate.InteractPriority) ||
                    (interactable.InteractPriority == currInteractableCandidate.InteractPriority &&
                        Vector2.Distance(interactable.InteractPos, transform.position) < Vector2.Distance(currInteractableCandidate.InteractPos, transform.position)))
                {
                    currInteractableCandidate = interactable;
                }
            }
        }

        //deselect prev candidate
        if (prevInteractableCandidate != currInteractableCandidate)
        {
            DeselectInteractable();
            SelectInteractable();
        }

        prevInteractableCandidate = currInteractableCandidate;

    }

    private void Hit()
    {
        if (OnHit != null) OnHit();
        if (candidateHittables.Count > 0) candidateHittables[0].OnHit();
    }

    private void Interact()
    {
        if (OnInteract != null) OnInteract();
        if (currInteractableCandidate != null) currInteractableCandidate.OnInteract();
    }

    private void SelectInteractable() { if (currInteractableCandidate != null) currInteractableCandidate.OnSelect(); }
    private void DeselectInteractable() { if (prevInteractableCandidate != null) prevInteractableCandidate.OnDeselect(); }

    public IEnumerator ReadjustPlayer(GameObject target, float minDistance, bool standLeft, System.Action onAdjustFinished)
    {
        float horizontalInput;
        if (!standLeft) //face right and move until reaching -minDist
        {
            FaceRight();
            horizontalInput = 1f;
        }
        else // face left and move until reaching +minDist
        {
            FaceLeft();
            horizontalInput = -1f;
        }

        characterPositionAdjustment.enabled = true;
        characterPositionAdjustment.InitializeAdjustment(horizontalInput);

        if (!standLeft)
            while (transform.position.x - target.transform.position.x < minDistance) yield return new WaitForSeconds(1 / 60f);
        else
            while (target.transform.position.x - transform.position.x < minDistance) yield return new WaitForSeconds(1 / 60f);

        characterPositionAdjustment.enabled = false; //set idle

        if (!standLeft) FaceLeft();
        else FaceRight();

        if (onAdjustFinished != null) onAdjustFinished();
    }

    //check for overlapping hitboxes
    #region
    public bool OverlapsLeftHitBox(Vector2 point)
    {
        return leftHitBox.gameObject.activeSelf && leftHitBox.OverlapPoint(point);
    }

    public bool OverlapsLeftHitBox(Collider2D collider)
    {
        return leftHitBox.gameObject.activeSelf && leftHitBox.IsTouching(collider);
    }

    public bool OverlapsRightHitBox(Vector2 point)
    {
        return rightHitBox.gameObject.activeSelf && rightHitBox.OverlapPoint(point);
    }

    public bool OverlapsRightHitBox(Collider2D collider)
    {
        return rightHitBox.gameObject.activeSelf && rightHitBox.IsTouching(collider);
    }

    public bool OverlapsHitBox(Vector2 point)
    {
        return hitBox.OverlapPoint(point);
    }

    public bool OverlapsHitBox(Collider2D collider)
    {
        return hitBox.IsTouching(collider);
    }
    #endregion

    private void OnDestroy()
    {
        spawnPos = transform.position.x;
        if (DialogueManager.Instance)
        {
            DialogueManager.Instance.DialogueRunner.onDialogueStart.RemoveListener(OnDialogueStart);
            DialogueManager.Instance.DialogueRunner.onDialogueComplete.RemoveListener(OnDialogueEnd);
        }
    }
}
