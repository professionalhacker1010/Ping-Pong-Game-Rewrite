using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//add this

public class SpaceToContinue : MonoBehaviour
{
    void Update()
    {
        if (KeyCodes.Hit())
        {
            //StartCoroutine(ChangeScene("LevelSelect")); //debug

            if (GameManager.Instance.GameIsWon())
            {
                OverworldManager.Instance.TransitionToOverworld(OverworldManager.Instance.GameScene);
                GameManager.Instance.ResetGame();
            }
            else if (GameManager.Instance.GameIsLost())
            {
                TransitionManager.Instance.QuickOut(GameManager.Instance.GameScene);
            }
        }
    }
}
