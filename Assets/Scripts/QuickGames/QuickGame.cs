using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class QuickGame : MonoBehaviour
{
    [SerializeField] Camera cam;

    virtual protected void Start()
    {
        Camera.main.GetComponent<UniversalAdditionalCameraData>().cameraStack.Add(cam);
    }

    virtual protected void Update()
    {
        if (KeyCodes.Pause())
        {
            WinGame();
        }
    }

    virtual protected void WinGame()
    {
        var overworldManager = OverworldManager.Instance;
        if (overworldManager)
        {
            overworldManager.ExitQuickGame();
        }
    }
}
