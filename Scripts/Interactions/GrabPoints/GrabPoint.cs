using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public enum GrabPointType
    {
        Left = 0,
        Right = 1,
        Both = 2
    }

    public class GrabPoint : MonoBehaviour
    {
        private Vector3 palmOffset = new Vector3(-0.035f, -0.021f, -0.0012f);

        public GrabPointType grabPointType;

        protected bool isActive = true;

        public bool hasCustomPose;
        public HandPose pose;

        private void OnDrawGizmos()
        {
            if(!(TryGetComponent<PoseEditor>(out PoseEditor pe) && pe.isEditingPose))
            {
                Mesh hand = Resources.Load<Mesh>("Hand") as Mesh;

                Vector3 scale = transform.localScale;
                Vector3 adjustedPalmOffset = palmOffset;

                if(grabPointType == GrabPointType.Left)
                {
                    scale -= 2 * Vector3.right;

                    adjustedPalmOffset.x *= -1;
                    adjustedPalmOffset.z *= -1;
                }

                Gizmos.color = new Color(0, 1, 0, .5f);
                Gizmos.DrawMesh(hand, transform.TransformPoint(adjustedPalmOffset), transform.rotation, scale * 0.01f);
            }
        }

        public virtual bool IsGrabPossible(Transform handTransform, Hand hand)
        {
            //If hands match or both hands are accepted
            if(((int)hand == (int)grabPointType || grabPointType == GrabPointType.Both) && isActive)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void BlockGrabPoint()
        {
            isActive = false;
        }

        public virtual void ReleaseGrabPoint()
        {
            isActive = true;
        }

        public virtual GrabPoint GetAligned(Transform hand)
        {
            return this;
        }
    }
}
