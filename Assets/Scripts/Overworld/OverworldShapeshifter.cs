using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldShapeshifter : OverworldCharacter
{
    [SerializeField] private Animator shapeshiftAnimator;
    [SerializeField] private List<GameObject> phasePrefabs;
    [SerializeField] private GameObject poofPrefab;
    [SerializeField] Vector3 poofOffset = new Vector3(0f, 2.9f, 0f);
    ShapeShifterPhase currPhase;

    private int phaseIdx = 0, catForm = 0;
    private static bool initialized = false;

    protected override void Start()
    {
        base.Start();
        if (!initialized && dialogueRunner)
        {
            dialogueRunner.AddCommandHandler("shapeshift", Shapeshift);
            dialogueRunner.AddCommandHandler("shapeshifterIntro", Intro);
            dialogueRunner.AddCommandHandler("shapeshifterOutro", Outro);
            initialized = true;
        }

        if (!LevelManager.IsLevelPlayed(level)) //gonna have an intro where you interact with the shapeshifter's hitbox (but you think it's the table), then the shapeshifter poofs in, then you start the game
        {
            shapeshiftAnimator.SetTrigger("idle");

            shapeshiftAnimator.gameObject.SetActive(false);
            transform.position = new Vector3(transform.position.x, 12f); //move out of the way so that the interact key prompt doesn't show up
        }
        else
        {
            shapeshiftAnimator.SetTrigger("idle");
            table.UnlockThisTable();
        }
    }

    //shapeshift stuffs
    #region
    public void Shapeshift(string[] parameters, System.Action onComplete)
    {
        StartCoroutine(ShapeshiftHelper(onComplete));
    }

    private IEnumerator ShapeshiftHelper(System.Action onComplete)
    {
        yield return new WaitForSeconds(0.5f);

        if (phaseIdx == 0)
        {
            shapeshiftAnimator.SetTrigger("shapeshift"); //start shapeshift animation
            yield return new WaitForSeconds(22 / 24f); // wait for length of animation -3 frames
        }

        //poof
        Poof();
        yield return new WaitForSeconds(4 / 24f);

        if (currPhase != null) Destroy(currPhase.gameObject);
        shapeshiftAnimator.gameObject.SetActive(true);

        //transition to shapeshifter inbetween forms
        if (phaseIdx != 0 && phaseIdx != phasePrefabs.Count - 1) 
        {

            shapeshiftAnimator.SetTrigger("shapeshift");
            yield return new WaitForSeconds(22 / 24f);

            Poof();
            yield return new WaitForSeconds(4 / 24f);

            shapeshiftAnimator.gameObject.SetActive(false);
        }

        // on next phase
        phaseIdx++;

        if (phaseIdx == 1) catForm = 0;

        //back to shapeshifter
        if (phaseIdx == phasePrefabs.Count)
        {
            phaseIdx = 0;
            shapeshiftAnimator.SetTrigger("idle");
        }
        //new form
        else
        {
            currPhase = Instantiate(phasePrefabs[phaseIdx], shapeshiftAnimator.transform.position, Quaternion.identity).GetComponent<ShapeShifterPhase>();
            if (phaseIdx == phasePrefabs.Count - 1) currPhase.transform.position = new Vector2(transform.position.x, OverworldManager.Instance.PlayerController.transform.position.y);
            OverworldManager.Instance.MoveToGameScene(currPhase.gameObject);
            currPhase.GetComponent<SpriteRenderer>().sortingLayerID = spriteRenderer.sortingLayerID;

        }

        yield return new WaitForSeconds(15 / 24f);

        onComplete();
    }

    public void Intro(string[] parameters, System.Action onComplete)
    {
        StartCoroutine(IntroHelper(onComplete));
    }

    private IEnumerator IntroHelper(System.Action onComplete)
    {
        yield return new WaitForSeconds(0.5f);

        Poof();
        yield return new WaitForSeconds(4 / 24f);

        shapeshiftAnimator.gameObject.SetActive(true);

        //chuckle 6 times
        for (int i = 0; i < 2; i++)
        {
            shapeshiftAnimator.SetTrigger("shapeshift");
            yield return new WaitForSeconds(15 / 24f);
            shapeshiftAnimator.SetTrigger("idle");
        }

        onComplete();
    }

    public void Poof()
    {
        var poofObj = Instantiate(poofPrefab, shapeshiftAnimator.transform.position + poofOffset, Quaternion.identity);
        OverworldManager.Instance.MoveToGameScene(poofObj);
        poofObj.GetComponent<SpriteRenderer>().sortingLayerID = spriteRenderer.sortingLayerID;
    }

    public void Outro(string[] parameters, System.Action onComplete)
    {
        StartCoroutine(OutroHelper(onComplete));
    }

    private IEnumerator OutroHelper(System.Action onComplete)
    {
        shapeshiftAnimator.SetTrigger("shapeshift");
        yield return new WaitForSeconds(22 / 24f);

        onComplete();
        TableSelectManager.Instance.TransitionToGame(level);
        yield return new WaitForSeconds(4 / 24f);

        shapeshiftAnimator.speed = 0f;
    }
    #endregion

    public override void OnInteract()
    {
        if (!LevelManager.IsLevelPlayed(level))
        {
            base.OnInteract();
        }
    }

    public override void OnHit()
    {
        if (phaseIdx == 1)
        {
            catForm++;
/*            if (catForm > 3)
            {
                catForm = 1;
                catAnimator.SetBool("3", false);
                catAnimator.SetBool("2", false);
                catAnimator.SetBool("1", false);
                catAnimator.SetTrigger("revert");
            }

            catAnimator.SetBool(catForm.ToString(), true);*/
        }
        else if (phaseIdx == 2)
        {

        }
        else if (phaseIdx == 3)
        {

        }
    }
}
