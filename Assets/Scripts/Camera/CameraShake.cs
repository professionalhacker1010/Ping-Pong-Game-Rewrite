using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] float strengthZeroRadius, strengthOneRadius;
    [SerializeField] float shakeIntervalTime;
    private float startX, startY;
    // Start is called before the first frame update
    void Start()
    {
        startX = transform.position.x;
        startY = transform.position.y;
    }

    public void Shake(int strength)
    {
        StartCoroutine(ShakeCameraHelper(strength));
    }

    private IEnumerator ShakeCameraHelper(int strength)
    {
        float radius = 0;
        if (strength == 0)
        {
            radius = strengthZeroRadius;
        }
        else if (strength == 1)
        {
            radius = strengthOneRadius;
        }

        float Z = transform.position.z;
        //shake in a start pattern
        //left
        transform.position = new Vector3(-1 * radius + startX, startY, Z);
        yield return new WaitForSeconds(shakeIntervalTime);

        //right
        transform.position = new Vector3(radius + startX, startY, Z);
        yield return new WaitForSeconds(shakeIntervalTime);

        //bottom left
        transform.position = new Vector3(-1 * radius + startX, -1 * radius + startY, Z);
        yield return new WaitForSeconds(shakeIntervalTime);

        //up
        transform.position = new Vector3(startX, radius + startY, Z);
        yield return new WaitForSeconds(shakeIntervalTime);

        //bottom right (half radius)
        transform.position = new Vector3(radius/2 + startX, -1 * radius/2 + startY, Z);
        yield return new WaitForSeconds(shakeIntervalTime);

        transform.position = new Vector3(startX, startY, Z);
    }
}
