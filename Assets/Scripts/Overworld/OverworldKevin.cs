using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldKevin : OverworldCharacter
{
    [SerializeField] Vector2 watchingVideosPosition;
    private static bool watchingVideos = false;

    //For kevin, make tables not selectable until you talk to him for the first time
    //after you beat him and the outro dialogue plays, he will sit in the corner watching youtube videos for the rest of the game

    protected override void Start()
    {
        base.Start();
        if (watchingVideos) transform.position = watchingVideosPosition;
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if (DialoguePlayed(DialogueSequenceID.PREGAME, 1) && !LevelManager.IsLevelPlayed(level))
        {
            table.UnlockThisTable();
        }
    }

    protected override string GetNextDialogue()
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

    protected override void OnOutroDialogueComplete()
    {
        base.OnOutroDialogueComplete();
        //move kevin to the corner
        transform.position = watchingVideosPosition;
        watchingVideos = true;
    }
}
