using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class OverworldShapeshifter : OverworldCharacter
{
    [SerializeField] private Animator shapeshiftAnimator;
    [SerializeField] private List<GameObject> phasePrefabs;
    [SerializeField] private GameObject poofPrefab;
    GameObject currPhase;

    private int phaseIdx = -1;

    protected override void Awake()
    {
        base.Awake();
        Conditions.Initialize(conditionPrefix + "_interactedOnce", false);
    }

    protected override void Start()
    {
        base.Start();

        if (!Conditions.Get(conditionPrefix + "_interactedOnce"))
        {
            shapeshiftAnimator.SetTrigger("idle");
            shapeshiftAnimator.gameObject.SetActive(false);
        }
        else
        {
            shapeshiftAnimator.SetTrigger("idle");
        }

        DialogueRunner dr = FindObjectOfType<DialogueRunner>();
        if (dr)
        {
            dr.AddCommandHandler("shapeshift", Shapeshift);
            dr.AddCommandHandler("shapeshifterIntro", Intro);
        }
    }

    //shapeshift stuffs
    #region

    public IEnumerator Shapeshift()
    {
        yield return new WaitForSeconds(0.5f);

        if (phaseIdx == -1)
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
        if (phaseIdx != -1 && phaseIdx != phasePrefabs.Count - 1) 
        {

            shapeshiftAnimator.SetTrigger("shapeshift");
            yield return new WaitForSeconds(22 / 24f);

            Poof();
            yield return new WaitForSeconds(4 / 24f);

            shapeshiftAnimator.gameObject.SetActive(false);
        }

        // on next phase
        phaseIdx++;

        //back to shapeshifter
        if (phaseIdx == phasePrefabs.Count)
        {
            phaseIdx = -1;
            shapeshiftAnimator.SetTrigger("idle");
        }
        //new form
        else
        {
            currPhase = Instantiate(phasePrefabs[phaseIdx], transform);
            if (phaseIdx == phasePrefabs.Count - 1)
            {
                StartCoroutine(Util.VoidCallbackNextFrame(() =>
                {
                    currPhase.transform.position = new Vector2(transform.position.x, FindObjectOfType<CharacterControls>().transform.position.y);
                }));
                
            }
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
        var poofObj = Instantiate(poofPrefab, transform);
    }
    #endregion

    public override void OnSelect()
    {
        if (!Conditions.Get(conditionPrefix + "_interactedOnce"))
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
        Conditions.Set(conditionPrefix + "_interactedOnce", true);
    }

    private void OnDestroy()
    {
        DialogueRunner dr = FindObjectOfType<DialogueRunner>();
        if (dr)
        {
            dr.RemoveCommandHandler("shapeshift");
            dr.RemoveCommandHandler("shapeshifterIntro");
        }
    }
}
