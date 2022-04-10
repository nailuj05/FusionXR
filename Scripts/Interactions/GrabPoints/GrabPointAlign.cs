using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class GrabPointAlign : GrabPoint
    {
        public override GrabPoint GetAligned(Transform hand)
        {
            AlignPoint(transform, hand);

            UpdateAlignedPoint();
            return this;
        }

        public static void AlignPoint(Transform point, Transform hand)
        {
            var right = point.right;
            var ogRot = point.rotation;
            var forward = Vector3.ProjectOnPlane(hand.forward, point.right);

            point.rotation = Quaternion.LookRotation(forward, point.up);

            //If the hand is upside down recalculate with inverted right axis
            if (Vector3.Dot(right, point.right) < 0)
            {
                //Debug.Log("Wrong way");
                point.rotation = ogRot;
                point.rotation = Quaternion.LookRotation(forward, -point.up);
            }
        }
    }
}