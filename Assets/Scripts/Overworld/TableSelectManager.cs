using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

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
    public static bool selectable = true;
    private static int locks = 0;

    private void Awake()
    {
        _instance = this;
        tables = new List<TableSelect>();
    }

    public void AddTable(TableSelect tableSelect)
    {
        tables.Add(tableSelect);
    }

    public void RemoveTable(TableSelect tableSelect)
    {
        tables.Remove(tableSelect);
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

    public Vector3 GetTablePosition(int level)
    {
        Vector3 pos = Vector3.zero;
        tables.ForEach(table =>
        {
            if (table.Level == level) pos = table.transform.position;
        });
        return pos;
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
