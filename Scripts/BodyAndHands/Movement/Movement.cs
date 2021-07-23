using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fusion.XR
{
    public enum MovementDirection
    {
        HeadOriented = 0,
        HandOriented = 1,
    }

    public class Movement : MonoBehaviour
    {
        [Header("Basic Movement")]
        public float playerSpeed = 2f;

        public Transform head;
        public Transform hand;

        public MovementDirection movementDirection;

        private Transform currentMovementDirection;

        public InputActionReference movementAction;

        [Range(0.05f, 0.9f)]
        public float activationThreshold = 0.1f;

        [Header("External Factors")]
        public bool canMove;
        public bool useGravity;

        public Vector3 gravity = Physics.gravity;


        private void Start()
        {
            movementAction.action.performed += PreprocessMovement;

            if (movementDirection == MovementDirection.HandOriented)
            {
                currentMovementDirection = hand;
            }
            else
            {
                currentMovementDirection = head;
            }
        }

        private void Update()
        {
            //Move(gravity.normalized, gravity.magnitude);
        }

        public virtual void PreprocessMovement(InputAction.CallbackContext obj)
        {
            if (!canMove)
                return;


            Vector2 movementInput = movementAction.action.ReadValue<Vector2>();

            if(movementInput.magnitude >= activationThreshold)
            {
                currentMovementDirection.TransformDirection(movementInput);
                Move(movementInput.normalized);
            }
        }

        public virtual void Move(Vector3 direction)
        {

        }

        public virtual void Move(Vector3 direction, float customSpeed)
        {

        }
    }
}
