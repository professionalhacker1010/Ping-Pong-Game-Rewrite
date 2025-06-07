using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Shapeshifter : Opponent
{
    [Header("Shapeshifter")]
    [SerializeField] float startIntroWaitTime;

    //shifts through 4 different phases - hitting the weak spot makes the shapeshifter mess up, and the player progresses to next pattern
    [SerializeField] private GameObject shapeShifterObject;
    [SerializeField] private List<GameObject> phasePrefabs;
    [SerializeField] private GameObject poofPrefab;
    [SerializeField] Vector3 poofOffset = new Vector3(0f, 2.9f, 0f);

    private ShapeShifterPhase currPhase;
    private int currPattern = 0;

    protected override void Start()
    {
        base.Start();

        shapeShifterObject.SetActive(true);
        animator.SetTrigger("idle");

        TransitionManager.Instance.OnTransitionIn += ShapeShift;
        
    }

    public override Vector3 GetOpponentBallPath(float X, float Y, bool isServing)
    {
        Vector2 hit = currPhase.GetOpponentBallPath(X, Y, isServing);
        if (currPhase.isDefeated)
        {
            currPattern++;
            ShapeShift();
        }

        return hit;
    }

    public override void ChangeOpponentPosition(float startX, float startY, Vector3 end, int hitFrame)
    {
        StartCoroutine(ChangeOpponentPositionHelper(startX, startY));
    }

    private IEnumerator ChangeOpponentPositionHelper(float startX, float startY)
    {
        Debug.Log("change opp pos");
        yield return new WaitForSeconds(0.1f);

        yield return currPhase.ChangeOpponentPosition(startX, startY);
    }

    public void ShapeShift()
    {
        StartCoroutine(Shapeshift());
    }

    private IEnumerator Shapeshift()
    {
        yield return new WaitForEndOfFrame();

        Debug.Log("shapeshift");

        PaddleControls.LockInputs();

        if (currPattern == 0) //intro shapeshift animation
        {
            yield return new WaitForSeconds(startIntroWaitTime);
        }
        else //all other shapeshift animations
        {
            yield return new WaitForSeconds(2.0f); //have to wait for player's hit to reach opponent and for ball to finish exploding

            yield return ShapeshiftAnimation(currPattern - 1, -1);

            //set expression of shapeshifter here
            if (currPattern == phasePrefabs.Count) animator.SetTrigger("frown");
            else animator.SetTrigger("idle");

            yield return new WaitForSeconds(0.5f); //linger on idle pose of shifter for a little
        }

        if (currPattern < phasePrefabs.Count) //don't shift to new form after last phase
        {
            animator.SetTrigger("shapeshift"); //start shapeshift animation
            yield return new WaitForSeconds(1 / 24f);
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length - (3 / 24f)); // wait for length of animation -2 frames

            yield return ShapeshiftAnimation(-1, currPattern);
        }

        PaddleControls.UnlockInputs();
    }

    /// <summary>
    /// -1 indicates form should be base ShapeShifter
    /// </summary>
    /// <param name="startForm"></param>
    /// <param name="endForm"></param>
    /// <returns></returns>
    private IEnumerator ShapeshiftAnimation(int startForm, int endForm)
    {
        Debug.Log("Shapeshift anim");
        var poofObj = Instantiate(poofPrefab, poofOffset, Quaternion.identity);
        GameManager.Instance.MoveToGameScene(poofObj);
        yield return new WaitForSeconds(4/24f);

        //destroy current form
        if (startForm == -1) shapeShifterObject.SetActive(false);
        else Destroy(currPhase);
        currPhase = null;

        //instantiate new form
        if (endForm == -1) shapeShifterObject.SetActive(true);
        else
        {
            currPhase = Instantiate(phasePrefabs[currPattern]).GetComponent<ShapeShifterPhase>();
            GameManager.Instance.MoveToGameScene(currPhase.gameObject);
            currPhase.playerBallPath = GameManager.Instance.Player.ballPath;
        }
        
    }

    public override IEnumerator PlayServeAnimation()
    {
        //need to wait a bit longer if serving right after a shapeshift...
        yield return currPhase.PlayServeAnimation();
    }

    public override void PlayWinAnimation()
    {
    }

    public override void PlayLoseAnimation()
    {
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (TransitionManager.Instance)
        {
            TransitionManager.Instance.OnTransitionIn -= ShapeShift;
        }
    }

}
