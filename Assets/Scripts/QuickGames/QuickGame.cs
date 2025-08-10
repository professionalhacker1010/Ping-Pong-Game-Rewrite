using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickGame : MonoBehaviour
{
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
