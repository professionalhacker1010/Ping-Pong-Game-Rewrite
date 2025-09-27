using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//add this

public class GameDev : Opponent
{
    private bool introPlayed = false;

    [Header("Base GameDev")]
    private Vector3 hitPlayerScore = new Vector3(0, 0, 2);

    // glitches
    [SerializeField] private List<int> hitsPerGlitch; //should have 4 values
    [SerializeField] GameDevGlitch uiGlitch;
    [SerializeField] List<GameDevGlitch> glitches;
    private int hits = 0, currGlitch = 0;

    //gameobjects
    [SerializeField] private GameObject buttonMash, glitchOutAnimation, opponentSprite;

    //state tracking
    private bool playerHit = false;
    private int playedRounds = 0;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //switch out to more pixellated sprites and change layering
        GameManager.Instance.GameUI.SwapToPixellated();
    }

    private void Update()
    {
        int updatedRounds = GameManager.Instance.OpponentWins + GameManager.Instance.PlayerWins;
        if (playedRounds < updatedRounds && introPlayed)
        {
            playedRounds = updatedRounds;
            if (playedRounds % 2 == 0) //player is serving
            {
                StartCoroutine(UIMoveWhilePlayerServe());
            }
        }
    }

    public override Vector3 GetBallPath(int ballId, float X, float Y, bool isServing)
    {
        var playerScore = GameManager.Instance.GameUI.PlayerScoreUI;
        var opponentScore = GameManager.Instance.GameUI.OpponentScoreUI;

        playerHit = true;
        if (!introPlayed)
        {
            Vector3 UIpos = new Vector3(X, Y);
            playerScore.transform.position = UIpos; //move player UI to wherever the ball is headed
            playerScore.transform.localScale = new Vector2(0.75f, 0.75f);

            return hitPlayerScore;
        }
        else //otherwise change the UI position
        {
            //normal UI behaviour for this opponent 
            uiGlitch.TurnOn();
            StartCoroutine(Util.VoidCallbackTimer(0.1f, () => { uiGlitch.TurnOff(); }));
        }

        Vector3 hit = new Vector3(X, Y);

        if (!isServing) //check if player hit UI only when opponent not serving
        {
            if (playerScore.GetComponent<BoxCollider2D>().OverlapPoint(hit)) //start button mash when player score gets to 7
            {
                print("hit player ui");
                if (GameManager.Instance.PlayerWins == 7)
                {
                    StartCoroutine(StartButtonMash());
                }
                else StartCoroutine(Util.VoidCallbackTimer(1.0f, () => { GameManager.Instance.AddPlayerWin(); }));
                //else return hitPlayerScore;
            }
            else if (opponentScore.GetComponent<BoxCollider2D>().OverlapPoint(hit)) //start button mash when opponent score gets to 7
            {
                print("hit opp ui");
                if (GameManager.Instance.OpponentWins >= 7) StartCoroutine(StartButtonMash());
                else StartCoroutine(Util.VoidCallbackTimer(1.0f, () => { GameManager.Instance.AddOpponentWin(); }));
            }
        }

        return base.GetBallPath(ballId, X, Y, isServing);
    }

    public override void OnPlayerHit(int ballId, float startX, float startY, Vector3 end, int hitFrame) //this is where glitches are called
    {
        if (!introPlayed)
        {
            StartCoroutine(Intro());
            introPlayed = true;
            return;
        }

        hits++;
        if (hits >= hitsPerGlitch[GameManager.Instance.PlayerWins]) //change glitch
        {
            SwitchGlitch();
        }
    }

    private IEnumerator Intro()
    {
        var pingpong = GameManager.Instance.balls[0];

        currGlitch = 0;
        glitches[currGlitch].TurnOn();
        opponentSprite.SetActive(false);
        StartCoroutine(PlayGlitchOutAnimation());

        //switch player UI into place
        yield return new WaitForSeconds(2.2f); //wait until ball reaches UI and explodes

        pingpong.Pause();
        PaddleControls.LockInputs();
        
        DialogueManager.Instance.StartDialogue("gameDev_gameIntro");
        yield return new WaitUntil(() => DialogueManager.Instance.DialogueRunning());
        yield return new WaitUntil(() => !DialogueManager.Instance.DialogueRunning());

        pingpong.Resume();
        PaddleControls.UnlockInputs();
        introPlayed = true;
        playerServing = true;
        pingpong.PlayerServe(playerServePosition);
    }

    private void SwitchGlitch()
    {
        hits = 0;
        glitches[currGlitch].TurnOff();

        int glitch = Random.Range(0, 3); //get random glitch
        if (glitch == currGlitch) currGlitch = (currGlitch + 1) % 4; //this is so that the same glitch doesn't play twice
        else currGlitch = glitch;

        StartCoroutine(PlayGlitchOutAnimation());

        print("turn on glitch " + glitches[currGlitch].gameObject.name);
        glitches[currGlitch].TurnOn();
    }

    private IEnumerator PlayGlitchOutAnimation()
    {
        glitchOutAnimation.SetActive(true);
        yield return new WaitForSeconds(20 / 30f);
        glitchOutAnimation.SetActive(false);
    }

    private IEnumerator StartButtonMash()
    {
        yield return new WaitForSeconds(23 / 24f); //wait until ball is just about to hit the UI
        yield return PlayGlitchOutAnimation();
        yield return new WaitForSeconds(0.5f);
        buttonMash.SetActive(true);

        glitches[currGlitch].TurnOff();
        PaddleControls.LockInputs(); //pause game
        GameManager.Instance.balls[0].Pause();
        var volume = FindObjectOfType<UnityEngine.Rendering.Volume>();
        if (volume) volume.gameObject.SetActive(false);
    }

    public IEnumerator UIMoveWhilePlayerServe()
    {
        //print("ui move while player serves");
        playerHit = false;
           
        uiGlitch.TurnOn();
        while (!playerHit)
        {
            yield return new WaitForEndOfFrame();
        }
        uiGlitch.TurnOff();
        //print("end ui moves while player serves");
    }

    public override IEnumerator PlayServeAnimation()
    {
        yield return new WaitForSeconds(serveTime);
        //print("play serve animation");
        uiGlitch.TurnOn();
        yield return new WaitForSeconds(1.5f);
        uiGlitch.TurnOff();
        //print("end of serve animation");
    }

    //other overrides
    #region
    public override IEnumerator PlayLoseRoundAnimation()  //kinda JANKY but using this function to move UI while player is serving
    {
        yield return null;
    }

    public override void PlayLoseAnimation()
    {
    }
    #endregion
}
