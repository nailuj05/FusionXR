using System.Linq;
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
        [Header("Stabber")]
        [Tooltip("The collider that will be used for stabbing, if not assigned the script will grab the collider automatically")]
        public BoxCollider stabCollider;

        public float requiredImpactVelocity;

        private Rigidbody rb;

        private float unstabTime = 0.3f;

        private ConfigurableJoint stabJoint;
        private float stabTime;
        private GameObject stabbedObject;

        /// <summary>
        /// All colliders of the object
        /// </summary>
        private Collider[] colliders;

        private void Awake()
        {
            try
            {
                rb = GetComponent<Rigidbody>();
                colliders = GetComponents<Collider>();
                stabCollider = GetComponent<BoxCollider>();
            }
            catch 
            {
                Debug.LogError($"Object not setup correctly, missing Rigidbody/Collider/Box(Stab)Collider");
            }
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

        //Maybe do this on a longer timestamp for performance (?)
        private void FixedUpdate()
        {
            if (stabbedObject)
            {
                Collider[] hitColliders = CheckBoxCollider(transform, stabCollider);

                //TODO: Check if hitColliders Contains the stabbed objects collider

                //Check if we hit more than one collider (the own collider)
                if (hitColliders.Length > 1)
                {
                    //Apply Friction Force
                }
                else
                {
                    Debug.Log("Exit");
                    DetachJoint();
                }
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

        public static Collider[] CheckBoxCollider(Transform transform, BoxCollider boxCollider)
        {
            Vector3 boxCenter = transform.TransformPoint(boxCollider.center);

            return Physics.OverlapBox(boxCenter, boxCollider.size / 2, transform.rotation);
        }
    }
}