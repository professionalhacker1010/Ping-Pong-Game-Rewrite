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
        //play transition
        TransitionManager.Instance.QuickOut("LevelSelect");
    }

	public void QuitGame()
	{
		Application.Quit();
	}
}
