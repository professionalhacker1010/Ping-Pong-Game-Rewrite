using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private float distance; //0 means it follows the camera as if it were in the foreground (no edit to transform), 1 means that it stays completely still relative to camera
    [SerializeField] private float startParallax;
    [SerializeField] Vector2 minmaxX;
    //public float distance;
    private Camera cam;
    private float prevCamX;
    private Vector2 minMaxCameraX;

    private void Start()
    {
        cam = FindObjectOfType<Camera>();
        prevCamX = cam.transform.position.x;

        minMaxCameraX = OverworldManager.Instance.GetSceneInfo().minMaxCameraX;

        var cf = cam.GetComponent<CameraFollower>();
        cf.afterUpdate += DoParallax;
    }

    private void DoParallax()
    {
        float camX = cam.transform.position.x;

        float lerp = Mathf.InverseLerp(minMaxCameraX.x, minMaxCameraX.y, camX);
        float newX = Mathf.Lerp(minmaxX.x, minmaxX.y, lerp);
        transform.localPosition = new Vector3(newX, transform.localPosition.y, transform.localPosition.z);
    }

    private void Update()
    {
        //Parallax();
        /*        if (camX >= startParallax)
                {
                    float x = distance * (camX - prevCamX);
                    transform.position = new Vector3(transform.position.x + x, transform.position.y);
                }
                prevCamX = camX;*/
    }
}
