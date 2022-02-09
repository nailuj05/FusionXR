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

        private bool isTurning;
        private InputAction.CallbackContext context;

        private Transform currentMovementDirection => GetMovementDirection(movementDirection);

        public InputActionReference movementAction;
        public InputActionReference turnAction;

        [Range(0.05f, 0.9f)]
        public float activationThreshold = 0.1f;

        [Header("External Factors")]
        public bool canMove = true;
        [Tooltip("A custom gravity to apply. Don't use with a rigidbody that also uses gravity")]
        public bool usesCustomGravity = false;

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

        private void Start()
        {
            //Subscribe to Movement Actions
            movementAction.action.performed += PreprocessInput;
            turnAction.action.started += (c) => { isTurning = true; context = c; };
            turnAction.action.canceled += (c) => isTurning = false;

            head = Player.main.head;
            hand = Player.main.LeftHand.trackedController;

            //Add the standard Movement Override
            movementOverrides.Add(new MovementOverride());
        }

        private void Update()
        {
            if(usesCustomGravity && canMove)
            {
                QueueMove(gravity);
            }

            if (isTurning)
                Turn();
        }

        #region Turning
        private void Turn()
        {
            Turn(context);
        }

        private void Turn(InputAction.CallbackContext obj)
        {
            Vector2 turnDirection = obj.ReadValue<Vector2>();

            if (turnDirection.sqrMagnitude == 0) return;

            Direction direction = Utils.GetDirectionFromVector(turnDirection);

            Turn(direction);
        }

        public void Turn(Direction direction)
        {
            if (direction != Direction.North || direction != Direction.South)
            {
                if (turnMode == TurnMode.SnapTurn)
                {
                    SnapTurn(direction, turnAngle);
                }
                if (turnMode == TurnMode.SmoothTurn)
                {
                    SmoothTurn(direction, turnSpeed);
                }
            }
        }

        /// <summary>
        /// Performces a Snap Turn
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="angle"></param>
        public virtual void SnapTurn(Direction dir, float angle)
        {
            if (waitForTurn)
                return;

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

            transform.RotateAround(head.position, Vector3.up, speed * Time.deltaTime);
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

        #endregion

        public Transform GetMovementDirection(MovementDirection movementDirection)
        {
            if (movementDirection == MovementDirection.HandOriented)
            {
                return hand;
            }
            else
            {
                return head;
            }
        }

        #region Queuing, Processing, Overrides

        public virtual void PreprocessInput(InputAction.CallbackContext obj) { PreprocessInput(movementAction.action.ReadValue<Vector2>()); }

        Vector3 movementInput;

        /// <summary>
        /// Subscribes to the Movement Action and Transforms Vector to 3D and into worldspace
        /// </summary>
        /// <param name="obj"></param>
        public virtual void PreprocessInput(Vector2 moveInput)
        {
            if (!canMove)
                return;

            //Transform to Vec3
            movementInput.Set(moveInput.x, 0, moveInput.y);

            if (movementInput.magnitude >= activationThreshold)
            {
                movementInput = currentMovementDirection.TransformDirection(movementInput);
                //Debug.DrawRay(currentMovementDirection.position, movementInput, Color.red, 0.1f);

                QueueMove(movementInput.normalized * playerSpeed);
            }
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
        float mag;
        public void MovementProcessing(Vector3 direction)
        {
            direction = movementOverrides[movementOverrides.Count - 1].ProcessMovement(direction);
            mag = direction.magnitude;

            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            direction = direction.normalized * mag;

            Move(direction);
        }

        //Overriden by Movers
        public abstract void Move(Vector3 direction);

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
