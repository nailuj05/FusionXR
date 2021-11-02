using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyMover : Movement
    {
        public Rigidbody rigidBody;

        new bool usesGravity => false;

        Vector3 vel;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        public override void Move(Vector3 direction)
        {
            vel = Vector3.ProjectOnPlane(direction, Vector3.up);
            //vel += Physics.gravity;// / Time.deltaTime;

            vel.y = rigidBody.velocity.y;

            CurrentVelocity = rigidBody.velocity;

            //rigidBody.MovePosition(transform.TransformPoint(-vel));
            rigidBody.velocity = vel;
        }
    }
}
