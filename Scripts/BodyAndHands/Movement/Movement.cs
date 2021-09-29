using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fusion.XR
{
    public class Movement : MonoBehaviour
    {
        [Header("Basic Movement")]
        public float playerSpeed = 2f;

        public Transform head;
        public Transform hand;

        public MovementDirection movementDirection;
        public TurnMode turnMode;

        public float turnSpeed = 5;
        public float turnAngle = 45;
        public float turnCooldown = 0.3f;

        private Transform currentMovementDirection;

        public InputActionReference movementAction;
        public InputActionReference turnAction;

        [Range(0.05f, 0.9f)]
        public float activationThreshold = 0.1f;

        [Header("External Factors")]
        public bool canMove = true;
        public bool useGravity = false;

        public Vector3 gravity = Physics.gravity;

        private List<MovementOverride> movementOverrides = new List<MovementOverride>();

        private bool waitForTurn = false;

        private void Start()
        {
            //Subscribe to Movement Actions
            movementAction.action.performed += PreprocessMovement;
            turnAction.action.performed += Turn; //This working with overrides?

            //Set Movement Direction Initially
            SetMovementDirection(movementDirection);

            //Add the standart Movement Override
            movementOverrides.Add(new MovementOverride());
        }

        private void Update()
        {
            if(useGravity && canMove)
            {
                QueueMove(gravity.normalized, gravity.magnitude);
            }
        }

        private void Turn(InputAction.CallbackContext obj)
        {
            if (waitForTurn)
                return;

            Vector2 turnDirection = obj.ReadValue<Vector2>();

            Direction direction = Utilities.GetDirectionFromVector(turnDirection);

            Turn(direction);
        }

        public void Turn(Direction direction)
        {
            if (waitForTurn)
                return;

            if (direction != Direction.North || direction != Direction.South)
            {
                if (turnMode == TurnMode.SnapTurn)
                {
                    SnapTurn(direction, turnAngle);
                }
                //if (turnMode == TurnMode.SmoothTurn)
                //{
                //    SmoothTurn(direction, turnSpeed);
                //}
            }
        }

        public void SetMovementDirection(MovementDirection movementDirection)
        {
            if (movementDirection == MovementDirection.HandOriented)
            {
                currentMovementDirection = hand;
            }
            else
            {
                currentMovementDirection = head;
            }
        }

        #region Queuing, Processing, Overrides

        public virtual void PreprocessMovement(InputAction.CallbackContext obj)
        {
            if (!canMove)
                return;

            Vector3 movementInput = movementAction.action.ReadValue<Vector2>();

            movementInput.Set(movementInput.x, 0, movementInput.y);

            if (movementInput.magnitude >= activationThreshold)
            {
                movementInput = currentMovementDirection.TransformDirection(movementInput);
                movementInput = Vector3.ProjectOnPlane(movementInput, Vector3.up);

                QueueMove(movementInput.normalized, playerSpeed);
            }
        }

        //Queueing Moves allowes different preprocessing and multiple Movements being applied
        public void QueueMove(Vector3 direction, float speed)
        {
            QueueMove(direction.normalized * speed);
        }

        public void QueueMove(Vector3 direction)
        {
            MovementProcessing(direction);
        }

        public void MovementProcessing(Vector3 direction)
        {
            direction = movementOverrides[movementOverrides.Count - 1].ProcessMovement(direction);

            Move(direction);
        }

        public void AddMovementOverride(MovementOverride newMovementOverride)
        {
            for (int i = movementOverrides.Count - 1; i != 0; i--)
            {
                if (newMovementOverride.priority >= movementOverrides[i].priority)
                {
                    movementOverrides.Insert(i, newMovementOverride);
                    break;
                }
            }

            foreach (MovementOverride item in movementOverrides)
            {
                Debug.Log(item.priority);
            }
        }

        public void RemoveMovementOverride(MovementOverride movementOverrideToRemove)
        {
            movementOverrides.Remove(movementOverrideToRemove);
        }

        #endregion

        //Override the following questions
        public virtual void Move(Vector3 direction) { }
        
        public virtual void SnapTurn(Direction dir, float angle)
        {
            angle = dir == Direction.West ? -angle : angle;

            transform.RotateAround(head.position, Vector3.up, angle);

            StartCoroutine(WaitForTurn(turnCooldown));
        }

        public virtual void SmoothTurn(Direction dir, float speed)
        {
            speed = dir == Direction.West ? -speed : speed;

            transform.Rotate(head.position, speed * Time.deltaTime);
        }

        private IEnumerator WaitForTurn(float waitTime)
        {
            waitForTurn = true;

            yield return new WaitForSecondsRealtime(waitTime);

            waitForTurn = false;
        }
    }

    public class MovementOverride
    {
        public int priority = 0;

        public virtual Vector3 ProcessMovement(Vector3 direction)
        {
            return direction;
        }
    }
}
