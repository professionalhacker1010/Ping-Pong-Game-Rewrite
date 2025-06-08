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
        levelsPlayed = new List<bool>(new bool[opponentPrefabs.Count]);
        levelsWon = new List<bool>(new bool[opponentPrefabs.Count]);
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

    static List<bool> levelsPlayed;
    static List<bool> levelsWon;

    private void Start()
    {

    }

    public static bool IsLevelPlayed(int level) => levelsPlayed[level];
    public static void SetLevelPlayed(int level) { 
        levelsPlayed[level] = true;
        Conditions.SetCondition("LevelManager_Played_Level" + level.ToString(), true);
    }
    public static bool IsLevelWon(int level) => levelsWon[level];
    public static void SetLevelWon(int level) { 
        levelsWon[level] = true;
        Conditions.SetCondition("LevelManager_Won_Level" + level.ToString(), true);
    }

    public GameObject CreateChosenOpponent() => Instantiate(opponentPrefabs[chosenOpponent]);
}
