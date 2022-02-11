using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class PhysicsBody : CollisionAdjuster
    {
        [Header("Transforms")]
        public Transform targetHead;

        [Header("Body Settings")]
        [Range(0.1f, 0.9f)]
        public float chestPercent = 0.3f;
        [Range(0.1f, 0.9f)]
        public float legsPercent = 0.7f;

        #region Editor Stuff
        private float lastCP, lastLP;

        private void OnValidate()
        {
            if (chestPercent != lastCP)
                legsPercent = 1 - chestPercent;
            else
                chestPercent = 1 - legsPercent;

            lastCP = chestPercent;
            lastLP = legsPercent;
        }
        #endregion

        [Space]
        [Range(0f, 1f)]
        public float chestAdjustmentFactor = 0.75f;
        [Range(0f, 1f)]
        public float legAdjustmentFacotor = 0.75f;

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
            return;
            HeadJoint = SetupJoint(Chest, Head);
            HeadJoint.xMotion = HeadJoint.zMotion = ConfigurableJointMotion.Locked;

            Player.main.Rigidbody = Chest;

            //UpdateChest();
            //UpdateLegs();
            //PlaceFender();

            Head.interpolation = RigidbodyInterpolation.Interpolate;
            Chest.interpolation = RigidbodyInterpolation.Interpolate;
            Legs.interpolation = RigidbodyInterpolation.Interpolate;
            LocoSphere.interpolation = RigidbodyInterpolation.Interpolate;

            ToggleDebugObjects(renderDebugObjects);
        }

        void FixedUpdate()
        {
            Debug.DrawLine(LocoSphere.transform.position, LocoSphere.transform.position + Vector3.up * (p_localHeight * chestPercent), Color.blue);
            ChestJoint.targetPosition = new Vector3(0, -(p_localHeight * chestPercent), 0);

            HandleHMDMovement();
            //HandleHMDRotation();

            return;
            //NOTE: Head Offset can be used for jumping
            cameraPos = GetCameraGlobal() + currentHeadOffset;

            //HeadJoint.connectedAnchor = Chest.transform.InverseTransformPoint(cameraPos);
            //HeadJoint.targetPosition = Chest.transform.InverseTransformPoint(cameraPos).y * Vector3.up;

            UpdateChest();
            UpdateLegs();

            HandleHMDMovement();
            HandleHMDRotation();

            PlaceFender();
        }

        private void LateUpdate()
        {
            return;

            if (renderDebugObjects)
            {
                AlignObjectWithCollider(ChestCol, d_Chest);
                AlignObjectWithCollider(LegsCol, d_Legs);
            }
        }

        #region Chest and Legs

        private void UpdateChest()
        {
            colliderHeight = (p_localHeight * chestPercent);
            positionToReach = cameraPos + Vector3.down * colliderHeight;

            ChestJoint.connectedAnchor = ChestJoint.connectedBody.transform.InverseTransformPoint(positionToReach);

            ChestCol.height = colliderHeight;
            ChestCol.center = Vector3.up * ((colliderHeight - 0.5f) * chestAdjustmentFactor);
        }

        JointDrive drive = new JointDrive();
        private void UpdateLegs()
        {
            colliderHeight = (p_localHeight * (1 - legsPercent));

            drive.positionSpring = jointStrength;
            drive.positionDamper = jointDampener;
            drive.maximumForce = jointMaxStrength;

            LegsJoint.xDrive = LegsJoint.yDrive = LegsJoint.zDrive = drive;
            LegsJoint.anchor = Vector3.up * LocoSphereCollider.radius;
            LegsJoint.targetPosition = new Vector3(0, -(p_localHeight * legsPercent), 0);

            LegsCol.height = colliderHeight;
        } 

        private void PlaceFender()
        {
            FenderCol.transform.position = LocoSphereCollider.transform.position + Vector3.up * FenderHeight;
        }

        #endregion

        #region HMD Movement and Rotation

        private void HandleHMDMovement()
        {
            delta = p_VRCamera.position - Chest.transform.position;
            delta.y = 0f;

            if (delta.magnitude > 0.01f)
            {
                //deltaHead = p_VRCamera.position - Head.transform.position;
                //p_XRRig.transform.localPosition += Chest.transform.InverseTransformDirection(deltaHead.y * Vector3.down);

                //delta -= currentHeadOffset;

                Chest.MovePosition(Chest.position + delta);
                LocoSphere.MovePosition(LocoSphere.position + delta);

                p_XRRig.transform.localPosition -= Chest.transform.InverseTransformDirection(delta);
            }
        }

        //Do we need to expose step to the user?
        private void HandleHMDRotation()
        {
            newEulers = Mathf.MoveTowardsAngle(lastEulers, p_XRRig.transform.eulerAngles.y - p_VRCamera.transform.eulerAngles.y, step * Time.deltaTime);
            deltaEulers = (lastEulers - newEulers);

            deltaRot = Quaternion.AngleAxis(deltaEulers, Vector3.up);

            Chest.MoveRotation(Chest.rotation * deltaRot);
            //Legs.MoveRotation(Legs.rotation * deltaRot);

            p_XRRig.RotateAround(p_VRCamera.position, Vector3.up, -deltaEulers);

            lastEulers = newEulers;
        }
        #endregion

        #region Debug Objects

        private void ToggleDebugObjects(bool enabled)
        {
            d_Chest.SetActive(enabled);
            d_Fender.SetActive(enabled);
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
            return LocoSphere.transform.localPosition + Vector3.up * (p_localHeight - LocoSphereCollider.radius);
        }

        private Vector3 GetCameraGlobal()
        {
            return LocoSphere.position + Vector3.up * (p_localHeight - LocoSphereCollider.radius);
        }

        private ConfigurableJoint SetupJoint(Rigidbody connectTo, Rigidbody connectedBody)
        {
            ConfigurableJoint joint = connectTo.gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = connectedBody;

            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = joint.connectedAnchor = Vector3.zero;

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