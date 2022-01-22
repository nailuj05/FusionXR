using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class PhysicsBody : CollisionAdjuster
    {
        [Header("Transforms")]
        public Transform targetHead;
        public Rigidbody debugCylinder;

        [Header("Body Settings")]
        [Range(0.1f, 0.9f)]
        public float chestPercent;
        [Range(0.1f, 0.9f)]
        public float legsPercent;

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

        [Header("Joint Settings")]
        public float jointStrength = 20000;
        public float jointDampener = 250;

        [Header("Tracking Settings")]
        public Vector3 headOffset;
        public float neckFactor;
        private Vector3 currentHeadOffset => Head.transform.TransformVector(headOffset);

        public float FenderHeight = 0.1f;

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
        public ConfigurableJoint HeadJoint;
        public ConfigurableJoint ChestJoint;
        public ConfigurableJoint LegJoint;


        private void Start()
        {
            HeadJoint = SetupJoint(Chest, Head);
        }

        Vector3 cameraPos;
        void FixedUpdate()
        {
            //Head Offset still needed? 
            cameraPos = GetCameraGlobal() + currentHeadOffset * neckFactor;

            //This is only debug
            targetHead.position = cameraPos;

            //TODO: Do this with anchors instead

            HeadJoint.targetPosition = Chest.transform.InverseTransformPoint(cameraPos);

            UpdateChest();
            UpdateLegs();

            HandleHMDMovement();
            HandleHMDRotation();
            PlaceFender();
        }

        Vector3 positionToReach;
        void UpdateChest()
        {
            positionToReach = cameraPos + Vector3.down * (p_localHeight * chestPercent);

            ChestJoint.connectedAnchor = ChestJoint.connectedBody.transform.InverseTransformPoint(positionToReach);
        }

        void UpdateLegs()
        {
            positionToReach = cameraPos + Vector3.down * (p_localHeight * (1 - legsPercent));

            var tar = -LegJoint.connectedBody.transform.InverseTransformPoint(positionToReach);
            tar.x = tar.z = 0;
            LegJoint.anchor = tar;
        }

        void AdjustJointAnchor(ConfigurableJoint joint, float percentToReach, Vector3 cameraPosition)
        {
            positionToReach = cameraPos + Vector3.down * (p_localHeight * percentToReach);

            //Debug.DrawLine(LocoSphere.position + Vector3.right * 0.2f + Vector3.down * LocoSphereCollider.radius, positionToReach + Vector3.right * 0.2f, Color.red);
            //Debug.DrawLine(joint.transform.position + Vector3.right * 0.1f, joint.transform.TransformPoint(joint.transform.InverseTransformPoint(positionToReach)) + Vector3.right * 0.1f, Color.green);
            //Debug.DrawLine(joint.transform.position, joint.transform.TransformPoint(joint.anchor), Color.blue);

            //Debug.Log($"{p_localHeight} {joint.connectedBody.transform.InverseTransformPoint(positionToReach).y}");

            joint.connectedAnchor = joint.connectedBody.transform.InverseTransformPoint(positionToReach);
        }

        void PlaceFender()
        {
            FenderCol.transform.position = LocoSphereCollider.transform.position + Vector3.up * FenderHeight;
        }

        Vector3 delta;
        Vector3 deltaHead;
        void HandleHMDMovement()
        {
            delta = p_VRCamera.position - Chest.transform.position;

            if(delta.magnitude > 0.001f)
            {
                deltaHead = p_VRCamera.position - Head.transform.position;
                p_XRRig.transform.localPosition += Chest.transform.InverseTransformDirection(deltaHead.y * Vector3.down);

                delta.y = 0f;
                delta -= currentHeadOffset;

                Chest.MovePosition(Chest.position + delta);
                Legs.MovePosition(Legs.position + delta);
                LocoSphere.MovePosition(LocoSphere.position + delta);

                StopXZ(Chest);
                StopXZ(Legs);
                StopXZ(LocoSphere);

                p_XRRig.transform.localPosition -= Chest.transform.InverseTransformDirection(delta);
            }
        }

        //Can this run in LateUpdate
        //Can FenderPlacement Run in LateUpdate?
        //Do we need to expose step to the user?
        Quaternion deltaRot;
        private float step = 500;
        float lastEulers, newEulers, deltaEulers, targetEulers;
        void HandleHMDRotation()
        {
            newEulers = Mathf.MoveTowardsAngle(lastEulers, p_XRRig.transform.eulerAngles.y - p_VRCamera.transform.eulerAngles.y, step*Time.deltaTime);
            deltaEulers = (lastEulers - newEulers);

            deltaRot = Quaternion.AngleAxis(deltaEulers, Vector3.up);

            Chest.MoveRotation(Chest.rotation * deltaRot);
            Legs.MoveRotation(Legs.rotation * deltaRot);

            p_XRRig.RotateAround(p_VRCamera.position, Vector3.up, -deltaEulers);

            lastEulers = newEulers;
        }

        Vector3 vel;
        void StopXZ(Rigidbody rb)
        {
            vel = rb.velocity;
            vel.x = 0;
            vel.z = 0;
            rb.velocity = vel;
        }

        Vector3 GetCameraInRigSpace()
        {
            return LocoSphere.transform.localPosition + Vector3.up * (p_localHeight - LocoSphereCollider.radius);
        }

        Vector3 GetCameraGlobal()
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