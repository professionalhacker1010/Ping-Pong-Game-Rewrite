using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableSelect : MonoBehaviour, ICanInteract
{
    [SerializeField] private int level;
    [SerializeField] private BoxCollider2D playerCollider;
    private BoxCollider2D boxCollider;
    private Vector3 selectedTransform = new Vector3(0f, 0.5f, 0f);
    private bool indicatorPlaying = false, thisSelectable = true;

    //ICanInteract
    public int InteractPriority { get => 0; }
    public Vector2 InteractPos { get => transform.position; }
    
    public void LockThisTable()
    {
        thisSelectable = false;
    }

    public void UnlockThisTable()
    {
        thisSelectable = true;
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
