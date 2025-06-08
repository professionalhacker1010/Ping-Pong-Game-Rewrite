using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using System;

public class OverworldCharacter : MonoBehaviour, ICanInteract, IHittable
{
    //dialogue
    protected enum DialogueSequenceID
    {
        SEQ0,
        SEQ1,
        SEQ2,
        SEQ3,
        SEQ4,
        SEQ5,
        SEQ6,
        SEQ7,
        SEQ8,
        SEQ9,
        PREGAME,
        POSTGAME,
        PREGAME2,
        POSTGAME2
    }

    [System.Serializable]
    protected struct DialogueSequence
    {
        [SerializeField] public DialogueSequenceID id;
        [SerializeField] public string nodePrefix;
        [SerializeField] public int numNodes;
        public string GetNodeName(int n) => nodePrefix + n.ToString();
    }

    [SerializeField] protected YarnProgram dialogue;
    [SerializeField] protected List<DialogueSequence> dialogueSequences;

    //interaction
    [SerializeField] protected bool turnsToPlayer, facingLeft;
    float minDistance = 2.5f; //not serializable cause I don't wanna adjust individually for each character lol

    //opponent info
    [SerializeField] protected int level;

    //interact key prompt is always a C above their heads
    private KeyPressPrompt cKeyPrompt;
    private float cKeyPromptHeight = .5f;

    //refs
    protected CharacterControls characterControls;
    protected Collider2D collider2d;
    protected SpriteRenderer spriteRenderer;
    protected DialogueRunner dialogueRunner;

    //events
    public event Action OnHitEvent;

    private string conditionPrefix;

    //ICanInteract
    public int InteractPriority { get => 1; }
    public Vector2 InteractPos { get => transform.position; }
    public bool IsInteractable { get => GetIsInteractable(); }

    private void Awake()
    {
        collider2d = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        dialogueRunner = FindObjectOfType<DialogueRunner>();
        conditionPrefix = gameObject.name;

        foreach (var seq in dialogueSequences)
        {
            for (int i = 1; i <= seq.numNodes; i++)
            {
                string conditionName = conditionPrefix + "_" + seq.GetNodeName(i);
                if (!Conditions.HasCondition(conditionName))
                {
                    Conditions.SetCondition(conditionName, false);
                }
            }
        }
    }

    protected virtual void Start()
    {
        characterControls = OverworldManager.Instance.PlayerController;

        if (!IsDialoguePlayed(DialogueSequenceID.POSTGAME, 1) && LevelManager.IsLevelWon(level))
        {
            OnInteract();
        }

        cKeyPrompt = KeyPressPromptManager.Instance.GetKeyPressPrompt("C");
    }

    #region Overridable functions
    public virtual void OnInteract()
    {
        float distance = transform.position.x - characterControls.transform.position.x; // - player right, + player left
        bool playerStartedLeft = distance >= 0;
        bool readjusted = false;

        //check if player overlaps speaker awkwardly and readjust -- then start dialogue after readjustment
        if (!turnsToPlayer && (Mathf.Abs(distance) < minDistance || (playerStartedLeft && !facingLeft) || (!playerStartedLeft && facingLeft)))
        {
            StartCoroutine(characterControls.ReadjustPlayer(gameObject, minDistance, facingLeft, StartDialogue));
            readjusted = true;
        }

        if (turnsToPlayer && Mathf.Abs(distance) < minDistance)
        {
            StartCoroutine(characterControls.ReadjustPlayer(gameObject, minDistance, playerStartedLeft, StartDialogue));
            readjusted = true;
        }
        
        if (!readjusted)
        {
            StartDialogue();
        }

        //turn to face player, flip back once dialogue is complete
        if (turnsToPlayer && ((playerStartedLeft && !facingLeft) || (!playerStartedLeft && facingLeft)))
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
        cKeyPrompt.Show(new Vector3(transform.position.x, collider2d.bounds.center.y + collider2d.bounds.extents.y + cKeyPromptHeight));
    }

    public virtual void OnDeselect()
    {
        cKeyPrompt.Hide();
    }

    protected virtual string GetNextDialogue()
    {
        if (!LevelManager.IsLevelPlayed(level))
        {
            return GetNextNodeInSequence(DialogueSequenceID.PREGAME);
        }
        else
        {
            return GetNextNodeInSequence(DialogueSequenceID.POSTGAME);
        }
    }

    public virtual void OnHit()
    {
        if (OnHitEvent != null) OnHitEvent();
    }

    protected virtual bool GetIsInteractable() { return true;  }
    #endregion

    #region Sandbox dialogue functions
    protected void StartDialogue()
    {
        //start dialogue
        string startNode = GetNextDialogue();
        DialogueManager.Instance.StartDialogue(dialogue, GetNextDialogue());
        OnDeselect();
        StartCoroutine(Util.VoidCallbackNextFrameConditional(
                () => !dialogueRunner.Dialogue.IsActive,
                () =>
                {
                    Conditions.SetCondition(conditionPrefix + '_' + startNode, true);
                })
            );
    }

    protected string GetNextNodeInSequence(DialogueSequenceID id)
    {
        string nodeName = "";
        foreach (var seq in dialogueSequences)
        {
            if (seq.id != id) continue;
            for (int i = 1; i <= seq.numNodes; i++)
            {
                if (IsDialoguePlayed(id, i)) continue;
                nodeName = seq.GetNodeName(i);
                break;
            }
            if (nodeName == "") nodeName = seq.GetNodeName(seq.numNodes);
        }
        return nodeName;
    }

    protected bool IsDialoguePlayed(DialogueSequenceID id, int seqNum)
    {
        foreach (var seq in dialogueSequences)
        {
            if (seq.id != id) continue;
            return Conditions.GetCondition(conditionPrefix + "_" + seq.GetNodeName(seqNum));
        }
        Debug.Log("Invalid DialogueSequenceID or sequence number");
        return false;
    }
    #endregion
}
