using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    [System.Serializable]
    public class FingerTrackingBase
    {
        [SerializeField] public Vector3 offset = new Vector3(0, 0.4f, 0f);
        [SerializeField] public float radius = 0.0125f;
        [Tooltip("The objects the fingers can collide with, this should be everything but the hands")]
        [SerializeField] public LayerMask collMask;

        [SerializeField] public float slerpSpring = 3000;
        [SerializeField] public float slerpDamper = 250;
        [SerializeField] public float slerpMaxForce = 1500;

        [HideInInspector]
        public Transform[] fingerBones;
    }

    public abstract class FingerDriver
    {
        protected Transform[] fingers;
        protected FingerTrackingBase trackingBase;

        public abstract void StartTrack(FingerTrackingBase fingerTrackingBase);

        public abstract void UpdateTrack(Quaternion[] lastTargetRotations, Quaternion[] targetRotations, float currentLerp);

        public abstract void EndTrack();
    }

    public class KinematicFingerDriver : FingerDriver
    {
        public override void StartTrack(FingerTrackingBase fingerTrackingBase)
        {
            fingers = fingerTrackingBase.fingerBones;
            trackingBase = fingerTrackingBase;
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

        }
    }

    public class CollisionTestDriver : FingerDriver
    {
        public override void StartTrack(FingerTrackingBase fingerTrackingBase)
        {
            fingers = fingerTrackingBase.fingerBones;
            trackingBase = fingerTrackingBase;
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

        }
    }
}