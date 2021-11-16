using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class KinematicWheel : KinematicInteractable
    {
        private Vector3 initalOffset;

        private Vector3 globalAxis;

        protected override void InteractionStart()
        {
            isInteracting = true;

            var controllerPos = attachedHands[0].targetPosition - transform.position;
            globalAxis = transform.TransformDirection(axis);

            //TODO: Fix this using the up axis instead of the user set axis
            initalOffset = Vector3.ProjectOnPlane((transform.up * controllerPos.magnitude) - controllerPos, globalAxis);

            Debug.DrawRay(transform.position, globalAxis, Color.blue, 10f);
        }

        protected override void InteractionUpdate()
        {
            var controllerPos = Vector3.ProjectOnPlane(attachedHands[0].targetPosition - transform.position, globalAxis);

            //var angles = Vector3.SignedAngle(controllerPos, transform.up, globalAxis);
            var angles = Vector3.Angle(controllerPos, transform.up);

            transform.rotation = Quaternion.LookRotation(controllerPos, globalAxis);
            //transform.localRotation = Quaternion.AngleAxis(angles, axis);
        }

        protected override void InteractionEnd()
        {
            isInteracting = false;
        }
    }
}

//if (attachedHands.Count == 1) //If there is one hand grabbing
//{
//    //Get GrabPoint Offsets
//    Vector3 offsetPos = attachedHands[0].grabPosition.localPosition;

//    //Delta Vector from Grabable (+ offset) to hand
//    targetPosition = attachedHands[0].targetPosition - transform.TransformVector(offsetPos);
//}
//else //If there is two hands grabbing 
//{
//    Vector3[] posTargets = new Vector3[attachedHands.Count];

//    for (int i = 0; i < attachedHands.Count; i++)
//    {
//        //Get GrabPoint Offsets
//        Vector3 offsetPos = attachedHands[i].grabPosition.localPosition;

//        //Delta Vector from Grabable (+ offset) to hand
//        posTargets[i] = attachedHands[i].targetPosition - transform.TransformVector(offsetPos);
//    }
//    //Average target transformation
//    targetPosition = Vector3.Lerp(posTargets[0], posTargets[1], 0.5f);
//}