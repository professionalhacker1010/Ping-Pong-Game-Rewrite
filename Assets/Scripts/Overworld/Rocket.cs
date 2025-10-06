using System.Collections;
using UnityEngine;

public class Rocket : MonoBehaviour, ICanInteract
{
    public int InteractPriority => 0;

    public Vector2 InteractPos => transform.position;

    public bool IsInteractable => isSelectable;

    private bool isSelectable = true;
    private KeyPressPrompt cKeyPrompt;
    [SerializeField] private float cKeyPromptHeight;

    [SerializeField] string dialogueNode;

    [SerializeField] private Vector3 shakeRadius;
    [SerializeField] private float shakeTime;
    [SerializeField] private float shakeStepTime;

    [SerializeField] private float pauseTime;

    [SerializeField] private float takeoffTime;
    [SerializeField] private Vector3 takeoffPos;

    public void OnInteract()
    {
        if (!Conditions.Get("quickGame_trashRat_won"))
        {
            DialogueManager.Instance.StartDialogue(dialogueNode);
        }
        else
        {
            StartCoroutine(Transition());
        }
    }

    private IEnumerator Transition()
    {
        cKeyPrompt.Hide();

        var cc = FindObjectOfType<CharacterControls>();
        yield return cc.ReadjustPlayer(gameObject, .1f, (cc.transform.position.x - transform.position.x) > 0f, null);
        var srs =  cc.gameObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs)
        {
            sr.enabled = false;
        }

        yield return new WaitForSeconds(pauseTime);

        var om = OverworldManager.Instance;
        if (om.GameScene == "LevelSelect")
        {
            //shake animation
            float t = 0;
            float stepTime = shakeStepTime;
            Vector3 ogPos = transform.position;
            bool prevLeft = false;
            while (t < shakeTime)
            {
                if (stepTime <= 0)
                {
                    Vector3 newPos = new Vector3(
                        ogPos.x + (prevLeft ? Random.Range(0f, shakeRadius.x) : Random.Range(-shakeRadius.x, 0f)),
                        ogPos.y + Random.Range(-shakeRadius.y, shakeRadius.y),
                        ogPos.z
                    );
                    transform.position = newPos;
                    stepTime = shakeStepTime;
                }

                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;
                stepTime -= Time.deltaTime;
            }
            transform.position = ogPos;

            yield return new WaitForSeconds(pauseTime);

            //take off
            t = 0;
            bool transitioned = false;
            while (t < takeoffTime)
            {
                transform.position = Vector3.Lerp(ogPos, takeoffPos, t / takeoffTime);
                yield return new WaitForEndOfFrame();
                t += Time.deltaTime;

                if (takeoffTime - t <= 1.0f && !transitioned)
                {
                    om.TransitionToOverworld("Moon");
                    transitioned = true;
                }
            }
        }
        else
        {
            //play rocket sound effect?

            om.TransitionToOverworld("LevelSelect");
        }
    }


    public void OnDeselect()
    {
        cKeyPrompt.Hide();
    }

    public void OnSelect()
    {
        cKeyPrompt.Show(new Vector3(transform.position.x, transform.position.y + cKeyPromptHeight));
    }

    // Start is called before the first frame update
    void Start()
    {
        cKeyPrompt = KeyPressPromptManager.Instance.GetKeyPressPrompt("C");
    }
}
