using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Fusion.XR
{
    public class LocoSphereMover : Movement
    {
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

        #region Private Vars
        private Vector3 currentMove;
        private Vector3 torqueVec;
        private Vector3 vel;

        private float currentTorque;

        private float timeSinceMoveStarted = 0;
        private float timeSinceMoveEnded = 0;
        #endregion

        private void FixedUpdate()
        {
            LocoSphere.freezeRotation = true;

            if(currentMove.sqrMagnitude > 0)
            {
                timeSinceMoveStarted += Time.fixedDeltaTime / accelerationTime;
                timeSinceMoveEnded = 0;

                currentTorque = accelerationCurve.Evaluate(timeSinceMoveStarted) * torque;
            }
            else
            {
                timeSinceMoveEnded += Time.fixedDeltaTime / decelerationTime;
                timeSinceMoveStarted = 0;

                currentTorque = decelerationCurve.Evaluate(timeSinceMoveEnded) * torque;
            }

            if(currentTorque > 0)
            {
                torqueVec = Vector3.Cross(currentMove, Vector3.down);

                LocoSphere.AddTorque(torqueVec * currentTorque, forceMode);

                LocoSphere.freezeRotation = false;
            }

            currentMove = Vector3.zero;
        }

        public override void Move(Vector3 direction)
        {
            currentMove = direction;
        }
    } 
}
