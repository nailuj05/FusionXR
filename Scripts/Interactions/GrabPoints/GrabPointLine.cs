using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    [ExecuteAlways]
    public class GrabPointLine : GrabPoint
    {
        public bool alignHand = true;

        [Header("Line Start/End | Defined in parent space")]
        public Vector3 lineStart;
        public Vector3 lineEnd;

        public Vector3 lineStartGlobal  { get { return transform.parent.TransformPoint(lineStart);  } }
        public Vector3 lineEndGlobal    { get { return transform.parent.TransformPoint(lineEnd);    } }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(lineStartGlobal, lineEndGlobal);
        }
#endif

        public override GrabPoint GetAligned(Transform hand)
        {
            Vector3 line = lineEndGlobal - lineStartGlobal;
            float lineLength = line.magnitude;

            Vector3 handVec = hand.position - lineStartGlobal;
            Vector3 handPos = Vector3.Project(handVec, line) + lineStartGlobal;

            Vector3 lineMid = (lineStartGlobal + lineEndGlobal) / 2;

            //If we need to cap the projected position
            if (Vector3.Distance(handPos, lineMid) > (lineLength / 2))
            {
                if (Vector3.Dot(handPos - lineMid, lineStartGlobal - lineMid) > 0)
                {
                    handPos = lineStartGlobal;
                }
                else if (Vector3.Dot(handPos - lineMid, lineEndGlobal - lineMid) > 0)
                {
                    handPos = lineEndGlobal;
                }
            }

            transform.position = handPos;

            if (alignHand)
            {
                GrabPointAlign.AlignPoint(transform, hand);
            }

            return this;
        }
    } 
}
