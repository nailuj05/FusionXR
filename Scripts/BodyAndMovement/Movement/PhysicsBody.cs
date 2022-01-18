using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class PhysicsBody : CollisionAdjuster
    {
        [Header("Transforms")]
        public Transform targetHead;

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
            //This is only debug
            cameraPos = GetCameraGlobal() + currentHeadOffset * neckFactor;
            targetHead.position = cameraPos;

            //TODO: Do this with anchors instead

            //HeadJoint.targetPosition = Chest.transform.InverseTransformPoint(cameraPos);
            //ChestJoint.targetPosition = Legs.transform.InverseTransformPoint(cameraInRigSpace - Vector3.up * (cameraInRigSpace.y * chestPercent - LocoSphereCollider.radius * 2));

            HandleHMDMovement();
            PlaceFender();
        }

        void PlaceFender()
        {
            FenderCol.transform.position = LocoSphereCollider.transform.position + Vector3.up * FenderHeight;
        }

        Vector3 delta;
        Vector3 deltaHead;
        Vector3 deltaRot;
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

            deltaRot = Vector3.zero;
            deltaRot = Quaternion.LookRotation(p_VRCamera.forward, Vector3.up).eulerAngles;
            deltaRot.x = Chest.transform.eulerAngles.x;
            deltaRot.z = Chest.transform.eulerAngles.z;
            //Chest.MoveRotation(Quaternion.Euler(deltaRot));
            //Chest.rotation = Quaternion.Euler(deltaRot);

            //Maybe only rotate collider not whole chest
            deltaRot = Vector3.ProjectOnPlane(p_VRCamera.forward, Vector3.up);
            var rot = Quaternion.LookRotation(deltaRot, Vector3.up);
            Legs.MoveRotation(rot);
            Chest.MoveRotation(rot);

            var deltaAngle = p_XRRig.eulerAngles.y - Chest.transform.eulerAngles.y;
            p_XRRig.RotateAround(p_VRCamera.position, Vector3.up, -deltaAngle);
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