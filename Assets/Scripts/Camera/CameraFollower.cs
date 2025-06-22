using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private float maxEdge; //edge is taken from the center of the camera
    [SerializeField] private int centerAdjustmentFrames;
    [SerializeField] private GameObject player;
    private Vector3 newTransform;

    private bool normalCameraBehaviour = true;
    private int locks = 0;
    Vector2 minmax;

    public Action afterUpdate;

    private void Start()
    {
        minmax = OverworldManager.Instance.GetSceneInfo().minMaxCameraX;

        newTransform.x = Mathf.Clamp(player.transform.position.x, minmax.x, minmax.y);
        newTransform.y = transform.position.y;
        newTransform.z = -10f;

        transform.position = newTransform;
    }

    private void Update()
    {
        if (normalCameraBehaviour)
        {
            float distance = player.transform.position.x - transform.position.x;

            if ((Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow)) && distance != 0)
            //if (Input.GetAxis("Horizontal") == 0 && distance !=0)
            {
                //StartCoroutine(PanToPlayer(centerAdjustmentFrames));
            }
            //for following to the right
            else if (distance >= maxEdge)
            {
                newTransform.x = player.transform.position.x - maxEdge;
                if (newTransform.x > minmax.y) newTransform.x = minmax.y;
                transform.position = newTransform;
            }
            //for following to the left
            else if (distance <= -maxEdge)
            {
                newTransform.x = player.transform.position.x + maxEdge;
                if (newTransform.x < minmax.x) newTransform.x = minmax.x;
                transform.position = newTransform;
            }

            if (afterUpdate != null) afterUpdate();
        }
    }

    public void LockNormalBehavior()
    {
        locks++;
        normalCameraBehaviour = false;
    }

    public void UnlockNormalBehaviour()
    {
        if (locks > 0) locks--;
        if (locks == 0) normalCameraBehaviour = true;
    }

    /*public void PanCamera(int panFrames, float endPoint)
    {
        StartCoroutine(PanCamera(endPoint, panFrames));
    }*/

    public IEnumerator PanCamera(float endPoint, int panFrames)
    {
        LockNormalBehavior();
        float distance = endPoint - transform.position.x;

        //pan according to values on curve
        Vector3 start = transform.position;
        Vector3 end = new Vector3(endPoint, transform.position.y, transform.position.z);
        float curveIncrement = 1f / panFrames;
        for (int i = 0; i <= panFrames; i++)
        {
            transform.position = Vector3.Lerp(start, end, calcCurve(curveIncrement * i));
            yield return new WaitForSeconds(1 / 30f);
        }

        UnlockNormalBehaviour();
    }

    public IEnumerator PanToPlayer(int panFrames)
    {
        LockNormalBehavior();

        Vector3 start = transform.position;
        float curveIncrement = 1f / panFrames;
        for(int i = 0; i <= panFrames; i++)
        {
            float x = player.transform.position.x;
            if (x < 0f) x = 0f;
            Vector3 end = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(start, end, calcCurve(curveIncrement * i));
            yield return new WaitForSeconds(1 / 30f);
        }

        UnlockNormalBehaviour();
    }

    public void StopCurrentPan()
    {
        StopAllCoroutines();
    }

    private float calcCurve(float x)
    {
        if (x <= 0.5)
        {
            return 2 * Mathf.Pow(x, 2f);
        }
        else
        {
            return (-2 * Mathf.Pow(x-1f, 2f)) + 1f;
        }
    }
}
