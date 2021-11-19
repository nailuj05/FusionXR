using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class KinematicWheel : KinematicInteractable
    {
        private Vector3 grabPosition = Vector3.zero;
        private float offsetAngle = 0f;

        protected override void InteractionStart()
        {
            isInteracting = true;

            grabPosition = attachedHands[0].grabPosition.position;

            offsetAngle = Vector3.SignedAngle(LocalAngleSetup(attachedHands[0].targetPosition), LocalAngleSetup(grabPosition), axis);
        }

        protected override void InteractionUpdate()
        {
            var deltaAngle = offsetAngle - Vector3.SignedAngle(LocalAngleSetup(attachedHands[0].targetPosition), LocalAngleSetup(grabPosition), axis);

            transform.localEulerAngles = axis * deltaAngle;
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