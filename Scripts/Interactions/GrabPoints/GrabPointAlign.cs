using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class GrabPointAlign : GrabPoint
    {
        public bool allowFlip = true;

        public override GrabPoint GetAligned(Transform hand)
        {
            AlignPoint(transform, hand, allowFlip);

            UpdateAlignedPoint();
            return this;
        }

        public void AlignPoint(Transform point, Transform hand, bool allowFlip)
        {
            var right = point.right;
            var handRight = hand.right;
            var ogRot = point.rotation;
            var forward = Vector3.ProjectOnPlane(hand.forward, point.right);

            point.rotation = Quaternion.LookRotation(forward, point.up);

            //If the hand is upside down recalculate with inverted right axis
            if (Vector3.Dot(right, point.right) < 0)
            {
                point.rotation = ogRot;
                point.rotation = Quaternion.LookRotation(forward, -point.up);
            }

            var al = GetAlignedTransform();
            if (Vector3.Dot(handRight, al.right) < 0)
            {
                point.localEulerAngles += new Vector3(0, 0, 180);
            }
        }
    }
}