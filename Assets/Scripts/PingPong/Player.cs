using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] public BallPath ballPath;

    private void Start()
    {
        GameManager.Instance.Player = this;
    }
}
