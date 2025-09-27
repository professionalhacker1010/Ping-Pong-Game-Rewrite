using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Glitch_DesktopScreen : GameDevGlitch
{
    [SerializeField] private int minimizedCameraSize;
    [SerializeField] private Vector3 minimizedCameraPosition;
    private Camera cam;
    private PixelPerfectCamera pixelCam;
    private Vector3 originalPos;
    private int originalX, originalY;

    protected override void Start()
    {
        base.Start();
        cam = FindObjectOfType<Camera>();
        originalPos = cam.transform.position;
        
        pixelCam = FindObjectOfType<PixelPerfectCamera>();
        originalX = pixelCam.refResolutionX;
        originalY = pixelCam.refResolutionY;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            TurnOff();
        }
    }

    private void OnMouseDown()
    {
        TurnOff();
    }

    public override void TurnOn()
    {
        cam.transform.position = minimizedCameraPosition;
        pixelCam.refResolutionX = originalX * minimizedCameraSize;
        pixelCam.refResolutionY = originalY * minimizedCameraSize;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void TurnOff()
    {
        cam.transform.position = originalPos;
        pixelCam.refResolutionX = originalX;
        pixelCam.refResolutionY = originalY;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
