using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class PhysicsBody : CollisionAdjuster
    {
        [Header("Transforms")]
        public Transform targetHead;

        public float jointStrength = 20000;
        public float jointDampener = 250;

        [Header("Body Settings")]
        [Range(0.1f, 0.9f)]
        public float chestPercent = 0.5f;
        [Range(0.1f, 0.9f)]
        public float legPercent = 0.5f;

        [Header("Rigidbodys")]
        public Rigidbody Head;
        public Rigidbody Chest;
        public Rigidbody Legs;
        public Rigidbody LocoSphere;

        [Header("Colliders")]
        public SphereCollider HeadCol;
        public CapsuleCollider ChestCol;
        public CapsuleCollider LegsCol;
        public SphereCollider LocoSphereCollider;

        [Header("Joints")]
        public ConfigurableJoint HeadJoint;
        public ConfigurableJoint ChestJoint;
        public ConfigurableJoint LegJoint;

        #region Editor Stuff
        private float lastCP, lastLP;

        private void OnValidate()
        {
            if (chestPercent != lastCP)
                legPercent = 1 - chestPercent;
            else
                chestPercent = 1 - legPercent;

            lastCP = chestPercent;
            lastLP = legPercent;
        }  
        #endregion

        private void Start()
        {
            HeadJoint = SetupJoint(Chest, Head);
            ChestJoint = SetupJoint(Legs, Chest);

            //Init Legs
            legFactor = LegsCol.height * 2;
            initialLegCenter = LegsCol.center;

            //Init Chest
            chestFactor = ChestCol.height * 2;
        }

        Vector3 cameraInRigSpace;
        void FixedUpdate()
        {
            //This is only debug
            cameraInRigSpace = GetCameraInRigSpace();
            targetHead.position = cameraInRigSpace;

            HeadJoint.targetPosition = Chest.transform.InverseTransformPoint(cameraInRigSpace);
            ChestJoint.targetPosition = Legs.transform.InverseTransformPoint(cameraInRigSpace - Vector3.up * (cameraInRigSpace.y * chestPercent - LocoSphereCollider.radius * 2));

            HMDMove();
            UpdateLegs();
            UpdateChest();
        }

        float chestFactor;
        Vector3 initialChestCenter;
        void UpdateChest()
        {
            ChestCol.height = chestFactor * chestPercent;
            ChestCol.center = Vector3.up * (chestFactor * chestPercent * 0.5f - HeadCol.radius);
        }

        float legFactor;
        Vector3 initialLegCenter;
        void UpdateLegs()
        {
            LegsCol.height = legFactor * legPercent;
            LegsCol.center = initialLegCenter + Vector3.up * (legFactor * legPercent * 0.5f - LocoSphereCollider.radius * 2);
        }

        Vector3 delta;
        Vector3 deltaHead;
        void HMDMove()
        {
            delta = p_VRCamera.position - Chest.transform.position;

            if(delta.magnitude > 0.001f)
            {
                deltaHead = p_VRCamera.position - Head.transform.position;
                p_XRRig.transform.localPosition += Chest.transform.InverseTransformDirection(deltaHead.y * Vector3.down);

                delta.y = 0f;

                Debug.DrawRay(Chest.position, delta, Color.red, 0.1f);

                Head.MovePosition(Head.position + delta);
                Chest.MovePosition(Chest.position + delta);
                Legs.MovePosition(Legs.position + delta);
                LocoSphere.MovePosition(LocoSphere.position + delta);

                p_XRRig.transform.localPosition -= Chest.transform.InverseTransformDirection(delta);
            }
        }

        Vector3 GetCameraInRigSpace()
        {
            return LocoSphere.position + Vector3.up * (p_localHeight - LocoSphereCollider.radius);
        }

        ConfigurableJoint SetupJoint(Rigidbody connectTo, Rigidbody connectedBody)
        {
            ConfigurableJoint joint = connectTo.gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = connectedBody;

            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = joint.connectedAnchor = Vector3.zero;

            joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;

            var drive = new JointDrive();
            drive.positionSpring = jointStrength;
            drive.positionDamper = jointDampener;
            drive.maximumForce = Mathf.Infinity;

            joint.xDrive = joint.yDrive = joint.zDrive = drive;

            return joint;
        }
    }
}