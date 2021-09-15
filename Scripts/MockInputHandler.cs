using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class MockInputHandler : MonoBehaviour
    {
        public Movement movement;

        public Transform VRCamera;

        public float mouseSensitivity = 1f;

        private void Update()
        {
            ///Mouse Look
            float yLookDir = -Mathf.Clamp(Input.GetAxis("Mouse Y"), -90, 90);
            Vector3 lookDir = new Vector3(yLookDir, Input.GetAxis("Mouse X"), 0);

            lookDir *= mouseSensitivity;

            VRCamera.localEulerAngles += lookDir;

            ///Movement
            Vector3 movementDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            movementDir = Vector3.ProjectOnPlane(VRCamera.TransformVector(movementDir), Vector3.up);

            movement.Move(movementDir.normalized);

            ///Turn
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Direction dir = new Direction();
                dir = Direction.West;
                movement.Turn(dir);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                Direction dir = new Direction();
                dir = Direction.East;
                movement.Turn(dir);
            }

            ///Arms
        }
    }
}
