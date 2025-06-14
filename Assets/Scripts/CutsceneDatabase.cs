using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class CutsceneDatabase : MonoBehaviour
{
    //references to dialogue stuffs
    private DialogueRunner dialogueRunner;
    private CustomVariableStorage CVStore;
    [SerializeField] private GameObject itemInputObject;
    [SerializeField] private TMPro.TMP_InputField itemInput;
    [SerializeField] private DialogueBubble dialogueBubble;

    //current assets
    [SerializeField] private SpriteRenderer currBackground;
    [SerializeField] private SpriteRenderer currSprite;
    [SerializeField] private GameObject speechBubble;

    //storage of all the cutscene assets
    private Dictionary<string, Sprite> backgrounds;

    //static variables
    public static string item = "";

    private void Start()
    {
        //get components
        dialogueRunner = FindObjectOfType<Yarn.Unity.DialogueRunner>();
        CVStore = FindObjectOfType<CustomVariableStorage>();
        //itemInput = itemInputObject.GetComponent<TMPro.TMP_InputField>();

        //update variables
        if (item != "") CVStore.SetValue("$item", item);

        //create commands for yarn
        dialogueRunner.AddCommandHandler("change_speaker", ChangeSpeaker);
        dialogueRunner.AddCommandHandler("start_speaker", StartSpeaker);
        dialogueRunner.AddCommandHandler("end_speaker", EndSpeaker);
        dialogueRunner.AddCommandHandler("change_emotion", ChangeEmotion);
        dialogueRunner.AddCommandHandler("transition_out", TransitionOut);
        dialogueRunner.AddCommandHandler("transition_in", TransitionIn);
    }

    public void ChangeSpeaker(string[] parameters, System.Action onComplete) //first string = name of speaker
    {
        StartCoroutine(ChangeSpeakerHelper(parameters, onComplete));
    }

    private IEnumerator ChangeSpeakerHelper (string[] parameters, System.Action onComplete)
    {
        yield return dialogueBubble.ChangeSpeaker(parameters[0]);
        onComplete();
    }

    public void StartSpeaker(string[] parameters, System.Action onComplete)
    {
        StartCoroutine(StartSpeakerHelper(parameters, onComplete));
    }

    private IEnumerator StartSpeakerHelper(string[] parameters, System.Action onComplete)
    {
        dialogueBubble.isChangingSpeaker = true;
        dialogueBubble.MoveBubbleToSpeaker(parameters[0]);
        yield return new WaitForSeconds(0f);
        onComplete();
    }

    public void EndSpeaker(string[] parameters, System.Action onComplete)
    {
        StartCoroutine(EndSpeakerHelper(parameters, onComplete));
    }

    private IEnumerator EndSpeakerHelper(string[] parameters, System.Action onComplete)
    {
        yield return dialogueBubble.ExitBubbleHelper();
        onComplete();
    }

    public void ChangeEmotion(string[] parameters) //0 = name of speaker, 1 = trigger     OR     0 = name of speaker, 1 = name of child object, 2 = trigger
    {
        Animator animator;
        if (parameters.Length == 3)
        {
            animator = GameObject.Find(parameters[1]).GetComponent<Animator>();
            animator.SetTrigger(parameters[2]);
        }
        else {
            animator = GameObject.Find(parameters[0]).GetComponentInChildren<Animator>();
            animator.SetTrigger(parameters[1]);
        }
    }
    public void TransitionOut(string[] parameters, System.Action onComplete)
    {
        TransitionManager.Instance.QuickOut("");
        TransitionManager.Instance.OnTransitionOut += onComplete;
    }

    public void TransitionIn(string[] parameters, System.Action onComplete)
    {
        StartCoroutine(TransitionInHelper(parameters, onComplete));
    }

    private IEnumerator TransitionInHelper(string[] parameters, System.Action onComplete)
    {
        yield return new WaitForSeconds(0.25f);

        TransitionManager.Instance.QuickIn();
        yield return new WaitForSeconds(0.25f);
        onComplete();
    }

    [YarnCommand("transition")]
    public void PlayTransition(string name)
    {
        StartCoroutine(PlayTransitionHelper(name));
    }

    private IEnumerator PlayTransitionHelper(string name)
    {
        TransitionManager.Instance.QuickOut("");
        yield return new WaitForSeconds(18 / 24f);
        currBackground.sprite = backgrounds[name];
        yield return new WaitForSeconds(0.5f);
        TransitionManager.Instance.QuickIn();
    }

    [YarnCommand("disable_bubble")]
    public void DisableBubble()
    {
        dialogueBubble.ExitBubble();
    }

    [YarnCommand("enable_bubble")]
    public void EnableBubble()
    {
        dialogueBubble.EnterBubble();
    }
}
