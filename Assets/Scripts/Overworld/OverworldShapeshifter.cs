using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class OverworldShapeshifter : OverworldCharacter
{
    [SerializeField] private Animator shapeshiftAnimator;
    [SerializeField] private List<GameObject> phasePrefabs;
    [SerializeField] private GameObject poofPrefab;
    [SerializeField] Vector3 poofOffset = new Vector3(0f, 2.9f, 0f);
    ShapeShifterPhase currPhase;

    private int phaseIdx = 0, catForm = 0;
    private static bool interactedOnce = false;
    private static bool initialized = false;

    protected override void Start()
    {
        base.Start();

        if (!LevelManager.IsLevelPlayed(level))
        {
            shapeshiftAnimator.SetTrigger("idle");
            shapeshiftAnimator.gameObject.SetActive(false);
        }
        else
        {
            shapeshiftAnimator.SetTrigger("idle");
        }

        if (!initialized)
        {
            DialogueRunner dr = FindObjectOfType<DialogueRunner>();
            if (dr)
            {
                dr.AddCommandHandler("shapeshift", Shapeshift);
                dr.AddCommandHandler("shapeshifterIntro", Intro);
                initialized = true;
            }
        }
    }

    //shapeshift stuffs
    #region

    public IEnumerator Shapeshift()
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
            if (phaseIdx == phasePrefabs.Count - 1) currPhase.transform.position = new Vector2(transform.position.x, FindObjectOfType<CharacterControls>().transform.position.y);
            OverworldManager.Instance.MoveToGameScene(currPhase.gameObject);
            currPhase.GetComponent<SpriteRenderer>().sortingLayerID = spriteRenderer.sortingLayerID;

        }

        yield return new WaitForSeconds(15 / 24f);
    }

    public IEnumerator Intro()
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
    }

    public void Poof()
    {
        var poofObj = Instantiate(poofPrefab, shapeshiftAnimator.transform.position + poofOffset, Quaternion.identity);
        OverworldManager.Instance.MoveToGameScene(poofObj);
        poofObj.GetComponent<SpriteRenderer>().sortingLayerID = spriteRenderer.sortingLayerID;
    }
    #endregion

    public override void OnSelect()
    {
        if (!interactedOnce)
        {
            Vector3 tablePos = TableSelectManager.Instance.GetTablePosition(level);
            cKeyPrompt.Show(new Vector3(tablePos.x, tablePos.y + 2.0f));
        }
        else
        {
            base.OnSelect();
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();
        interactedOnce = true;
    }

    private IEnumerator OnTableInteractCoroutine()
    {
        shapeshiftAnimator.SetTrigger("shapeshift");
        yield return new WaitForSeconds(22 / 24f);
        shapeshiftAnimator.speed = 0f;
        Poof();
        yield return new WaitForSeconds(4 / 24f);
        shapeshiftAnimator.gameObject.SetActive(false);
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
