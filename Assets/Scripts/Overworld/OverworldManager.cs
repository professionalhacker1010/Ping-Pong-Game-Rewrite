using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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

    public string GameScene { get => gameScene; }
    [SerializeField] private string gameScene = "LevelSelect";

    [Serializable]
    public class SceneInfo
    {
        public string name;
        public Vector2 spawnPos;
        public bool facingLeft;
        public Vector2 minMaxCameraX;
    }
    [SerializeField] private List<SceneInfo> sceneInfo;


    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        TransitionManager.Instance.activeScene = gameScene;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneInfo.ForEach(i =>
        {
            if (i.name == scene.name)
            {
                TransitionManager.Instance.QuickIn();
            }
        });
    }

    public void TransitionToOverworld(string scene)
    {
        for (int i = 0; i < sceneInfo.Count; i++)
        {
            if (sceneInfo[i].name == gameScene)
            {
                var cc = FindObjectOfType<CharacterControls>();
                sceneInfo[i].spawnPos = cc.transform.position;
                sceneInfo[i].facingLeft = cc.FacingLeft;
            }
        }
        gameScene = scene;
        TransitionManager.Instance.QuickOut(scene);
    }

    public void TransitionToGame(int level)
    {
        for (int i = 0; i < sceneInfo.Count; i++)
        {
            if (sceneInfo[i].name == gameScene)
            {
                var cc = FindObjectOfType<CharacterControls>();
                sceneInfo[i].spawnPos = cc.transform.position;
                sceneInfo[i].facingLeft = cc.FacingLeft;
            }
        }
        TransitionManager.Instance.QuickOut("Game");
        LevelManager.chosenOpponent = level;
    }

    public SceneInfo GetSceneInfo()
    {
        SceneInfo retVal = new SceneInfo();
        sceneInfo.ForEach(i =>
        {
            if (i.name == gameScene) retVal = i;
        });
        return retVal;
    }

    public void MoveToGameScene(GameObject go)
    {
        SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(gameScene));
    }
}
