using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glitch_DesktopScreen : GameDevGlitch
{
    [SerializeField] private float minimizedCameraSize;
    [SerializeField] private Vector3 minimizedCameraPosition;
    private Camera cam;

    protected override void Start()
    {
        base.Start();
        cam = FindObjectOfType<Camera>();
        cam.orthographicSize = minimizedCameraSize;
        cam.transform.position = minimizedCameraPosition;
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
        print("enable");
        cam.orthographicSize = minimizedCameraSize;
        cam.transform.position = minimizedCameraPosition;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void TurnOff()
    {
        cam.orthographicSize = 5.375873f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
