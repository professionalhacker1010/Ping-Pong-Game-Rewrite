using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickGameTrigger : MonoBehaviour, IHittable
{
    [SerializeField] GameObject quickGame;
    [SerializeField] Vector3 indicatorOffset;

    KeyPressPrompt indicator;

    string wonCondition = "";
    bool indicatorVisible = false, gamePlaying = false;

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
        else if (indicatorVisible && !gamePlaying)
        {
            indicatorVisible = false;
            indicator.Hide();

            gamePlaying = true;
            GameObject game = OverworldManager.Instance.EnterQuickGame(quickGame);
        }
    }

    private void Awake()
    {
        wonCondition = "QuickGame_" + quickGame.name + "_Won";
        Conditions.Initialize(wonCondition, false);
    }

    private void Start()
    {
        indicator = KeyPressPromptManager.Instance.GetKeyPressPrompt("?");
    }
}
