using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;//add this

public class TableSelectManager : MonoBehaviour
{
    #region
    private static TableSelectManager _instance;
    public static TableSelectManager Instance
    {
        get
        {
            if (_instance == null) Debug.Log("The TableSelectManager is NULL");

            return _instance;
        }
    }
    #endregion

    private List<TableSelect> tables;
    private Dictionary<int, Func<IEnumerator>> beforeTransitionToGame;
    public static bool selectable = true;
    private static int locks = 0;

    private void Awake()
    {
        _instance = this;
        tables = new List<TableSelect>();
        beforeTransitionToGame = new Dictionary<int, Func<IEnumerator>>();
        Conditions.SetCondition("firstGameStarted", false);
    }

    public void TransitionToGame(int level)
    {
        StartCoroutine(TransitionToGameHelper(level));
    }

    private IEnumerator TransitionToGameHelper(int level)
    {
        if (beforeTransitionToGame.ContainsKey(level))
        {
            yield return beforeTransitionToGame[level]();
        }
        if (level == 0) Conditions.SetCondition("firstGameStarted", true);
        TransitionManager.Instance.QuickOut("Game");
        LevelManager.chosenOpponent = level;
    }

    public void AddTable(TableSelect tableSelect)
    {
        tables.Add(tableSelect);
    }

    public void LockTable(int level)
    {
        tables.ForEach(table =>
        {
            if (table.Level == level) table.LockThisTable();
        });
    }

    public void UnlockTable(int level)
    {
        tables.ForEach(table =>
        {
            if (table.Level == level) table.UnlockThisTable();
        });
    }

    //Shows select table indicator, doesn't actually select it
    public void SelectTable(int level)
    {
        tables.ForEach(table =>
        {
            if (table.Level == level) table.OnSelect();
        });
    }

    public void DeselectTable(int level)
    {
        tables.ForEach(table =>
        {
            if (table.Level == level) table.OnDeselect();
        });
    }

    public void AddBeforeTransitionToGameCoroutine(Func<IEnumerator> coroutine, int level)
    {
        beforeTransitionToGame.Add(level, coroutine);
    }

    public void ClearBeforeTransitionToGameCoroutine(int level) { beforeTransitionToGame.Remove(level); }

    public static void LockSelection()
    {
        locks++;
        selectable = false;
    }

    public static void UnlockSelection()
    {
        locks--;
        if (locks == 0) selectable = true;
    }
}
