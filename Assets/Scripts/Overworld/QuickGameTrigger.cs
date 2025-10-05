using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using UnityEngine.Events;

public class QuickGameTrigger : MonoBehaviour, IHittable
{
    [SerializeField] GameObject quickGame;
    [SerializeField] Vector3 indicatorOffset;
    public UnityEvent OnQuickGameWon;
    public UnityEvent OnDeinitialize;

    KeyPressPrompt indicator;

    string wonCondition = "";
    bool indicatorVisible = false;
    GameObject player;

    public void OnHit(float x, float y)
    {
        if (Conditions.Get(wonCondition) || !enabled)
        {
            return;
        }

        if (!indicatorVisible)
        {
            indicatorVisible = true;
            indicator.Show(transform.position + indicatorOffset);
        }
        else if (indicatorVisible)
        {
            HideIndicator();

            GameObject game = OverworldManager.Instance.CreateQuickGame(quickGame);
            game.GetComponent<QuickGame>().OnQuickGameWon +=
                () =>
                {
                    Conditions.Set(wonCondition, true);
                    OnQuickGameWon.Invoke();
                    OnDeinitialize.Invoke();
                };
        }
    }

    public void PlayDialogue(string node)
    {
        StartCoroutine(Util.VoidCallbackTimer(
            1.0f,
            () => DialogueManager.Instance.StartDialogue(node)
            ));
    }

    void HideIndicator()
    {
        indicatorVisible = false;
        indicator.Hide();
    }

    private void Awake()
    {
        wonCondition = quickGame.name + "_won";
        Conditions.Initialize(wonCondition, false);
    }

    private void Start()
    {
        indicator = KeyPressPromptManager.Instance.GetKeyPressPrompt("?");
        player = GameObject.FindWithTag("Player");
        var dr = FindObjectOfType<DialogueRunner>();
        if (dr)
        {
            dr.onDialogueStart.AddListener(HideIndicator);
        }

        if (Conditions.Get(wonCondition))
        {
            OnDeinitialize.Invoke();
        }
    }

    private void Update()
    {
        if (indicatorVisible && Mathf.Abs(player.transform.position.x - transform.position.x) > 5)
        {
            HideIndicator();
        }
    }

    private void OnDestroy()
    {
        var dr = FindObjectOfType<DialogueRunner>();
        if (dr)
        {
            dr.onDialogueStart.RemoveListener(HideIndicator);
        }
    }
}
