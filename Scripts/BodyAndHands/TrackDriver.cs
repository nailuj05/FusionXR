using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Fusion.XR
{
    /// <summary>
    /// All Variables for the Tracking drivers are defined here, the Tracking Drivers get the Base in their StartTrack() Function
    /// </summary>
    [System.Serializable]
    public class TrackingBase
    {
        //Velocity Tracking
        [Header("Velocity Tracking")]
        [SerializeField] public float positionStrength = 15f;
        [SerializeField] public float rotationStrength = 35f;

        //Active Joint Tracking
        [Header("Active Joint Tracking")]
        [SerializeField] public float positionSpring = 3000;
        [SerializeField] public float positionDamper = 250;
        [SerializeField] public float maxForce = 1500;

        [SerializeField] public float slerpSpring = 3000;
        [SerializeField] public float slerpDamper = 250;
        [SerializeField] public float slerpMaxForce = 1500;

        [HideInInspector] public GameObject tracker;
    }

    public class TrackDriver
    {
        protected Transform trackTarget;

        protected TrackingBase trackingBase;

        public virtual void StartTrack(Transform objectToTrack, TrackingBase assignedTrackingBase)
        {
            trackTarget = objectToTrack;
            trackingBase = assignedTrackingBase;
        }

        public virtual void UpdateTrack(Vector3 targetPosition, Quaternion targetRotation)
        {

        }

        public virtual void EndTrack()
        {

        }
    }

    public class KinematicDriver : TrackDriver
    {
        public override void StartTrack(Transform objectToTrack, TrackingBase assignedTrackingBase)
        {
            base.StartTrack(objectToTrack, assignedTrackingBase);
        }

        public override void UpdateTrack(Vector3 targetPosition, Quaternion targetRotation)
        {
            trackTarget.position = targetPosition;
            trackTarget.rotation = targetRotation;
        }

        public override void EndTrack()
        {

        }
    }

    public class VelocityDriver : TrackDriver
    {
        private Rigidbody rb;

        public override void StartTrack(Transform objectToTrack, TrackingBase assignedTrackingBase)
        {
            rb = objectToTrack.GetComponent<Rigidbody>();
            base.StartTrack(objectToTrack, assignedTrackingBase);
        }

        public override void UpdateTrack(Vector3 targetPosition, Quaternion targetRotation)
        {
            //Track Position
            Vector3 deltaVelocity = (targetPosition - trackTarget.position) * trackingBase.positionStrength;

            rb.velocity = deltaVelocity;

            //Track Rotation
            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(trackTarget.rotation);

            deltaRotation.ToAngleAxis(out var angle, out var axis);

            if (angle > 180f)
            {
                angle -= 360;
            }

            if (Mathf.Abs(axis.magnitude) != Mathf.Infinity)
                rb.angularVelocity = axis * (angle * trackingBase.rotationStrength * Mathf.Deg2Rad);
        }

        public override void EndTrack()
        {

        }
    }

    public class ActiveJointDriver : TrackDriver
    {
        private ConfigurableJoint activeJoint;
        private Rigidbody jointRB;
        private Rigidbody objectRB;

        private Vector3 lastControllerPos;

        public override void StartTrack(Transform objectToTrack, TrackingBase assignedTrackingBase)
        {
            base.StartTrack(objectToTrack, assignedTrackingBase);

            jointRB = Object.FindObjectOfType<Player>().GetComponent<Rigidbody>();
            objectRB = objectToTrack.GetComponent<Rigidbody>();

            SetupJoint();
            UpdateHandJointDrives();
        }

        public override void UpdateTrack(Vector3 targetPosition, Quaternion targetRotation)
        {
            TrackPositionRotation(targetPosition, targetRotation);
            UpdateTargetVelocity(targetPosition);
        }

        public override void EndTrack()
        {
            DestroyJoint();
        }

        private void SetupJoint()
        {
            if (activeJoint != null)
                return;

            activeJoint = jointRB.gameObject.AddComponent<ConfigurableJoint>();
            activeJoint.connectedBody = objectRB;
            activeJoint.autoConfigureConnectedAnchor = false;
            activeJoint.anchor = Vector3.zero;
            activeJoint.connectedAnchor = Vector3.zero;

            activeJoint.enableCollision = false;
            activeJoint.enablePreprocessing = false;

            activeJoint.rotationDriveMode = RotationDriveMode.Slerp;
        }

        private void DestroyJoint()
        {
            if(activeJoint != null)
                Object.Destroy(activeJoint);

            activeJoint = null;
        }

        private void TrackPositionRotation(Vector3 targetPos, Quaternion targetRot)
        {
            if (activeJoint != null && Time.frameCount % 10 == 0)
            {
                UpdateHandJointDrives();
            }

            activeJoint.targetPosition = jointRB.transform.InverseTransformPoint(targetPos);
            activeJoint.targetRotation = Quaternion.Inverse(jointRB.rotation) * targetRot;
        }

        private void UpdateHandJointDrives()
        {
            var drive = new JointDrive();
            drive.positionSpring = trackingBase.positionSpring;
            drive.positionDamper = trackingBase.positionDamper;
            drive.maximumForce = trackingBase.maxForce;

            activeJoint.xDrive = activeJoint.yDrive = activeJoint.zDrive = drive;

            var slerpDrive = new JointDrive();
            slerpDrive.positionSpring = trackingBase.slerpSpring;
            slerpDrive.positionDamper = trackingBase.slerpDamper;
            slerpDrive.maximumForce = trackingBase.slerpMaxForce;

            activeJoint.slerpDrive = slerpDrive;
        }

        private void UpdateTargetVelocity(Vector3 targetPos)
        {
            var currentControllerPos = objectRB.transform.InverseTransformPoint(targetPos);
            var velocity = (currentControllerPos - lastControllerPos) / Time.fixedDeltaTime;
            lastControllerPos = currentControllerPos;

            activeJoint.targetVelocity = velocity;
        }
    }

    public class PassiveJointDriver : TrackDriver
    {
        private Rigidbody objectRB;
        private Rigidbody trackerRB;

        private Joint joint;

        public override void StartTrack(Transform objectToTrack, TrackingBase assignedTrackingBase)
        {
            base.StartTrack(objectToTrack, assignedTrackingBase);

            try
            {
                trackerRB = trackingBase.tracker.GetComponent<Rigidbody>();
                objectRB = objectToTrack.GetComponent<Rigidbody>();
            }
            catch
            {
                Debug.Log("Target and Tracking Object need to have a Rigidbody attached, " +
                    "this should be used for grabbing, not for moving the hand (Use Active Joint Tracking for that");
            }
        }

        public override void UpdateTrack(Vector3 targetPosition, Quaternion targetRotation)
        {
            if (joint == null)
                SetupJoint(targetPosition, targetRotation);
        }

        public override void EndTrack()
        {
            DestroyJoint();
        }

        private void SetupJoint(Vector3 targetPosition, Quaternion targetRotation)
        {
            objectRB.transform.position = targetPosition;
            objectRB.transform.rotation = targetRotation;

            joint = trackerRB.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = objectRB;

            joint.enableCollision = false;
            joint.enablePreprocessing = false;
        }

        private void DestroyJoint()
        {
            if (joint != null)
                Object.Destroy(joint);

            joint = null;
        }
    }
}