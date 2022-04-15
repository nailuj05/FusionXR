using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    [ExecuteAlways]
    public class GrabPointLine : GrabPointAlign
    {
        public bool alignHand = true;

        [Header("Line Start/End | Defined in parent space")]
        public Vector3 lineStart;
        public Vector3 lineEnd;

        public Vector3 lineStartGlobal  { get { return transform.parent.TransformPoint(lineStart);  } }
        public Vector3 lineEndGlobal    { get { return transform.parent.TransformPoint(lineEnd);    } }

        private Vector3 globalLinePoint => transform.parent.TransformPoint((lineStart + lineEnd) * 0.5f);
        private Vector3 globalLineDir => transform.parent.TransformDirection(lineEnd - lineStart);

        private float globalLineLength => Vector3.Distance(lineStartGlobal, lineEndGlobal);

        public override GrabPoint GetAligned(Transform hand)
        {
            transform.position = Utils.ClosestPointOnLine(globalLinePoint, globalLineDir, globalLineLength, hand.position);

            //Align rotation
            if (alignHand)
            {
                AlignPoint(transform, hand, allowFlip);
            }

            UpdateAlignedPoint();

            return this;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(lineStartGlobal, lineEndGlobal);
        }
#endif
    } 
}
