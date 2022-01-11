using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class GrabPointAlign : GrabPoint
    {
        public override GrabPoint GetAligned(Transform hand)
        {
            var right = transform.right;
            var ogRot = transform.rotation;
            var forward = Vector3.ProjectOnPlane(hand.forward, transform.right);

            transform.rotation = Quaternion.LookRotation(forward, transform.up);

            //If the hand is upside down recalculate with inverted right axis
            if (Vector3.Dot(right, transform.right) < 0)
            {
                //Debug.Log("Wrong way");
                transform.rotation = ogRot;
                transform.rotation = Quaternion.LookRotation(forward, -transform.up);
            }

            return this;
        }
    }
}