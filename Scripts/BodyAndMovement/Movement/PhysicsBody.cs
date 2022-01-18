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

        public Vector3 headOffset;
        public float neckFactor;
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

            HeadJoint.targetPosition = Chest.transform.InverseTransformPoint(cameraPos);
            //ChestJoint.targetPosition = Legs.transform.InverseTransformPoint(cameraInRigSpace - Vector3.up * (cameraInRigSpace.y * chestPercent - LocoSphereCollider.radius * 2));

            //Debug.DrawRay(Chest.position, Chest.transform.InverseTransformPoint(cameraPos + currentHeadOffset), Color.blue);
            //Debug.DrawLine(LocoSphere.position + Vector3.down * LocoSphereCollider.radius, cameraPos, Color.green);
            //Debug.DrawRay(cameraPos, currentHeadOffset, Color.green);
            //Debug.DrawLine(Chest.position, p_VRCamera.position, Color.red);

            HMDMove();
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
                delta -= currentHeadOffset;

                //Head.MovePosition(Head.position + delta);
                Chest.MovePosition(Chest.position + delta);
                Legs.MovePosition(Legs.position + delta);
                LocoSphere.MovePosition(LocoSphere.position + delta);

                StopXZ(Chest);
                StopXZ(Legs);
                StopXZ(LocoSphere);

                p_XRRig.transform.localPosition -= Chest.transform.InverseTransformDirection(delta);
            }
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