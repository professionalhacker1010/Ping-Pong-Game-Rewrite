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
            if (_instance == null) Debug.Log("The LevelSelectDialogueManager is NULL");

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

    public DialogueRunner DialogueRunner { get => dialogueRunner; }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void StartDialogue(YarnProgram file, string startNode)
    {
        StartCoroutine(StartDialogueHelper(file, startNode));
    }

    private IEnumerator StartDialogueHelper(YarnProgram file, string startNode)
    {
        dialogueRunner.Clear();
        yield return new WaitForSeconds(0.05f);
        dialogueRunner.Add(file);
        dialogueRunner.StartDialogue(startNode);
    }

    public bool DialogueRunning() {
        return dialogueRunner.IsDialogueRunning;
    }

    private void OnDestroy()
    {

    }
}
