using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class KinematicLever : KinematicInteractable
    {
        public Vector2 minMaxClamp = new Vector2(-45f, 45f);

        private Vector3 grabPosition = Vector3.zero;

        private float offsetAngle = 0f;

        private void Start()
        {
            allowCollisionInteraction = false;
        }

        protected override void InteractionStart()
        {
            if (attachedHands.Count == 0) return;

            grabPosition = attachedHands[0].grabPosition.position;

            offsetAngle = Vector3.SignedAngle(LocalAngleSetup(GetMeanPosition()), LocalAngleSetup(grabPosition), axis);

            startAngle = transform.localEulerAngles;
        }

        Vector3 startAngle;

        protected override void InteractionUpdate()
        {
            var angle = offsetAngle - Vector3.SignedAngle(LocalAngleSetup(GetMeanPosition()), LocalAngleSetup(grabPosition), axis);

            angle = Mathf.Clamp(angle, minMaxClamp.x, minMaxClamp.y);

            transform.localRotation = Quaternion.Euler(startAngle + axis * angle);
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