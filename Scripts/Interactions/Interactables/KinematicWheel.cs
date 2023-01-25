using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class KinematicWheel : KinematicInteractable
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

            gripPosition = attachedHands[0].gripPosition.position;

            offsetAngle = Vector3.SignedAngle(LocalAngleSetup(GetMeanPosition()), LocalAngleSetup(gripPosition), axis);
            startAngle = transform.localEulerAngles;
        }

        Vector3 startAngle;

        protected override void InteractionUpdate()
        {
            var angle = Vector3.SignedAngle(LocalAngleSetup(GetMeanPosition()), LocalAngleSetup(gripPosition), axis); // * Mathf.Rad2Deg;
            var offsettedAngle = offsetAngle - angle;

            transform.localEulerAngles = startAngle + axis * offsettedAngle;
        }

        protected override void InteractionEnd()
        {

        }

        Vector3 LocalAngleSetup(Vector3 pos)
        {
            return Vector3.ProjectOnPlane(transform.InverseTransformPoint(pos).normalized, axis);
        }
    }
}