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

        [Header("Passive Joint")]
        [SerializeField] public float rotationPower = 35f;
        [SerializeField] public float rotationDampener = 0.5f;

        //Force Tracking
        [Header("Force Tracking")]
        [SerializeField] public float forcePositionMultiplier = 200f;
        [SerializeField] public float forceRotationMultiplier = 35f;
        [SerializeField] public float forceDampener   = -10f;
        [SerializeField] public float maxForceClamp   = 150f;

        //Active Joint Tracking
        [Header("Active Joint Tracking")]
        [SerializeField] public float positionSpring = 3000;
        [SerializeField] public float positionDamper = 250;
        [SerializeField] public float maxJointForce  = 1500;

        [SerializeField] public float slerpSpring   = 3000;
        [SerializeField] public float slerpDamper   = 250;
        [SerializeField] public float slerpMaxForce = 1500;

        [SerializeField] public bool adjustAnchor = true;
        [SerializeField] public float limit = 0.7f;

        [HideInInspector] public GameObject tracker;
        [HideInInspector] public Transform palm;
    }

    public abstract class TrackDriver
    {
        protected Transform objectToTrack;

        protected TrackingBase trackingBase;

        public abstract void StartTrack(Transform assignedObjectToTrack, TrackingBase assignedTrackingBase);

        public abstract void UpdateTrack(Vector3 targetPosition, Quaternion targetRotation);

        public abstract void EndTrack();
    }

    public class KinematicDriver : TrackDriver
    {
        public override void StartTrack(Transform assignedObjectToTrack, TrackingBase assignedTrackingBase)
        {
            objectToTrack = assignedObjectToTrack;
            trackingBase = assignedTrackingBase;
        }

        public override void UpdateTrack(Vector3 targetPosition, Quaternion targetRotation)
        {
            objectToTrack.position = targetPosition;
            objectToTrack.rotation = targetRotation;
        }

        public override void EndTrack()
        {

        }
    }

    public class VelocityDriver : TrackDriver
    {
        private Rigidbody rb;

        public override void StartTrack(Transform assignedObjectToTrack, TrackingBase assignedTrackingBase)
        {
            objectToTrack = assignedObjectToTrack;
            trackingBase = assignedTrackingBase;

            rb = objectToTrack.GetComponent<Rigidbody>();
        }

        public override void UpdateTrack(Vector3 targetPosition, Quaternion targetRotation)
        {
            //Track Position
            Vector3 deltaVelocity = (targetPosition - objectToTrack.position) * trackingBase.positionStrength;

            rb.velocity = deltaVelocity;

            //Track Rotation
            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(objectToTrack.rotation);

            deltaRotation.ToAngleAxis(out var angle, out var axis);

            if (angle > 180f)
            {
                angle -= 360;
            }

            if (Mathf.Abs(axis.sqrMagnitude) != Mathf.Infinity)
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

        public override void StartTrack(Transform assignedObjectToTrack, TrackingBase assignedTrackingBase)
        {
            objectToTrack = assignedObjectToTrack;
            trackingBase = assignedTrackingBase;

            jointRB = Object.FindObjectOfType<Player>().Rigidbody;
            objectRB = objectToTrack.GetComponent<Rigidbody>();

            SetupJoint();
            UpdateHandJointDrives();
        }

        public override void UpdateTrack(Vector3 targetPosition, Quaternion targetRotation)
        {
            TrackPositionRotation(targetPosition, targetRotation);
            //UpdateTargetVelocity(targetPosition);
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

            activeJoint.xMotion = activeJoint.yMotion = activeJoint.zMotion = ConfigurableJointMotion.Limited;

            var limit = new SoftJointLimit();
            limit.limit = trackingBase.limit;
            activeJoint.linearLimit = limit;

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

            activeJoint.targetPosition = jointRB.transform.InverseTransformPoint(targetPos) - activeJoint.anchor;
            activeJoint.targetRotation = Quaternion.Inverse(jointRB.rotation) * targetRot;
        }

        private void UpdateHandJointDrives()
        {
            if (trackingBase.adjustAnchor)
                activeJoint.anchor = jointRB.transform.InverseTransformPoint(Player.main.head.position);

            var drive = new JointDrive();
            drive.positionSpring = trackingBase.positionSpring;
            drive.positionDamper = trackingBase.positionDamper;
            drive.maximumForce = trackingBase.maxJointForce;

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

        private ConfigurableJoint joint;

        private Quaternion initalRotation;

        public override void StartTrack(Transform assignedObjectToTrack, TrackingBase assignedTrackingBase)
        {
            objectToTrack = assignedObjectToTrack;
            trackingBase = assignedTrackingBase;

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

            //Track Rotation
            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(objectToTrack.rotation);

            deltaRotation.ToAngleAxis(out var angle, out var axis);

            if (angle > 180f)
            {
                angle -= 360;
            }

            if (Mathf.Abs(axis.sqrMagnitude) != Mathf.Infinity)
            {
                //objectRB.angularVelocity = axis * (angle * trackingBase.rotationStrength * Mathf.Deg2Rad);
                objectRB.AddTorque(axis * (angle * trackingBase.rotationPower * Mathf.Deg2Rad) / objectRB.mass);
                objectRB.AddTorque(-objectRB.angularVelocity * trackingBase.rotationDampener / objectRB.mass);
            }
        }

        public override void EndTrack()
        {
            DestroyJoint();
        }

        private void SetupJoint(Vector3 targetPosition, Quaternion targetRotation)
        {
            joint = objectRB.gameObject.AddComponent<ConfigurableJoint>();
            
            joint.anchor = objectRB.transform.InverseTransformPoint(trackerRB.position);
            joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedBody = trackerRB;
            joint.connectedAnchor = Vector3.zero;

            joint.rotationDriveMode = RotationDriveMode.Slerp;

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

    public class FixedJointDriver : TrackDriver
    {
        private Rigidbody objectRB;
        private Rigidbody trackerRB;

        private FixedJoint joint;

        private Quaternion initalRotation;

        public override void StartTrack(Transform assignedObjectToTrack, TrackingBase assignedTrackingBase)
        {
            objectToTrack = assignedObjectToTrack;
            trackingBase = assignedTrackingBase;

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
            if (!objectRB.isKinematic)
            {
                objectRB.transform.rotation = targetRotation;
            }

            joint = objectRB.gameObject.AddComponent<FixedJoint>();

            joint.anchor = Vector3.zero;
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedBody = trackerRB;
            joint.connectedAnchor = Vector3.zero;

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

    public class ForceDriver : TrackDriver
    {
        private Rigidbody objectRigidbody;

        private Rigidbody trackerRigidbody;

        public override void StartTrack(Transform assignedObjectToTrack, TrackingBase assignedTrackingBase)
        {
            objectToTrack = assignedObjectToTrack;
            trackingBase = assignedTrackingBase;

            objectRigidbody = objectToTrack.GetComponent<Rigidbody>();
            trackerRigidbody = trackingBase.tracker.GetComponent<Rigidbody>();
        }

        public override void UpdateTrack(Vector3 targetPosition, Quaternion targetRotation)
        {
            //Track Position
            Vector3 velocity = trackerRigidbody.GetPointVelocity(targetPosition);
            Vector3 force = (targetPosition - objectToTrack.position) * trackingBase.forcePositionMultiplier;

            Vector3 acceleration = Vector3.ClampMagnitude(force, trackingBase.maxForceClamp) / objectRigidbody.mass;

            objectRigidbody.AddForce(acceleration, ForceMode.Acceleration);
            objectRigidbody.AddForce(trackingBase.forceDampener * velocity / Mathf.Sqrt(objectRigidbody.mass), ForceMode.Acceleration);

            //Track Rotation
            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(objectToTrack.rotation);

            deltaRotation.ToAngleAxis(out var angle, out var axis);

            if (angle > 180f)
            {
                angle -= 360;
            }

            if (Mathf.Abs(axis.sqrMagnitude) != Mathf.Infinity)
            {
                objectRigidbody.angularVelocity = axis * (angle * trackingBase.forceRotationMultiplier * Mathf.Deg2Rad);
            }
        }

        public override void EndTrack()
        {

        }
    }
}