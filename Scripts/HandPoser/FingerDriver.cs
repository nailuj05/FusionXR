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

        [SerializeField] public float fingerMass;
        [SerializeField] public float fingerDrag;
        [SerializeField] public float fingerAngularDrag;

        [HideInInspector]
        public Transform[] fingerBones;

        [HideInInspector]
        public Rigidbody hand;
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
                var lastRot = fingers[i].localRotation;
                fingers[i].localRotation = Quaternion.Lerp(lastTargetRotations[i], targetRotations[i], currentLerp);

                Collider[] colliders = Physics.OverlapSphere(fingers[i].TransformPoint(Finger.GetFingerCollisionOffset(i, trackingBase)), trackingBase.radius, trackingBase.collMask);

                //Reset rotation if we hit something
                if (colliders.Length > 0)
                {
                    fingers[i].localRotation = lastRot;
                }
            }
        }

        public override void EndTrack()
        {

        }
    }

    public class JointDriver : FingerDriver
    {
        public Quaternion[] initalRotations = new Quaternion[3];
        public ConfigurableJoint[] configurableJoints;

        public override void StartTrack(FingerTrackingBase fingerTrackingBase)
        {
            fingers = fingerTrackingBase.fingerBones;
            trackingBase = fingerTrackingBase;

            initalRotations = new Quaternion[fingers.Length];
            configurableJoints = new ConfigurableJoint[fingers.Length];

            configurableJoints[0] = trackingBase.hand.gameObject.AddComponent<ConfigurableJoint>();

            JointDrive slerpDrive = new JointDrive();
            slerpDrive.positionSpring = trackingBase.slerpSpring;
            slerpDrive.positionDamper = trackingBase.slerpDamper;
            slerpDrive.maximumForce   = trackingBase.slerpMaxForce;

            for (int i = 0; i < fingers.Length; i++)
            {
                initalRotations[i] = fingers[i].localRotation;

                var rb = fingers[i].gameObject.AddComponent<Rigidbody>();
                rb.mass = trackingBase.fingerMass;
                rb.drag = trackingBase.fingerDrag;
                rb.angularDrag  = trackingBase.fingerAngularDrag;

                rb.gameObject.layer = LayerMask.NameToLayer("Fingers");

                if (i != 0)
                    configurableJoints[i] = fingers[i - 1].gameObject.AddComponent<ConfigurableJoint>();

                configurableJoints[i].autoConfigureConnectedAnchor = true;
                configurableJoints[i].anchor = configurableJoints[i].connectedAnchor = Vector3.zero;

                configurableJoints[i].connectedBody = rb;
                configurableJoints[i].rotationDriveMode = RotationDriveMode.Slerp;
                configurableJoints[i].xMotion = configurableJoints[i].yMotion = configurableJoints[i].zMotion = ConfigurableJointMotion.Locked;
                configurableJoints[i].angularXMotion = configurableJoints[i].angularYMotion = configurableJoints[i].angularZMotion = ConfigurableJointMotion.Locked;

                configurableJoints[i].slerpDrive = slerpDrive;
            }
        }

        public override void UpdateTrack(Quaternion[] lastTargetRotations, Quaternion[] targetRotations, float currentLerp)
        {
            for (int i = 0; i < fingers.Length; i++)
            {
                configurableJoints[i].SetTargetRotationLocal(targetRotations[i], initalRotations[i]);
            }
        }

        public override void EndTrack()
        {
            for (int i = 0; i < fingers.Length; i++)
            {
                GameObject.Destroy(configurableJoints[i].connectedBody);
                GameObject.Destroy(configurableJoints[i]);
            }
        }
    }
}