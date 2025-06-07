using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using System;

public class OverworldCharacter : MonoBehaviour, ICanInteract, IHittable
{
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

    //dialogue
    [SerializeField] protected YarnProgram dialogue;
    [SerializeField] protected List<DialogueSequence> dialogueSequences;
    
    //protected int yarnFileCounter = 0;
    protected static bool outroDialoguePlayed = false; //automatically trigger outro dialogue when first beating the opponent

    //interaction
    [SerializeField] protected bool turnsToPlayer, facingLeft;
    float minDistance = 2.5f; //not serializable cause I don't wanna adjust individually for each character lol

    //opponent info
    [SerializeField] protected TableSelect table;
    [SerializeField] protected int level;

    //interact key prompt is always a C above their heads
    private KeyPressPrompt cKeyPrompt;
    private bool cKeyPromptSet = false;
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

    protected virtual void Start()
    {
        characterControls = OverworldManager.Instance.PlayerController;
        collider2d = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        dialogueRunner = FindObjectOfType<DialogueRunner>();
        conditionPrefix = gameObject.name;

        if (!outroDialoguePlayed && LevelManager.IsLevelWon(level))
        {
            OnInteract();

            StartCoroutine(Util.VoidCallbackNextFrameConditional(
                () => !dialogueRunner.Dialogue.IsActive,
                OnOutroDialogueComplete));
        }

        cKeyPrompt = KeyPressPromptManager.Instance.GetKeyPressPrompt("C");

        foreach (var seq in dialogueSequences)
        {
            for (int i = 1; i < seq.numNodes; i++)
            {
                string conditionName = conditionPrefix + "_" + seq.GetNodeName(i);
                if (!Conditions.HasCondition(conditionName))
                {
                    Conditions.SetCondition(conditionName, false);
                }
            }
        }
    }

    public virtual void OnInteract()
    {
        float distance = transform.position.x - characterControls.transform.position.x;

        //turn to face player
        if (turnsToPlayer)
        {
            if ((distance >= 0 && !facingLeft) || (distance < 0 && facingLeft))
            {
                FlipSprite();

                //flip back once dialogue is complete
                StartCoroutine(Util.VoidCallbackNextFrameConditional(
                    () => !dialogueRunner.Dialogue.IsActive,
                    () => { spriteRenderer.flipX = !spriteRenderer.flipX; }));
            }
        }

        //check if player overlaps speaker awkwardly and readjust -- then start dialogue after readjustment
        if (Mathf.Abs(distance) < minDistance)
        {
            StartCoroutine(characterControls.ReadjustPlayer(gameObject, minDistance, StartDialogue));
        }
        else
        {
            StartDialogue();
        }
    }

    private void FlipSprite()
    { 
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    public void OnSelect()
    {
        cKeyPrompt.Show(new Vector3(transform.position.x, collider2d.bounds.center.y + collider2d.bounds.extents.y + cKeyPromptHeight));
    }

    public void OnDeselect()
    {
        cKeyPrompt.Hide();
    }

    protected virtual void StartDialogue()
    {
        //start dialogue
        string startNode = GetNextDialogue();
        DialogueManager.Instance.StartDialogue(dialogue, GetNextDialogue());
        StartCoroutine(Util.VoidCallbackNextFrameConditional(
                () => !dialogueRunner.Dialogue.IsActive,
                () => Conditions.SetCondition(conditionPrefix + '_' + startNode, true))
            );
    }

    protected virtual string GetNextDialogue()
    {
        return "";
    }

    protected string GetNextNodeInSequence(DialogueSequenceID id)
    {
        string nodeName = "";
        foreach (var seq in dialogueSequences)
        {
            if (seq.id != id) continue;
            for (int i = 1; i < seq.numNodes; i++)
            {
                if (DialoguePlayed(id, i)) continue;
                nodeName = seq.GetNodeName(i);
                break;
            }
            if (nodeName == "") nodeName = seq.GetNodeName(seq.numNodes);
        }
        return nodeName;
    }

    protected virtual void OnOutroDialogueComplete()
    {
        outroDialoguePlayed = true;
    }

    public virtual void OnHit()
    {
        if (OnHitEvent != null) OnHitEvent();
    }

    protected bool DialoguePlayed(DialogueSequenceID id, int seqNum)
    {
        foreach (var seq in dialogueSequences)
        {
            if (seq.id != id) continue;
            return Conditions.GetCondition(conditionPrefix + "_" + seq.GetNodeName(seqNum));
        }
        Debug.LogError("Invalid DialogueSequenceID or sequence number");
        return false;
    }
}
