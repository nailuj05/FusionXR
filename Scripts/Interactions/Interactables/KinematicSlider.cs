using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class KinematicSlider : KinematicInteractable
    {
        public bool shouldRelease;

        public Vector3 lineStart;
        public Vector3 lineEnd;

        public Vector3 lineStartGlobal { get { return transform.parent.TransformPoint(lineStart); } }
        public Vector3 lineEndGlobal { get { return transform.parent.TransformPoint(lineEnd); } }

        private Vector3 globalLinePoint => transform.parent.TransformPoint((lineStart + lineEnd) * 0.5f);
        private Vector3 globalLineDir => transform.parent.TransformDirection(lineEnd - lineStart);

        private float globalLineLength => Vector3.Distance(transform.parent.TransformVector(lineStart), transform.parent.TransformVector(lineEnd));

        protected override void InteractionStart()
        {
            if (attachedHands.Count == 0) return;
        }

        protected override void InteractionUpdate()
        {
            transform.position = Utils.ClosestPointOnLine(globalLinePoint, globalLineDir, globalLineLength, GetMeanPosition());
        }

        protected override void InteractionEnd()
        {

        }

        public void yesCanBeGrabbed()
        {
            canBeGrabbed = true;
        }

        public void noCanBeGrabbed()
        {
            canBeGrabbed = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(lineStartGlobal, lineEndGlobal);
        }
#endif
    }
}
