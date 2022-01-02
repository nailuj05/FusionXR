using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class PhysicsBody : CollisionAdjuster
    {
        [Header("Transforms")]
        public Transform targetHead;

        [Header("Rigidbodys")]
        public Rigidbody Head;
        public Rigidbody Chest;
        public Rigidbody Legs;
        public Rigidbody LocoSphere;

        [Header("Colliders")]
        public CapsuleCollider ChestCol;
        public CapsuleCollider LegsCol;
        public SphereCollider LocoSphereCollider;

        [Header("Joints")]
        public ConfigurableJoint HeadJoint;
        public ConfigurableJoint LegJoint;
        public ConfigurableJoint LocoJoint;

        private Vector3 delta;

        private Vector3 localCameraPos;

        public override void UpdateCollision(float p_height, Vector3 p_localCameraPosition, float p_CollisionRadius)
        {
            localCameraPos = p_localCameraPosition;
        }

        void FixedUpdate()
        {
            targetHead.position = GetCameraInChestSpace();

            HMDMove();
        }

        void HMDMove()
        {
            delta = p_VRCamera.position - Chest.transform.position;

            if(delta.magnitude > 0.001f)
            {
                delta.y = 0f;
                //delta = delta * Time.fixedDeltaTime;

                Debug.DrawRay(Chest.position, delta, Color.red, 0.1f);

                Head.MovePosition(Head.position + delta);
                Chest.MovePosition(Chest.position + delta);
                Legs.MovePosition(Legs.position + delta);
                LocoSphere.MovePosition(LocoSphere.position + delta);

                p_XRRig.transform.localPosition -= Chest.transform.InverseTransformDirection(delta);
                //Camera.transform.position -= delta;

                //TODO: Is this needed?
                StopXZ(Head);
                StopXZ(Chest);
                StopXZ(Legs);
                StopXZ(LocoSphere);
            }
        }

        private Vector3 vel;

        void StopXZ(Rigidbody rb)
        {
            vel = rb.velocity;
            vel.Set(0, vel.y, 0);
            rb.velocity = vel;
        }

        Vector3 GetCameraInChestSpace()
        {
            Debug.DrawRay(Vector3.zero, LocoSphere.transform.TransformPoint(localCameraPos - Vector3.up * LocoSphereCollider.radius), Color.red);
            Debug.DrawRay(Vector3.zero, Chest.transform.InverseTransformPoint(LocoSphere.transform.TransformPoint(localCameraPos - Vector3.up * LocoSphereCollider.radius)), Color.blue);

            return Chest.transform.InverseTransformPoint(LocoSphere.transform.TransformPoint(localCameraPos - Vector3.up * LocoSphereCollider.radius));
        }
    }
}