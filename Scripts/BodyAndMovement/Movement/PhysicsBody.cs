using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class PhysicsBody : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform Rig;
        private Transform Camera;

        [Header("Rigidbodys")]
        public Rigidbody Head;
        public Rigidbody Chest;
        public Rigidbody Legs;
        public Rigidbody LocoSphere;

        [Header("Colliders")]
        public CapsuleCollider ChestCol;
        public CapsuleCollider LegsCol;

        private Vector3 delta;

        void Start()
        {
            Camera = Player.main.head;
        }

        void FixedUpdate()
        {
            HMDMove();
        }

        void HMDMove()
        {
            delta = Camera.position - Chest.transform.position;

            if(delta.magnitude > 0.001f)
            {
                delta.y = 0f;
                //delta = delta * Time.fixedDeltaTime;

                Debug.DrawRay(Chest.position, delta, Color.red, 0.1f);

                Head.MovePosition(Head.position + delta);
                Chest.MovePosition(Chest.position + delta);
                Legs.MovePosition(Legs.position + delta);
                LocoSphere.MovePosition(LocoSphere.position + delta);

                Rig.transform.localPosition -= Chest.transform.InverseTransformDirection(delta);
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
    }
}