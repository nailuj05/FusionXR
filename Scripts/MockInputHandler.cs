using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class MockInputHandler : MonoBehaviour
    {
        public float mouseSensitivity = 1f;

        public GameObject player;

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

            movement = player.GetComponent<Movement>();
            VRCamera = Camera.main.transform;

            l_handPoser = l_hand.GetComponent<HandPoser>();
            r_handPoser = r_hand.GetComponent<HandPoser>();
        }

        private void Update()
        {
            #region Mouse Look and move
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                float yLookDir = -Mathf.Clamp(Input.GetAxis("Mouse Y"), -90, 90);
                Vector3 lookDir = new Vector3(yLookDir, Input.GetAxis("Mouse X"), 0);

                lookDir *= mouseSensitivity;

                VRCamera.localEulerAngles += lookDir;

                ///Movement
                Vector2 movementDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

                movement?.PreprocessInput(movementDir);
            }
            #endregion

            # region Turn
            if (Input.GetKey(KeyCode.Q))
            {
                Direction dir = new Direction();
                dir = Direction.West;
                movement.Turn(dir);
            }
            else if (Input.GetKey(KeyCode.E))
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

            #region Hands (Poser and Grabbing)
            float pinchTarget = Input.GetMouseButton(0) ? 1 : 0;
            float grabTarget = Input.GetMouseButton(1) ? 1 : 0;

            mockPinch = Mathf.MoveTowards(mockPinch, pinchTarget, Time.deltaTime * 3);
            mockGrab = Mathf.MoveTowards(mockGrab, grabTarget, Time.deltaTime * 3);

            if (currentHand == leftHand)
            {
                //Apply mock Pinch/Grab to Hand/HandPoser here
                if (l_hand)
                {
                    if (Input.GetMouseButtonDown(1))
                        l_hand.DebugGrab();
                    if(Input.GetMouseButtonUp(1))
                        l_hand.DebugLetGo();
                }
                if (l_handPoser)
                {
                    l_handPoser.SetPinchGrabDebug(mockPinch, mockGrab);
                }
            }
            else if(currentHand == rightHand)
            {
                //Apply mock Pinch/Grab to Hand/HandPoser here
                if (r_hand)
                {
                    if (Input.GetMouseButtonDown(1))
                        r_hand.DebugGrab();
                    if (Input.GetMouseButtonUp(1))
                        r_hand.DebugLetGo();
                }
                if (r_handPoser)
                {
                    r_handPoser.SetPinchGrabDebug(mockPinch, mockGrab);
                }
            }
            #endregion
        }
    }
}
