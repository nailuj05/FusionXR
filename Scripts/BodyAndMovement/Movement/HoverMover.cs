using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    [RequireComponent(typeof(Rigidbody))]
    public class HoverMover : Movement
    {
        [Header("Hover Mover")]
        public LayerMask groundLayers;

        public float hoverStrength = 500f;

        public float hoverDampening = 100f;

        public float addedHeight = 0f;

        [HideInInspector]
        public Rigidbody rb;
        private CollisionAdjuster collisionAdjuster;

        new bool usesGravity => false;

        Vector3 vel;

        RaycastHit hit;
        float currentHeight;
        float heightDifference;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            collisionAdjuster = GetComponent<CollisionAdjuster>();
        }

        public override void Move(Vector3 direction)
        {
            //Stick Movement 
            vel = Vector3.ProjectOnPlane(direction, Vector3.up);

            vel.y = rb.velocity.y;

            CurrentVelocity = rb.velocity;

            rb.velocity = vel;
        }

        private void FixedUpdate()
        {
            //Hovering
            if (Physics.Raycast(Player.main.head.position, Vector3.down, out hit, 2f, groundLayers))
            {
                currentHeight = (Player.main.head.position - hit.point).magnitude;
                heightDifference = (collisionAdjuster.p_localHeight - currentHeight) + addedHeight;

                //Square height Difference? 
                rb.AddForce(Vector3.up * heightDifference * hoverStrength);
                rb.AddForce(-rb.velocity * hoverDampening);
            }
        }
    }
}