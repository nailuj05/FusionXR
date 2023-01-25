using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public class GrabPointReach : GrabPoint
    {
        public float reachDistance = 0.2f;

        public override bool IsGrabPossible(Transform handTransform, Hand hand, TwoHandedModes twoHandedMode)
        {
            //If hands match or both hands are accepted and if the grab Point is free or can be switched
            if (((int)hand == (int)grabPointType || grabPointType == GrabPointType.Both) && (isActive || twoHandedMode == TwoHandedModes.SwitchHand))
            {
                if(Vector3.Distance(transform.position, handTransform.position) <= reachDistance)
                {
                    return true;
                }
            }
            return false;
        }

        public override void BlockGrabPoint()
        {
            isActive = false;
        }

        public override void ReleaseGrabPoint()
        {
            isActive = true;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 0, 1, .2f);
            Gizmos.DrawSphere(transform.position, reachDistance);
        }
#endif
    }
}
