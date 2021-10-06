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
            //Ground Check: Raycast from players head downwards, with max distance being the players height + a small epsilon and the radius of the players collider
            //if(Physics.SphereCast(head.position, Player.main.collisionAdjuster.p_CollisionRadius, Vector3.down, out RaycastHit hit, Player.main.collisionAdjuster.p_localHeight + 0.3f))

            vel = direction * Time.deltaTime * 50;
            rigidBody.velocity = vel + Vector3.up * rigidBody.velocity.y;
        }
    }
}
