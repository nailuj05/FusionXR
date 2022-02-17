using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations;

namespace Fusion.XR
{
    public class LocoSphereMover : Movement
    {
        private PhysicsBody body;

        [Header("Torques and Accelerations")]
        [SerializeField]
        private Rigidbody LocoSphere;

        [SerializeField]
        private ForceMode forceMode = ForceMode.VelocityChange;

        [SerializeField]
        private float torque = 5f;

        [SerializeField] [Range(0.1f, 1f)]
        private float accelerationTime = 0.5f;

        [SerializeField]
        private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 0.5f, 1);

        [SerializeField] [Range(0.1f, 1f)]
        private float decelerationTime = 0.33f;

        [SerializeField]
        private AnimationCurve decelerationCurve = AnimationCurve.EaseInOut(0, 1, 0.5f, 0);

        [Header("Crouch and Jump")]
        [SerializeField]
        private InputActionReference jumpReference;

        [SerializeField] [ReadOnly]
        private PlayerState playerState = PlayerState.Standing;

        [SerializeField]
        private float jumpForce = 1000f;

        [SerializeField]
        private float crouchHeight = 1.3f;

        #region Private Vars
        private Vector3 currentMove;
        private Vector3 torqueVec;
        private Vector3 vel;

        private float currentTorque;

        private float timeSinceMoveStarted = 0;
        private float timeSinceMoveEnded = 0;
        #endregion

        private void Awake()
        {
            body = GetComponent<PhysicsBody>();

            jumpReference.action.performed += OnJump;
        }

        private void FixedUpdate()
        {
            LocoSphere.freezeRotation = true;

            currentTorque = UpdateTorqueAcceleration();

            ApplyTorque();
        }

        public override void Move(Vector3 direction)
        {
            currentMove = direction;
        }

        #region Jumping

        public void OnJump(InputAction.CallbackContext obj) { OnJump(); }

        public void OnJump()
        {
            body.StartJump(jumpForce);
        }

        #endregion

        #region Torque
        private float UpdateTorqueAcceleration()
        {
            if (currentMove.sqrMagnitude > 0)
            {
                timeSinceMoveStarted += Time.fixedDeltaTime / accelerationTime;
                timeSinceMoveEnded = 0;

                return accelerationCurve.Evaluate(timeSinceMoveStarted) * torque;
            }
            else
            {
                timeSinceMoveEnded += Time.fixedDeltaTime / decelerationTime;
                timeSinceMoveStarted = 0;

                return decelerationCurve.Evaluate(timeSinceMoveEnded) * torque;
            }
        }

        private void ApplyTorque()
        {
            if (currentTorque > 0)
            {
                torqueVec = Vector3.Cross(currentMove, Vector3.down);

                LocoSphere.AddTorque(torqueVec * currentTorque, forceMode);

                LocoSphere.freezeRotation = false;
            }

            currentMove = Vector3.zero;
        } 
        #endregion
    } 
}
