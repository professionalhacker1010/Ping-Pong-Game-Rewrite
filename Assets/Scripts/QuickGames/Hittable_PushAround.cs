using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hittable_PushAround : MonoBehaviour, IHittable
{
    static float drag = .9f;
    Vector3 force = Vector3.zero;
    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnHit(float hitX, float hitY)
    {
        force = new Vector3(hitX, hitY);   
    }

    private void FixedUpdate()
    {
        rb.AddForce(force);
        force = Vector2.zero;
        //force *= drag;

        //if (force.magnitude < 0.01f) force = Vector3.zero;
    }
}
