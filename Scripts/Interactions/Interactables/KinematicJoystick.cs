using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class KinematicJoystick : KinematicInteractable
    {
        public float Rad = 10;

        private Vector3 grabPosition = Vector3.zero;

        private Vector3 initialAxis;

        private Quaternion initialRot;

        private void Awake()
        {
            initialRot = transform.localRotation;
        }

        protected override void InteractionStart()
        {
            if (attachedHands.Count == 0) return;

            isInteracting = true;

            grabPosition = attachedHands[0].grabPosition.position;

            initialAxis = transform.parent.InverseTransformDirection(transform.TransformDirection(axis)).normalized;
        }

        protected override void InteractionUpdate()
        {
            var newAxis = GetMeanPosition() - transform.position;
            //newAxis = transform.parent.InverseTransformDirection(newAxis);
            newAxis.Normalize();

            //newAxis = Vector3.MoveTowards(initialAxis, newAxis, Rad);

            transform.localRotation = initialRot * Quaternion.FromToRotation(transform.parent.TransformDirection(initialAxis), newAxis);
        }

        protected override void InteractionEnd()
        {
            
        }
    }
}