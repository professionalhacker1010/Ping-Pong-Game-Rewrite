using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldKevin : OverworldCharacter
{
    [SerializeField] Vector2 watchingVideosPosition;
    private static bool watchingVideos = false;

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
        if (watchingVideos) transform.position = WatchVidoes();
    }

    void WatchVidoes()
    {
        transform.position = watchingVideosPosition;
        if (facingLeft)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();
    }
}
