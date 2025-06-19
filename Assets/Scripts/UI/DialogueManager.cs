using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    #region
    private static DialogueManager _instance;
    public static DialogueManager Instance
    {
        get
        {
            if (_instance == null) Debug.Log("The DialogueManager is NULL");

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }
    #endregion

    //references to dialogue stuffs
    [SerializeField] DialogueRunner dialogueRunner;
    //[SerializeField] DialogueUI dialogueUI;

    public DialogueRunner DialogueRunner { get => dialogueRunner; }
    //public DialogueUI DialogueUI { get => dialogueUI; }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void StartDialogue(string startNode)
    {
        dialogueRunner.StartDialogue(startNode);
    }

    public bool DialogueRunning() {
        return dialogueRunner.IsDialogueRunning;
    }

    private void OnDestroy()
    {

    }
}
