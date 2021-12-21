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

        public float resistance = 100f;

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
            if (collision.relativeVelocity.magnitude > requiredImpactVelocity)
            {
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

                if (CheckColliders(hitColliders))
                {
                    ApplyFriction();
                }
                else
                {
                    DetachJoint();
                }
            }
        }

        #region Joints
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
            if ((Time.time - stabTime) > unstabTime)
            {
                IgnoreCollisions(stabbedObject.GetComponents<Collider>(), false);

                stabbedObject = null;
                Destroy(stabJoint);
            }
        }
        #endregion

        #region Collisions
        void IgnoreCollisions(Collider[] stabbedColliders, bool ignore)
        {
            foreach (Collider coll in colliders)
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

        public bool CheckColliders(Collider[] colliders)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject == stabbedObject) return true;
            }

            return false;
        } 
        #endregion

        public void ApplyFriction()
        {
            Vector3 connectedAnchor = stabJoint.connectedBody ? stabJoint.connectedBody.transform.TransformPoint(stabJoint.connectedAnchor) : stabJoint.connectedAnchor;
            float stabDistance = Vector3.Distance(transform.TransformPoint(stabJoint.anchor), connectedAnchor);
            Vector3 friction = -rb.velocity * resistance * stabDistance;

            rb.AddForce(friction * rb.mass);

            stabJoint.connectedBody?.AddForce(-friction * stabJoint.connectedBody.mass);
        }
    }
}