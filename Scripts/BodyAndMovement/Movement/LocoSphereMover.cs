using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class LocoSphereMover : Movement
    {
        [SerializeField]
        private Rigidbody LocoSphere;

        [SerializeField]
        private float torque = 500f;

        private Vector3 currentMove;

        private Vector3 torqueVec;
        public override void Move(Vector3 direction)
        {
            currentMove = direction;
        }

        private void FixedUpdate()
        {
            LocoSphere.freezeRotation = true;

            if(currentMove.sqrMagnitude > 0)
            {
                torqueVec = Vector3.Cross(currentMove, Vector3.down);

                LocoSphere.AddTorque(torqueVec * torque, ForceMode.VelocityChange);

                LocoSphere.freezeRotation = false;
            }

            currentMove = Vector3.zero;
        }

        Vector3 vel;
        private void StopAngularMomentum(Rigidbody rb)
        {
            vel = rb.angularVelocity;
            vel.x = 0;
            vel.z = 0;
            rb.angularVelocity = vel;
        }
    } 
}
