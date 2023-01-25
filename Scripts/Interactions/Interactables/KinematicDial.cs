using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class KinematicDial : KinematicInteractable
    {
        private Vector3 gripPosition = Vector3.zero;
        private float offsetAngle = 0f;

        private void Start()
        {
            allowCollisionInteraction = false;
        }

        protected override void InteractionStart()
        {
            if (attachedHands.Count == 0) return;

            isInteracting = true;

            gripPosition = attachedHands[0].gripPosition.position;

            offsetAngle = Vector3.SignedAngle(LocalAngleSetup(GetMeanPosition()), LocalAngleSetup(gripPosition), axis);
            startAngles = transform.localEulerAngles;
        }

        Vector3 startAngles;

        protected override void InteractionUpdate()
        {
            var deltaAngle = offsetAngle - Vector3.SignedAngle(LocalAngleSetup(GetMeanPosition()), LocalAngleSetup(gripPosition), axis);

            transform.localEulerAngles = startAngles + axis * deltaAngle;
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