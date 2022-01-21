using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public class GrabPointAngle : GrabPoint
    {
        public float maxAngle = 45f;

        public override bool IsGrabPossible(Transform handTransform, Hand hand, TwoHandedModes twoHandedMode)
        {
            //If hands match or both hands are accepted and if the grab Point is free or can be switched
            if (((int)hand == (int)grabPointType || grabPointType == GrabPointType.Both) && (isActive || twoHandedMode == TwoHandedModes.SwitchHand))
            {
                if (Vector3.Angle(handTransform.forward, transform.forward) <= maxAngle)
                {
                    return true;
                }
                return true;
            }
            return false;
        }
    }
}