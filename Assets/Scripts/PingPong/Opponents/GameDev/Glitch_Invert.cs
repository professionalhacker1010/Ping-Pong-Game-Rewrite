using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Glitch_Invert : GameDevGlitch
{
    private Camera cam;
    private PixelPerfectCamera pixelCam;
    private float defaultCamSize;

    protected override void Start()
    {
        base.Start();
        cam = FindObjectOfType<Camera>();
        pixelCam = FindObjectOfType<PixelPerfectCamera>();
        defaultCamSize = cam.orthographicSize;
    }

    public override void TurnOn()
    {
        GameManager.Instance.PaddleControls.inverted = true;
        pixelCam.enabled = false;
        cam.orthographicSize = defaultCamSize * -1;
    }

    public override void TurnOff()
    {
        GameManager.Instance.PaddleControls.inverted = false;
        cam.orthographicSize = defaultCamSize;
        pixelCam.enabled = true;
    }
}
