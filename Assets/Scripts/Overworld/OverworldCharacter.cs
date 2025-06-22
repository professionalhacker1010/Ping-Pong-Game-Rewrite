using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using System;
using UnityEditor.Experimental.GraphView;

public class OverworldCharacter : MonoBehaviour, ICanInteract, IHittable
{
    //dialogue
    protected enum DialogueEvent
    {
        INTERACT,
        START,
    }
    protected enum DialogueSequenceEnd
    {
        REPEATLAST,
        END,
    }
    [System.Serializable]
    protected struct DialogueSequence
    {
        //[SerializeField] public DialogueSequenceID id;
        [SerializeField] public string nodePrefix;
        [SerializeField] public int numNodes;
        [SerializeField] public string condition;
        [SerializeField] public DialogueEvent trigger;
        [SerializeField] public DialogueSequenceEnd endBehavior;
        public string GetNodeName(int n) => nodePrefix + n.ToString();
    }

    [SerializeField] protected List<DialogueSequence> dialogueSequences;

    //interaction
    [SerializeField] protected bool turnsToPlayer, facingLeft;
    [SerializeField] protected Facing facing;

    [System.Serializable]
    protected enum Facing
    {
        LEFT,
        RIGHT,
        FORWARD
    }
    float minDistance = 2.5f; //not serializable cause I don't wanna adjust individually for each character lol

    //opponent info
    [SerializeField] protected int level;

    //interact key prompt is always a C above their heads
    protected KeyPressPrompt cKeyPrompt;
    private float cKeyPromptHeight = .5f;

    //refs
    protected CharacterControls characterControls;
    protected Collider2D collider2d;
    protected SpriteRenderer spriteRenderer;
    protected DialogueRunner dialogueRunner;

    //events
    public event Action OnHitEvent;

    [SerializeField] protected string conditionPrefix;

    //ICanInteract
    public int InteractPriority { get => 1; }
    public Vector2 InteractPos { get => transform.position; }
    public bool IsInteractable { get => isInteractable; }
    private bool isInteractable = true;

    protected virtual void Awake()
    {
        collider2d = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        dialogueRunner = FindObjectOfType<DialogueRunner>();

        foreach (var seq in dialogueSequences)
        {
            for (int i = 1; i <= seq.numNodes; i++)
            {
                string conditionName = conditionPrefix + "_" + seq.GetNodeName(i);
                Conditions.Initialize(conditionName, false);
            }
        }
    }

    protected virtual void Start()
    {
        characterControls = FindAnyObjectByType<CharacterControls>();

        cKeyPrompt = KeyPressPromptManager.Instance.GetKeyPressPrompt("C");

        if (FindNextNode(DialogueEvent.START) != "")
        {
            StartDialogue(DialogueEvent.START);
            FacePlayerForDialogue();
        }
    }

    #region Overridable functions
    public virtual void OnInteract()
    {
        float distance = transform.position.x - characterControls.transform.position.x; // - player right, + player left
        bool playerStartedLeft = distance >= 0;
        bool readjusted = false;

        //check if player overlaps speaker awkwardly and readjust -- then start dialogue after readjustment
        if (!turnsToPlayer && (Mathf.Abs(distance) < minDistance || (playerStartedLeft && facing == Facing.RIGHT) || 
            (!playerStartedLeft && facing == Facing.LEFT) || facing == Facing.FORWARD))
        {
            bool standLeft = facing == Facing.LEFT || (playerStartedLeft && facing == Facing.FORWARD);
            StartCoroutine(characterControls.ReadjustPlayer(gameObject, minDistance, standLeft, () => StartDialogue(DialogueEvent.INTERACT)));
            readjusted = true;
        }

        if (turnsToPlayer && Mathf.Abs(distance) < minDistance)
        {
            StartCoroutine(characterControls.ReadjustPlayer(gameObject, minDistance, playerStartedLeft, () => StartDialogue(DialogueEvent.INTERACT)));
            readjusted = true;
        }
        
        if (!readjusted)
        {
            StartDialogue(DialogueEvent.INTERACT);
        }

        FacePlayerForDialogue();
    }

    //turn to face player, flip back once dialogue is complete
    private void FacePlayerForDialogue()
    {
        float distance = transform.position.x - characterControls.transform.position.x; // - player right, + player left
        bool playerStartedLeft = distance >= 0;
        if (turnsToPlayer && ((playerStartedLeft && facing == Facing.RIGHT) || (!playerStartedLeft && facing == Facing.LEFT)))
        {
            StartCoroutine(Util.VoidCallbackConditional(
                    () => dialogueRunner.Dialogue.IsActive,
                    () => {
                        spriteRenderer.flipX = !spriteRenderer.flipX;
                        StartCoroutine(Util.VoidCallbackNextFrameConditional(
                            () => !dialogueRunner.Dialogue.IsActive,
                            () => { spriteRenderer.flipX = !spriteRenderer.flipX; }));
                    }
                    ));
        }
    }

    public virtual void OnSelect()
    {
        if (FindNextNode(DialogueEvent.INTERACT) == "") return;
        cKeyPrompt.Show(new Vector3(transform.position.x, collider2d.bounds.center.y + collider2d.bounds.extents.y + cKeyPromptHeight));
    }

    public virtual void OnDeselect()
    {
        cKeyPrompt.Hide();
    }

    public virtual void OnHit()
    {
        if (OnHitEvent != null) OnHitEvent();
    }

    #endregion

    #region dialogue functions
    protected void StartDialogue(DialogueEvent trigger)
    {
        //start dialogue
        string startNode = FindNextNode(trigger);
        if (startNode == "") return;

        DialogueManager.Instance.StartDialogue(conditionPrefix + "_" + startNode);
        OnDeselect();
        StartCoroutine(Util.VoidCallbackNextFrameConditional(
                () => !dialogueRunner.Dialogue.IsActive,
                () =>
                {
                    Conditions.Set(conditionPrefix + '_' + startNode, true);
                })
            );
    }

    private string FindNextNode(DialogueEvent trigger)
    {
        string startNode = "";
        for (int i = 0; i < dialogueSequences.Count; i++)
        {
            DialogueSequence seq = dialogueSequences[i];
            if (seq.trigger != trigger) continue;
            if (Conditions.Evaluate(seq.condition))
            {
                for (int j = 1; j <= seq.numNodes; j++)
                {
                    if (Conditions.Get(conditionPrefix + "_" + seq.GetNodeName(j))) continue;
                    startNode = seq.GetNodeName(j);
                    break;
                }
                if (startNode == "")
                {
                    if (seq.endBehavior == DialogueSequenceEnd.REPEATLAST) startNode = seq.GetNodeName(seq.numNodes);
                }
                break;
            }
        }
        return startNode;
    }

    protected bool IsDialoguePlayed(string nodePrefix, int seqNum)
    {
        return Conditions.Get(conditionPrefix + "_" + nodePrefix + seqNum);
    }
    #endregion
}
