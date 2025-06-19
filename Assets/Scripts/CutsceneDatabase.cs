using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class CutsceneDatabase : MonoBehaviour
{
    //references to dialogue stuffs
    private InMemoryVariableStorage CVStore;
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
        CVStore = FindObjectOfType<InMemoryVariableStorage>();
        //itemInput = itemInputObject.GetComponent<TMPro.TMP_InputField>();

        //update variables
        if (item != "") CVStore.SetValue("$item", item);

        DialogueRunner dr = FindObjectOfType<DialogueRunner>();
        if (dr)
        {
            dr.AddCommandHandler<string>("change_speaker", ChangeSpeaker);
            dr.AddCommandHandler<string>("start_speaker", StartSpeaker);
            dr.AddCommandHandler("end_speaker", EndSpeaker);
            dr.AddCommandHandler<string,string>("change_emotion", ChangeEmotion);
            dr.AddCommandHandler("transition_out", TransitionOut);
            dr.AddCommandHandler("transition_in", TransitionIn);
            dr.AddCommandHandler<string>("transition", PlayTransition);
            dr.AddCommandHandler("disable_bubble", DisableBubble);
            dr.AddCommandHandler("enable_bubble", EnableBubble);
            dr.AddCommandHandler<int>("unlock_table", UnlockTable);
        }
    }

    public IEnumerator ChangeSpeaker (string speaker)
    {
        yield return dialogueBubble.ChangeSpeaker(speaker);
    }

    public IEnumerator StartSpeaker(string speaker)
    {
        dialogueBubble.isChangingSpeaker = true;
        dialogueBubble.MoveBubbleToSpeaker(speaker);
        yield return dialogueBubble.EnterBubbleHelper();
    }

    public IEnumerator EndSpeaker()
    {
        yield return dialogueBubble.ExitBubbleHelper();
    }

    public void ChangeEmotion(string character, string emotion) //0 = name of speaker, 1 = trigger     OR     0 = name of speaker, 1 = name of child object, 2 = trigger
    {
        Animator animator = GameObject.Find(character).GetComponentInChildren<Animator>();
        animator.SetTrigger(emotion);
    }

    public IEnumerator TransitionOut()
    {
        yield return new WaitForSeconds(0.25f);
        TransitionManager.Instance.QuickOut("");
        yield return new WaitForSeconds(0.25f);
    }

    public IEnumerator TransitionIn()
    {
        yield return new WaitForSeconds(0.25f);
        TransitionManager.Instance.QuickIn();
        yield return new WaitForSeconds(0.25f);
    }
    public IEnumerator PlayTransition(string name)
    {
        TransitionManager.Instance.QuickOut("");
        yield return new WaitForSeconds(18 / 24f);
        currBackground.sprite = backgrounds[name];
        yield return new WaitForSeconds(0.5f);
        TransitionManager.Instance.QuickIn();
    }

    public void DisableBubble()
    {
        dialogueBubble.ExitBubble();
    }

    public void EnableBubble()
    {
        dialogueBubble.EnterBubble();
    }

    public void UnlockTable(int level)
    {
        if (TableSelectManager.Instance) TableSelectManager.Instance.UnlockTable(level);
    }
}
