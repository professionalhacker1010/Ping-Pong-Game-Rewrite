using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldKevin : OverworldCharacter
{
    [SerializeField] Vector2 watchingVideosPosition;
    [SerializeField] GameObject table;
    //For kevin, make tables not selectable until you talk to him for the first time
    //after you beat him and the outro dialogue plays, he will sit in the corner watching youtube videos for the rest of the game

    protected override void Awake()
    {
        base.Awake();

        Conditions.Initialize(conditionPrefix + "_watchingVideos", false);
    }

    protected override void Start()
    {
        base.Start();
        if (Conditions.Get(conditionPrefix + "_watchingVideos")) WatchVideos();
    }

    [Yarn.Unity.YarnCommand]
    public void WatchVideos()
    {
        transform.position = watchingVideosPosition;
        if (facing == Facing.LEFT)
        {
            facing = Facing.RIGHT;
            turnsToPlayer = false;
            spriteRenderer.flipX = !spriteRenderer.flipX;
            QuickGameTrigger qgt = table.GetComponent<QuickGameTrigger>();
            qgt.enabled = true;
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();
    }
}
