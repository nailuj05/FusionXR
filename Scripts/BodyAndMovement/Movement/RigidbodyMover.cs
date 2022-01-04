using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyMover : Movement
    {
        [HideInInspector]
        public Rigidbody rb;

        new bool usesGravity => false;

        Vector3 vel;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public override void Move(Vector3 direction)
        {
            vel = Vector3.ProjectOnPlane(direction, Vector3.up);
            //vel += Physics.gravity;// / Time.deltaTime;

            vel.y = rb.velocity.y;

            CurrentVelocity = rb.velocity;

            Debug.Log("Move");

            //rigidBody.MovePosition(transform.TransformPoint(-vel));
            rb.velocity = vel;
        }
    }
}
