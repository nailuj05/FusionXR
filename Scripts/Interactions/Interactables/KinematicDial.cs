using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class KinematicDial : KinematicInteractable
    {
        private Vector3 grabPosition = Vector3.zero;

        public float angle = 0f;

        private float refVel;

        protected override void InteractionStart()
        {
            isInteracting = true;

            grabPosition = attachedHands[0].grabPosition.position;
            angle = 0f;
        }

        protected override void InteractionUpdate()
        {
            var targetPos = transform.TransformPoint(grabPosition);

            var deltaAngle = Vector3.SignedAngle(LocalAngleSetup(attachedHands[0].targetPosition), LocalAngleSetup(grabPosition), axis);
            //var deltaAngle = Vector3.Angle(LocalAngleSetup(attachedHands[0].targetPosition), LocalAngleSetup(targetPos));

            Debug.Log(deltaAngle);

            transform.localEulerAngles = axis * deltaAngle;

            //transform.Rotate(axis, deltaAngle, Space.Self);
        }

        protected override void InteractionEnd()
        {
            isInteracting = false;
        }

        Vector3 LocalAngleSetup(Vector3 pos)
        {
            return Vector3.ProjectOnPlane(transform.InverseTransformPoint(pos).normalized, axis);
        }
    }

}