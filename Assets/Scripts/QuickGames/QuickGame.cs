using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class QuickGame : MonoBehaviour
{
    [SerializeField] Camera cam;
    public PaddleControls paddleControls;

    protected virtual void Start()
    {
        Camera.main.GetComponent<UniversalAdditionalCameraData>().cameraStack.Add(cam);
        paddleControls.OnHit += OnPaddleHit;
    }

    protected virtual void Update()
    {
        if (KeyCodes.Pause())
        {
            CloseGame();
        }
    }

    public void CloseGame()
    {
        var overworldManager = OverworldManager.Instance;
        if (overworldManager)
        {
            overworldManager.ExitQuickGame();
        }
    }

    protected virtual void OnPaddleHit(IHittable hittable)
    {

    }

    private void OnDestroy()
    {
        paddleControls.OnHit -= OnPaddleHit;
    }
}
