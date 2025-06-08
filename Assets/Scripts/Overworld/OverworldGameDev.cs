using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldGameDev : OverworldCharacter
{
    protected override string GetNextDialogue()
    {
        return base.GetNextDialogue();
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if (!LevelManager.IsLevelPlayed(level) && IsDialoguePlayed(DialogueSequenceID.PREGAME, 3) && TableSelectManager.Instance)
        {
            TableSelectManager.Instance.UnlockTable(level);
            
        }
    }
}
