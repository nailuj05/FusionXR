using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class KinematicJoystick : KinematicInteractable
    {
        private Vector3 grabPosition = Vector3.zero;

        private Vector3 initialAxis;

        protected override void InteractionStart()
        {
            if (attachedHands.Count == 0) return;

            isInteracting = true;

            grabPosition = attachedHands[0].grabPosition.position;

            initialAxis = transform.parent.InverseTransformDirection(transform.TransformDirection(axis));
        }

        protected override void InteractionUpdate()
        {
            
        }

        protected override void InteractionEnd()
        {
            
        }
    }
}