using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    [System.Serializable]
    public class FingerTrackingBase
    {
        [SerializeField] public Vector3 offset;
        [SerializeField] public float radius;
        [SerializeField] public LayerMask collMask;

        [SerializeField] public float slerpSpring = 3000;
        [SerializeField] public float slerpDamper = 250;
        [SerializeField] public float slerpMaxForce = 1500;

        [HideInInspector] public Transform[] fingers;
    }

    public class FingerDriver
    {
        protected Transform[] fingers;
        protected FingerTrackingBase trackingBase;

        public virtual void StartTrack(FingerTrackingBase fingerTrackingBase)
        {
            fingers = fingerTrackingBase.fingers;
            trackingBase = fingerTrackingBase;
        }

        public virtual void UpdateTrack(Quaternion[] lastTargetRotations, Quaternion[] targetRotations, float currentLerp)
        {

        }

        public virtual void EndTrack()
        {

        }
    }

    public class KinematicFingerDriver : FingerDriver
    {
        public override void StartTrack(FingerTrackingBase fingerTrackingBase)
        {
            base.StartTrack(fingerTrackingBase);
        }

        public override void UpdateTrack(Quaternion[] lastTargetRotations, Quaternion[] targetRotations, float currentLerp)
        {
            for (int i = 0; i < targetRotations.Length; i++)
            {
                fingers[i].localRotation = Quaternion.Lerp(lastTargetRotations[i], targetRotations[i], currentLerp);
            }
        }

        public override void EndTrack()
        {
            base.EndTrack();
        }
    }

    public class CollisionTestDriver : FingerDriver
    {
        public override void StartTrack(FingerTrackingBase fingerTrackingBase)
        {
            base.StartTrack(fingerTrackingBase);
        }

        public override void UpdateTrack(Quaternion[] lastTargetRotations, Quaternion[] targetRotations, float currentLerp)
        {
            for (int i = 0; i < targetRotations.Length; i++)
            {
                Collider[] colliders = Physics.OverlapSphere(fingers[i].TransformPoint(Finger.GetFingerCollisionOffset(i, trackingBase)), trackingBase.radius, trackingBase.collMask);

                if (colliders.Length == 0) //Only rotate if we didn't hit anything
                {
                    fingers[i].localRotation = Quaternion.Lerp(lastTargetRotations[i], targetRotations[i], currentLerp);
                }
            }
        }

        public override void EndTrack()
        {
            base.EndTrack();
        }
    }
}