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
        public float spring = 10f;

        public LayerMask stabbingLayers;

        private Rigidbody rb;
        private float unstabTime = 0.3f;

        private ConfigurableJoint stabJoint;
        private float stabTime;
        private GameObject stabbedObject;
        private Collider[] colliders;

        private JointDrive _drive;

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
            if (collision.relativeVelocity.magnitude > requiredImpactVelocity & Utilities.ObjectMatchesLayermask(collision.gameObject, stabbingLayers))
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


        public bool CheckColliders(Collider[] colliders)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject == stabbedObject) return true;
            }

            return false;
        }
        #endregion

        Vector3 connectedAnchor;
        float stabDistance;

        public void ApplyFriction()
        {
            connectedAnchor = stabJoint.connectedBody ? stabJoint.connectedBody.transform.TransformPoint(stabJoint.connectedAnchor) : stabJoint.connectedAnchor;
            stabDistance = Vector3.Distance(transform.TransformPoint(stabJoint.anchor), connectedAnchor);

            _drive = stabJoint.xDrive;
            _drive.positionDamper = resistance + resistance * Mathf.Pow(stabDistance, 2);
            _drive.maximumForce = 1500;
            _drive.positionSpring = spring;

            stabJoint.xDrive = stabJoint.yDrive = stabJoint.zDrive = _drive;

            stabJoint.targetPosition = transform.InverseTransformPoint(connectedAnchor);
        }
    }
}