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

        private Hand currentHand;

        public void ChangeCurrentHand(Hand newHand)
        {
            currentHand = newHand;
        }

        private Transform alignedTransform;
        public Transform AlignedTransform
        {
            get
            {
                if (alignedTransform == null)
                {
                    alignedTransform = new GameObject("alignedTransform").transform;
                    alignedTransform.SetParent(transform.parent);
                }

                return alignedTransform;
            }
        }

        public GrabPointType grabPointType;
        public bool alignForLeftHand = false;
        private Transform tempLeftTransform;

        public Vector3 leftHandAddedRotation;
        public Vector3 leftHandAddedPosition;

        protected bool isActive = true;

        public bool hasCustomPose { get { return (bool)pose; } }
        public HandPose pose;

        public virtual bool IsGrabPossible(Transform handTransform, Hand hand, TwoHandedModes twoHandedMode)
        {
            ChangeCurrentHand(hand);

            //If hands match or both hands are accepted and if the grab Point is free or can be switched
            if (((int)hand == (int)grabPointType || grabPointType == GrabPointType.Both) && (isActive || twoHandedMode == TwoHandedModes.SwitchHand))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateAlignedPoint()
        {
            if (currentHand == Hand.Left & alignForLeftHand)
            {
                AlignedTransform.position = transform.TransformPoint(leftHandAddedPosition);
                AlignedTransform.rotation = transform.rotation * Quaternion.Euler(leftHandAddedRotation);
            }
            else
            {
                AlignedTransform.position = transform.position;
                AlignedTransform.rotation = transform.rotation;
            }
        }

        public void RemoveAlignedForEditor()
        {
            if (Application.isPlaying)
                Destroy(alignedTransform.gameObject);
            else
                DestroyImmediate(alignedTransform.gameObject);
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
            //This needs to be called in all overrides aswell for the alignment to work
            UpdateAlignedPoint();
            return this;
        }

        private void OnDrawGizmos()
        {
            if (!(TryGetComponent<PoseEditor>(out PoseEditor pe) && pe.isEditingPose))
            {
                Mesh hand = Resources.Load<Mesh>("Hand") as Mesh;

                Vector3 scale = transform.localScale;
                Vector3 adjustedPalmOffset = palmOffset;

                if (grabPointType == GrabPointType.Left)
                {
                    scale -= 2 * Vector3.right;

                    adjustedPalmOffset.x *= -1;
                    adjustedPalmOffset.z *= -1;
                }

                Gizmos.color = new Color(0, 1, 0, .5f);
                Gizmos.DrawMesh(hand, transform.TransformPoint(adjustedPalmOffset), transform.rotation, scale * 0.01f);
            }
        }
    }
}
