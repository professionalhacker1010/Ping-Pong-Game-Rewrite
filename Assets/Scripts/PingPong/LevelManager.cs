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
        for (int i = 0; i < opponentScenes.Count; i++)
        {
            string playedCondition = "level_played_" + i.ToString();
            Conditions.Initialize(playedCondition, false);

            string wonCondition = "level_won_" + i.ToString();
            Conditions.Initialize(wonCondition, false);
        }
    }
    #endregion

    [Header("Opponents")]
    [SerializeField] private List<string> opponentScenes;

    [SerializeField]
    public static int chosenOpponent = 0; //opponent chosen from level select screen or from game's progression

    private void Start()
    {

    }

    public static bool IsLevelPlayed(int level) => Conditions.Get("level_played_" + level.ToString());
    public static void SetLevelPlayed(int level) { 
        Conditions.Set("level_played_" + level.ToString(), true);
    }
    public static bool IsLevelWon(int level) => Conditions.Get("level_won_" + level.ToString());
    public static void SetLevelWon(int level) { 
        Conditions.Set("level_won_" + level.ToString(), true);
    }

    public string GetChosenOpponentScene() => opponentScenes[chosenOpponent];
}
