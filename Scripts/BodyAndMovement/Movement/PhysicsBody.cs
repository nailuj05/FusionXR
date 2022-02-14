using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class PhysicsBody : CollisionAdjuster
    {
        [Header("Body Settings")]
        [Range(0.1f, 0.9f)]
        public float chestPercent = 0.3f;
        [Range(0.1f, 0.9f)]
        public float legsPercent = 0.7f;

        [Header("Joint Settings")]
        public float jointStrength = 5000;
        public float jointDampener = 500;
        public float jointMaxStrength = 1000;

        [Header("Tracking Settings")]
        public float FenderHeight = 0.1f;

        private Vector3 headOffset;
        private Vector3 currentHeadOffset => Head.transform.TransformVector(headOffset);

        [Header("Rigidbodys")]
        public Rigidbody Head;
        public Rigidbody Chest;
        public Rigidbody Legs;
        public Rigidbody LocoSphere;

        [Header("Colliders")]
        public SphereCollider HeadCol;
        public CapsuleCollider ChestCol;
        public CapsuleCollider LegsCol;
        public SphereCollider FenderCol;
        public SphereCollider LocoSphereCollider;

        [Header("Joints")]
        public ConfigurableJoint ChestJoint;
        public ConfigurableJoint LegsJoint;
        private ConfigurableJoint HeadJoint;

        [Header("Debug Objects")]
        public bool renderDebugObjects = true;
        public GameObject d_LocoSphere;
        public GameObject d_Fender;
        public GameObject d_Legs;
        public GameObject d_Chest;

        private float actualHeight;

        #region Private vars to avoid frame allocations

        //Fixed Update
        Vector3 cameraPos;

        //Update Chest/Legs
        float colliderHeight;
        Vector3 positionToReach;

        //HandleHMDMovement
        Vector3 delta;
        Vector3 deltaHead;

        //HandleHMDRotation
        Quaternion deltaRot;
        private float step = 500;
        float lastEulers, newEulers, deltaEulers, targetEulers;

        //StopHorizontalMomentum
        Vector3 vel;
        #endregion

        private void Start()
        {
            Chest.WakeUp();
            HeadJoint = SetupJoint(Chest, Head);

            Player.main.Rigidbody = Chest;

            UpdateJointDrive(ChestJoint);
            UpdateJointDrive(LegsJoint);

            UpdateChest();
            UpdateLegs();
            //PlaceFender();

            Head.interpolation = RigidbodyInterpolation.Interpolate;
            Chest.interpolation = RigidbodyInterpolation.Interpolate;
            Legs.interpolation = RigidbodyInterpolation.Interpolate;
            LocoSphere.interpolation = RigidbodyInterpolation.Interpolate;

            ToggleDebugObjects(renderDebugObjects);
        }

        void FixedUpdate()
        {
            cameraPos = GetCameraGlobal();
            actualHeight = GetActualHeight();

            UpdateHead();
            UpdateChest();
            UpdateLegs();

            //Debug.Log($"{Mathf.Round(VRCamera.position.y * 100f) / 100f} {Mathf.Round(actualHeight * 100f) / 100f} {Mathf.Round(localHeight * 100f) / 100f}");

            HandleHMDMovement();
            HandleHMDRotation();
        }

        #region Body

        private void UpdateHead()
        {
            HeadJoint.targetPosition = Chest.transform.InverseTransformPoint(cameraPos);
        }

        private void UpdateChest()
        {
            colliderHeight = (localHeight * chestPercent) - LocoSphereCollider.radius;

            ChestJoint.targetPosition = new Vector3(0, -colliderHeight, 0);

            ChestCol.height = actualHeight - (actualHeight * chestPercent - LocoSphereCollider.radius);
        }

        private void UpdateLegs()
        {
            colliderHeight = (localHeight * chestPercent * legsPercent);

            LegsJoint.targetPosition = new Vector3(0, colliderHeight, 0);

            LegsCol.height = (actualHeight * chestPercent * legsPercent) + LocoSphereCollider.radius;
        }

        private void PlaceFender()
        {
            FenderCol.transform.position = LocoSphereCollider.transform.position + Vector3.up * FenderHeight;
        }

        #endregion

        #region HMD Movement and Rotation

        private void HandleHMDMovement()
        {
            delta = VRCamera.position - Chest.transform.position;
            delta.y = 0f;

            deltaHead = VRCamera.position - Head.transform.position;
            XRRig.transform.localPosition += Chest.transform.InverseTransformDirection(deltaHead.y * Vector3.down);

            if (delta.magnitude > 0.01f)
            {
                delta -= currentHeadOffset;

                Chest.MovePosition(Chest.position + delta);
                LocoSphere.MovePosition(LocoSphere.position + delta);

                XRRig.transform.localPosition -= Chest.transform.InverseTransformDirection(delta);
            }
        }

        //Do we need to expose step to the user?
        private void HandleHMDRotation()
        {
            newEulers = Mathf.MoveTowardsAngle(lastEulers, XRRig.transform.eulerAngles.y - VRCamera.transform.eulerAngles.y, step * Time.deltaTime);
            deltaEulers = (lastEulers - newEulers);

            deltaRot = Quaternion.AngleAxis(deltaEulers, Vector3.up);

            Chest.MoveRotation(Chest.rotation * deltaRot);

            XRRig.RotateAround(VRCamera.position, Vector3.up, -deltaEulers);

            lastEulers = newEulers;
        }
        #endregion

        #region Debug Objects

        private void LateUpdate()
        {
            if (renderDebugObjects)
            {
                AlignObjectWithCollider(ChestCol, d_Chest);
                AlignObjectWithCollider(LegsCol, d_Legs);
            }
        }

        private void ToggleDebugObjects(bool enabled)
        {
            d_Chest.SetActive(enabled);
            //d_Fender.SetActive(enabled);
            d_Legs.SetActive(enabled);
            d_LocoSphere.SetActive(enabled);
        }

        private void AlignObjectWithCollider(CapsuleCollider coll, GameObject gameObject)
        {
            gameObject.transform.position = coll.transform.TransformPoint(coll.center);
            gameObject.transform.localScale = new Vector3(coll.radius * 2, coll.height / 2, coll.radius * 2);
        }  
        #endregion

        #region Helper Functions

        private void StopHorizontalMomentum(Rigidbody rb)
        {
            vel = rb.velocity;
            vel.x = 0;
            vel.z = 0;
            rb.velocity = vel;
        }

        private Vector3 GetCameraInRigSpace()
        {
            return LocoSphere.transform.localPosition + Vector3.up * (localHeight - LocoSphereCollider.radius);
        }

        private Vector3 GetCameraGlobal()
        {
            return LocoSphere.position + Vector3.up * (localHeight - LocoSphereCollider.radius);
        }

        private float GetActualHeight()
        {
            return Head.transform.position.y - LocoSphere.transform.position.y + LocoSphereCollider.radius;
        }

        JointDrive drive;
        private void UpdateJointDrive(ConfigurableJoint joint)
        {
            drive.positionSpring = jointStrength;
            drive.positionDamper = jointDampener;
            drive.maximumForce = jointMaxStrength;

            joint.xDrive = joint.yDrive = joint.zDrive = drive;
            joint.xMotion = joint.zMotion = ConfigurableJointMotion.Locked;
        }

        private ConfigurableJoint SetupJoint(Rigidbody connectTo, Rigidbody connectedBody)
        {
            ConfigurableJoint joint = connectTo.gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = connectedBody;

            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = joint.connectedAnchor = Vector3.zero;

            joint.xMotion = joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;

            var drive = new JointDrive();
            drive.positionSpring = jointStrength;
            drive.positionDamper = jointDampener;
            drive.maximumForce = jointMaxStrength;

            joint.xDrive = joint.yDrive = joint.zDrive = drive;

            return joint;
        } 
        #endregion
    }
}