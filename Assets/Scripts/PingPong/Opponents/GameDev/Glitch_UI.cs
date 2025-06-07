using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glitch_UI : GameDevGlitch
{
    [SerializeField] private Vector2 minColliderPos, maxColliderPos;
    bool glitchUI = false;

    public override void TurnOn()
    {
        var playerScore = GameManager.Instance.GameUI.PlayerScoreUI;
        var opponentScore = GameManager.Instance.GameUI.OpponentScoreUI;
        StartCoroutine(GlitchUILoop(playerScore));
        StartCoroutine(GlitchUILoop(opponentScore));
        glitchUI = true;
    }

    public override void TurnOff()
    {
        glitchUI = false;
    }

    private IEnumerator GlitchUILoop(GameObject UI)
    {
        while (glitchUI)
        {
            //wait for a random amount of time to switch positions - but enough for player to react
            int wait = Random.Range(6, 12);
            Vector3 newPos = new Vector3(Random.Range(minColliderPos.x, maxColliderPos.x), Random.Range(minColliderPos.y, maxColliderPos.y));

            yield return new WaitForSeconds((wait / 24f) + 1.0f); //time it takes for ball to travel + wait time
            UI.transform.position = newPos;
            UI.transform.localScale = new Vector2(0.75f, 0.75f);
        }
    }
}
