using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glitch_Invert : GameDevGlitch
{
    private Camera cam;
    private float defaultCamSize;

    protected override void Start()
    {
        base.Start();
        cam = FindObjectOfType<Camera>();
        defaultCamSize = cam.orthographicSize;
    }

    public override void TurnOn()
    {
        GameManager.Instance.PaddleControls.inverted = true;
        cam.orthographicSize = defaultCamSize * -1;
    }

    public override void TurnOff()
    {
        GameManager.Instance.PaddleControls.inverted = false;
        cam.orthographicSize = defaultCamSize;
    }
}
