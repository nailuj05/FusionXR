using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fusion.XR
{
    public abstract class Movement : MonoBehaviour
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
        public bool usesGravity = false;

        public Vector3 gravity = Physics.gravity;

        //Ground Check: Raycast from players head downwards, with max distance being the players height 
        //+ a small epsilon and the radius of the players collider
        protected bool isGrounded => Physics.SphereCast(head.position, Player.main.collisionAdjuster.p_CollisionRadius,
                Vector3.down, out RaycastHit hit, Player.main.collisionAdjuster.p_localHeight * 2);

        private Vector3 currentVelocity;
        public Vector3 CurrentVelocity {
            get { return currentVelocity; }
            protected set { currentVelocity = value; }
        }

        private List<MovementOverride> movementOverrides = new List<MovementOverride>();

        private bool waitForTurn = false;

        private void Awake()
        {
            head = Player.main.head;
        }

        private void Start()
        {
            //Subscribe to Movement Actions
            movementAction.action.performed += PreprocessInput;
            turnAction.action.performed += Turn; //This working with overrides?

            //Set Movement Direction Initially
            SetMovementDirection(movementDirection);

            //Add the standard Movement Override
            movementOverrides.Add(new MovementOverride());
        }

        private void Update()
        {
            if(usesGravity && canMove)
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

        /// <summary>
        /// Subscribes to the Movement Action and transforms the Vec2 Input to Vec3
        /// </summary>
        /// <param name="obj"></param>
        public virtual void PreprocessInput(InputAction.CallbackContext obj)
        {
            if (!canMove)
                return;

            //Transform to Vec3
            Vector3 movementInput = movementAction.action.ReadValue<Vector2>();
            movementInput.Set(movementInput.x, 0, movementInput.y);

            PreprocessMovement(movementInput);
        }

        /// <summary>
        /// Handles the movementInput and queues a move
        /// </summary>
        /// <param name="movementInput"></param>
        public virtual void PreprocessMovement(Vector3 movementInput)
        {
            if (movementInput.magnitude >= activationThreshold)
            {
                movementInput = currentMovementDirection.TransformDirection(movementInput);
                movementInput = Vector3.ProjectOnPlane(movementInput, Vector3.up);

                QueueMove(movementInput, playerSpeed);
            }
        }

        /// <summary>
        /// Queues a move.
        /// Queueing Moves allowes different preprocessing and multiple Movements being applied
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="speed"></param>
        public void QueueMove(Vector3 direction, float speed)
        {
            QueueMove(direction.normalized * speed);
        }

        /// <summary>
        /// Queues a move.
        /// Queueing Moves allowes different preprocessing and multiple Movements being applied.
        /// </summary>
        /// <param name="direction"></param>
        public void QueueMove(Vector3 direction)
        {
            MovementProcessing(direction);
        }

        /// <summary>
        /// Processes the movement by applying movement overrides.
        /// </summary>
        /// <param name="direction"></param>
        public void MovementProcessing(Vector3 direction)
        {
            direction = movementOverrides[movementOverrides.Count - 1].ProcessMovement(direction);

            Move(direction);
        }

        /// <summary>
        /// Adds an override and sorts it into the stack
        /// </summary>
        /// <param name="newMovementOverride"></param>
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

        /// <summary>
        /// Removes an movement override
        /// </summary>
        /// <param name="movementOverrideToRemove"></param>
        public void RemoveMovementOverride(MovementOverride movementOverrideToRemove)
        {
            movementOverrides.Remove(movementOverrideToRemove);
        }

        #endregion

        //Overriden by Movers
        public abstract void Move(Vector3 direction);
        
        /// <summary>
        /// Performces a Snap Turn
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="angle"></param>
        public virtual void SnapTurn(Direction dir, float angle)
        {
            angle = dir == Direction.West ? -angle : angle;

            transform.RotateAround(head.position, Vector3.up, angle);

            StartCoroutine(WaitForTurn(turnCooldown));
        }

        /// <summary>
        /// Performs a smooth turn
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="speed"></param>
        public virtual void SmoothTurn(Direction dir, float speed)
        {
            speed = dir == Direction.West ? -speed : speed;

            transform.Rotate(head.position, speed * Time.deltaTime);
        }

        /// <summary>
        /// Waits until next Turn is possible
        /// </summary>
        /// <param name="waitTime"></param>
        /// <returns></returns>
        private IEnumerator WaitForTurn(float waitTime)
        {
            waitForTurn = true;

            yield return new WaitForSecondsRealtime(waitTime);

            waitForTurn = false;
        }
    }

    //TODO, extent to base and default classes making base class abstract
    /// <summary>
    /// Base MovementOverride and default override
    /// </summary>
    public class MovementOverride
    {
        public int priority = 0;

        public virtual Vector3 ProcessMovement(Vector3 direction)
        {
            return direction;
        }
    }
}
