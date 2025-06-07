using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OverworldManager : MonoBehaviour
{
    #region
    private static OverworldManager _instance;
    public static OverworldManager Instance
    {
        get
        {
            if (_instance == null) Debug.Log("The TransitionManager is NULL");

            return _instance;
        }

    }
    #endregion

    [SerializeField] CharacterControls playerController;

    string gameScene = "LevelSelect";

    public CharacterControls PlayerController { get => playerController; }
    public string GameScene { get => gameScene; }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        TransitionManager.Instance.activeScene = gameScene;
    }

    public void MoveToGameScene(GameObject go)
    {
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(gameScene));
    }
}
