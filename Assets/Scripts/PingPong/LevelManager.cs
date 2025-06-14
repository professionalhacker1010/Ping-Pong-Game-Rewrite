using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//add this

public class LevelManager : MonoBehaviour
{
    #region
    private static LevelManager _instance;
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null) Debug.Log("The LevelManager is NULL");

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        for (int i = 0; i < opponentPrefabs.Count; i++)
        {
            Conditions.SetCondition("LevelManager_Played_Level" + i.ToString(), false);
            Conditions.SetCondition("LevelManager_Won_Level" + i.ToString(), false);
        }
    }
    #endregion

    [Header("Opponents")]
    [SerializeField] private List<GameObject> opponentPrefabs;

    public static int chosenOpponent = 0; //opponent chosen from level select screen or from game's progression

    private void Start()
    {

    }

    public static bool IsLevelPlayed(int level) => Conditions.GetCondition("LevelManager_Played_Level" + level.ToString());
    public static void SetLevelPlayed(int level) { 
        Conditions.SetCondition("LevelManager_Played_Level" + level.ToString(), true);
    }
    public static bool IsLevelWon(int level) => Conditions.GetCondition("LevelManager_Won_Level" + level.ToString());
    public static void SetLevelWon(int level) { 
        Conditions.SetCondition("LevelManager_Won_Level" + level.ToString(), true);
    }

    public GameObject CreateChosenOpponent() => Instantiate(opponentPrefabs[chosenOpponent]);
}
