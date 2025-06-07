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
    public static void SetLevelPlayed(int level) { levelsPlayed[level] = true;}
    public static bool IsLevelWon(int level) => levelsWon[level];
    public static void SetLevelWon(int level) { levelsWon[level] = true;}

    public GameObject CreateChosenOpponent() => Instantiate(opponentPrefabs[chosenOpponent]);
}
