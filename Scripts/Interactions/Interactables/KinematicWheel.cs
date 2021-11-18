using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class KinematicWheel : KinematicInteractable
    {
        private Vector3 localGrabPosition = Vector3.zero;

        public float angle = 0f;

        protected override void InteractionStart()
        {
            isInteracting = true;

            localGrabPosition = transform.InverseTransformPoint(attachedHands[0].grabPosition.position);
            angle = 0f;
        }

        protected override void InteractionUpdate()
        {
            var targetPos = transform.TransformPoint(localGrabPosition);
            var deltaAngle = Vector3.SignedAngle(LocalAngleSetup(attachedHands[0].targetPosition), LocalAngleSetup(targetPos), axis);

            angle = Mathf.MoveTowardsAngle(angle, deltaAngle, 2f);

            //transform.rotation = Quaternion.Euler(transform.InverseTransformDirection(axis) * deltaAngle);
            transform.rotation *= Quaternion.Euler(transform.InverseTransformDirection(axis) * angle);
            //transform.Rotate(transform.InverseTransformDirection(axis), deltaAngle, Space.Self);
            //transform.Rotate(axis, deltaAngle, Space.World);
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