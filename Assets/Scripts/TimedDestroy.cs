using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
    [SerializeField] float frames;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, frames / 24f);
    }
}
