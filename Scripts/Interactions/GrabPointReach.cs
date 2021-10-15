using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public class GrabPointReach : GrabPoint
    {
        public float reachDistance = 0.2f;

        public override bool IsGrabPossible(Transform handTransform, Hand hand)
        {
            //If hands match or both hands are accepted
            if ((int)hand == (int)grabPointType || grabPointType == GrabPointType.Both)
            {
                if(Vector3.Distance(transform.position, handTransform.position) <= reachDistance)
                {
                    return true;
                }
            }
            return false;
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
