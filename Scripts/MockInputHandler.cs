using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class MockInputHandler : MonoBehaviour
    {
        public float mouseSensitivity = 1f;

        public Transform leftHand;
        public Transform rightHand;

        private Movement movement;

        private Transform VRCamera;

        public FusionXRHand l_hand;
        public FusionXRHand r_hand;

        private HandPoser l_handPoser;
        private HandPoser r_handPoser;

        private float mockPinch;
        private float mockGrab;


        private Transform currentHand;

        public float scrollFactor = 0.5f;
        private float scrollDelta = 1f;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            currentHand = rightHand;

            movement = GetComponent<Movement>();
            VRCamera = Camera.main.transform;

            l_handPoser = l_hand.GetComponent<HandPoser>();
            r_handPoser = r_hand.GetComponent<HandPoser>();
        }

        private void Update()
        {
            #region Mouse Look
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                float yLookDir = -Mathf.Clamp(Input.GetAxis("Mouse Y"), -90, 90);
                Vector3 lookDir = new Vector3(yLookDir, Input.GetAxis("Mouse X"), 0);

                lookDir *= mouseSensitivity;

                VRCamera.localEulerAngles += lookDir;

                ///Movement
                Vector3 movementDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

                movementDir = Vector3.ProjectOnPlane(VRCamera.TransformVector(movementDir), Vector3.up);

                movement.Move(movementDir.normalized);
            }
            #endregion

            # region Turn
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
            #endregion

            #region Arms
            ///Switch current hand
            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentHand = (currentHand == rightHand) ? leftHand : rightHand;
                scrollDelta = currentHand.localPosition.z / scrollFactor;
            }

            ///Move hand forwards/backwards
            scrollDelta += Input.mouseScrollDelta.y;

            Vector3 localPos = currentHand.localPosition;
            localPos.z = scrollDelta * scrollFactor;
            currentHand.localPosition = localPos;

            ///Control hand with Mouse
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                //Unlock cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 deltaArmMovement = mouseRay.direction - currentHand.forward;

                currentHand.forward = Vector3.MoveTowards(currentHand.forward, deltaArmMovement, .5f);
            }
            else
            {
                //Lock cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            #endregion

        }
    }
}
