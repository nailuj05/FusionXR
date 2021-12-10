using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WheelSpinner : MonoBehaviour
{
    Rigidbody rb;

    public Vector3 axis;
    public float speed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        var newRot = transform.localRotation * Quaternion.Euler(axis * speed);
        rb.MoveRotation(newRot);
    }
}
