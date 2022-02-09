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

        private Vector3 torqueVec;
        public override void Move(Vector3 direction)
        {
            Debug.DrawRay(LocoSphere.position, direction);

            torqueVec = Vector3.Cross(direction, Vector3.down);

            LocoSphere.AddTorque(torqueVec * torque);
        }
    } 
}
