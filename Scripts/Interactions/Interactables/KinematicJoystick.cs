using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class KinematicJoystick : KinematicInteractable
    {
        public float Rad = 10;

        private Vector3 gripPosition = Vector3.zero;

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

            gripPosition = attachedHands[0].gripPosition.position;

            initialAxis = transform.parent.InverseTransformDirection(transform.TransformDirection(axis)).normalized;
        }

        protected override void InteractionUpdate()
        {
            var gripDir = GetMeanPosition() - transform.position;


            Debug.DrawRay(transform.position, gripDir);
            Debug.DrawRay(transform.position, transform.parent.TransformDirection(initialAxis), Color.blue);

            //transform.rotation = Quaternion.FromToRotation(transform.parent.TransformDirection(initialAxis), gripDir);
            //transform.localRotation = initialRot;
            var rot = Quaternion.FromToRotation(Vector3.up, gripDir);
                
            transform.rotation = rot;
        }

        protected override void InteractionEnd()
        {
            
        }
    }
}