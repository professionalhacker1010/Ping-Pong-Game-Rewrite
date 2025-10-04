using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] Vector2 minmaxX;

    private Camera cam;
    private Vector2 minMaxCameraX;
    bool staticFollow = false;

    private void Awake()
    {
        staticFollow = (minmaxX.x == 0 && minmaxX.y == 0);
    }

    private void Start()
    {
        cam = Camera.main;

        minMaxCameraX = OverworldManager.Instance.GetSceneInfo().minMaxCameraX;

        var cf = cam.GetComponent<CameraFollower>();
        cf.afterUpdate += DoParallax;
    }

    private void DoParallax()
    {
        float camX = cam.transform.position.x;

        if (staticFollow)
        {
            transform.localPosition = new Vector3(camX, transform.localPosition.y, transform.localPosition.z);
            return;
        }

        float lerp = Mathf.InverseLerp(minMaxCameraX.x, minMaxCameraX.y, camX);
        float newX = Mathf.Lerp(minmaxX.x, minmaxX.y, lerp);
        transform.localPosition = new Vector3(newX, transform.localPosition.y, transform.localPosition.z);
    }

    private void OnDestroy()
    {
        if (cam)
        {
            var cf = cam.GetComponent<CameraFollower>();
            cf.afterUpdate -= DoParallax;
        }

    }
}
