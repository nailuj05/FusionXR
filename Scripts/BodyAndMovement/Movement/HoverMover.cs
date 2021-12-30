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

        public float hoverStrength = 2000f;

        public float hoverDampening = 500f;

        [Tooltip("Additional Dampening only applied to the vertical axis, 0 means no additional dampening")]
        public float verticalDampening = 100f;

        public float addedHeight = 0f;

        [Tooltip("Will there be the same force applied to the object below the player")]
        public bool applyCounterForce = true;

        [Tooltip("Use this to tune the amount of counter force applied")]
        public float counterForceScale = 1f;

        [HideInInspector]
        public Rigidbody rb;
        private CollisionAdjuster collisionAdjuster;

        new bool usesGravity => false;

        Vector3 vel;

        RaycastHit hit;

        float currentHeight;
        float heightDifference;

        Collider currentCollider;
        Rigidbody currentRigidbody;

        Vector3 force;
        Vector3 verticalVel;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            collisionAdjuster = GetComponent<CollisionAdjuster>();
        }

        public override void Move(Vector3 direction)
        {
            //Stick Movement 
            vel.Set(direction.x, rb.velocity.y, direction.z);

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

                force = Vector3.up * heightDifference * hoverStrength;

                //Apply Force
                rb.AddForce(force);

                //Apply Dampening
                rb.AddForce(-rb.velocity * hoverDampening);

                //Apply Vertical Dampening
                verticalVel = Vector3.Project(rb.velocity, Vector3.up);
                rb.AddForce(-verticalVel * verticalDampening);

                //Counter Force
                if (!applyCounterForce) return;

                if(currentCollider != hit.collider)
                {
                    if (hit.collider.TryGetComponent(out currentRigidbody))
                    {
                        currentCollider = hit.collider;
                    }
                    else
                    {
                        currentCollider = null;
                        return;
                    }
                }

                if (currentRigidbody)
                {
                    currentRigidbody.AddForceAtPosition(-force * counterForceScale, hit.point);
                }
            }
        }
    }
}