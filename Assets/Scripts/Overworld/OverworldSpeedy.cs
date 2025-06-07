using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldSpeedy : OverworldCharacter
{
    private static bool hadEnergyDrink = false;
    protected override void Start()
    {
        base.Start();
        //if (!LevelManager.IsLevelPlayed(level)) yarnFileCounter = 1;
        if (hadEnergyDrink) table.UnlockThisTable();
    }

    public override void OnHit()
    {
        base.OnHit();
    }

    protected override void OnOutroDialogueComplete()
    {
        base.OnOutroDialogueComplete();
    }

    protected override void StartDialogue()
    {
        //start dialogue
/*        if (LevelManager.currOpponent < level) //loops through same dialogue before you can play her
        {
            //DialogueManager.Instance.StartDialogue(preGameDialogue[0]);
            DialogueManager.Instance.StartDialogue(preGameDialogue[yarnFileCounter]);
            if (yarnFileCounter < preGameDialogue.Count - 1) yarnFileCounter++;

        }*/
/*        if (!LevelManager.IsLevelWon(level)) //true pregame dialogue starts when you can play her
        {
            DialogueManager.Instance.StartDialogue(preGameDialogue[yarnFileCounter]);
            if (yarnFileCounter < preGameDialogue.Count - 1) yarnFileCounter++;
        }
        else
        {
            DialogueManager.Instance.StartDialogue(postGameDialogue[yarnFileCounter]);
            if (yarnFileCounter < postGameDialogue.Count - 1) yarnFileCounter++;
        }*/
    }
}
