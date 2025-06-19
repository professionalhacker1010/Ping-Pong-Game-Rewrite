using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableSelect : MonoBehaviour, ICanInteract
{
    [SerializeField] private int level;
    private Vector3 selectedTransform = new Vector3(0f, 0.5f, 0f);
    private string selectableCondition;

    public int Level { get => level; }

    //ICanInteract
    public int InteractPriority { get => 0; }
    public Vector2 InteractPos { get => transform.position; }
    public bool IsInteractable { 
        get {
            return Conditions.Get(selectableCondition);
        } 
    }

    private void Awake()
    {
        selectableCondition = "table" + level.ToString() + "_selectable";
        Conditions.Initialize(selectableCondition, false);
    }

    private void Start()
    {
        if (TableSelectManager.Instance)
        {
            TableSelectManager.Instance.AddTable(this);
        }
    }

    public void LockThisTable()
    {
        Conditions.Set(selectableCondition, false);
    }

    public void UnlockThisTable()
    {
        Conditions.Set(selectableCondition, true);
    }

    public void OnInteract()
    {
        TableSelectManager.Instance.TransitionToGame(level);
    }

    public void OnSelect()
    {
        transform.Translate(selectedTransform);
    }

    public void OnDeselect()
    {
        transform.Translate(-selectedTransform);
    }
}
