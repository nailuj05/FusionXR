using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    /// <summary>
    /// A physics interactor to simulate axes, knifes, swords, arrows, etc..
    /// THIS ONLY WORKS ON BOXCOLLIDERS CURRENTLY
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Stabber : MonoBehaviour
    {
        public float requiredImpactVelocity;

        private Rigidbody rb;

        private float unstabTime = 0.3f;

        private ConfigurableJoint stabJoint;
        private float stabTime;
        private GameObject stabbedObject;
        private Collider[] colliders;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            colliders = GetComponents<Collider>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.relativeVelocity.magnitude > requiredImpactVelocity)
            {
                Debug.Log("Attach Joint");

                IgnoreCollisions(collision.gameObject.GetComponents<Collider>(), true);
                AttachJoint(collision.collider.gameObject);
                stabTime = Time.time;
            }
        }

        private void FixedUpdate()
        {
            if (stabbedObject)
            {
                //Check box collisions here
            }
        }

        //This is not being called if Ignore Collision is enabled
        private void OnCollisionExit(Collision collision)
        {
            if (collision.collider.gameObject == stabbedObject)
            {
                Debug.Log($"Exit {collision.collider}");
                DetachJoint();
            }
        }

        void AttachJoint(GameObject objectToStab)
        {
            stabJoint = gameObject.AddComponent<ConfigurableJoint>();

            stabJoint.angularXMotion = stabJoint.angularYMotion = stabJoint.angularZMotion = ConfigurableJointMotion.Locked;
            stabJoint.yMotion = stabJoint.zMotion = ConfigurableJointMotion.Locked;

            stabbedObject = objectToStab;

            if (objectToStab.TryGetComponent(out Rigidbody rigidbodyToStab))
            {
                stabJoint.connectedBody = rigidbodyToStab;
            }
        }

        void DetachJoint()
        {
            Debug.Log($"Try detach: {(Time.time - stabTime)}");

            if((Time.time - stabTime) > unstabTime)
            {
                Debug.Log("Detach Joint");
                IgnoreCollisions(stabbedObject.GetComponents<Collider>(), false);

                stabbedObject = null;
                Destroy(stabJoint);
            }
        }

        void IgnoreCollisions(Collider[] stabbedColliders, bool ignore)
        {
            foreach(Collider coll in colliders)
            {
                foreach (Collider stabColl in stabbedColliders)
                {
                    Physics.IgnoreCollision(coll, stabColl, ignore);
                }
            }
        }
    }
}