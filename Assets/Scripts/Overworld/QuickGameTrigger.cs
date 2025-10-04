using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class QuickGameTrigger : MonoBehaviour, IHittable
{
    [SerializeField] GameObject quickGame;
    [SerializeField] Vector3 indicatorOffset;

    KeyPressPrompt indicator;

    string wonCondition = "";
    bool indicatorVisible = false;
    GameObject player;

    public void OnHit(float x, float y)
    {
        if (Conditions.Get(wonCondition))
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
        }
    }

    void HideIndicator()
    {
        indicatorVisible = false;
        indicator.Hide();
    }

    private void Awake()
    {
        wonCondition = "QuickGame_" + quickGame.name + "_Won";
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
