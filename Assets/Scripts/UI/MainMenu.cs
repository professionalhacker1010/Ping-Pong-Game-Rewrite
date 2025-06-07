using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//add this

public class MainMenu : MonoBehaviour
{
	private static bool saveStarted = false;

    private void Start()
    {
		TransitionManager.Instance.activeScene = "Menu";
		TransitionManager.Instance.QuickIn();
    }

    public void PlayGame()
	{


		//get the appropriate scene from start button
		if (!saveStarted) //condition for when first starting game
        {
			Debug.Log("saveStarted = " + saveStarted.ToString());
			saveStarted = true;
			//play transition
			TransitionManager.Instance.QuickOut("Cutscene");
		}
		/*else if (GameManager.GameInProgress)
		{
			StartCoroutine(ChangeScene("Game"));
		}*/
		else if (CutsceneManager.cutsceneInProgress) //for if player exits to menu during cutscene and then goes back
        {
			TransitionManager.Instance.QuickOut("Cutscene");
		}
		else
        {
			//play transition
			TransitionManager.Instance.QuickOut("LevelSelect");
        }

		//Debug.Log("Game in prog: " + GameManager.GameInProgress.ToString());
		//Debug.Log("Cutscene in prog " + CutsceneManager.cutsceneInProgress.ToString());
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
