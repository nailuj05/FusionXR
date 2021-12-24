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
        public Axis axis = Axis.Z;

        [Tooltip("The collider that will be used for stabbing, if not assigned the script will grab the collider automatically")]
        public BoxCollider stabCollider;

        public float requiredImpactVelocity;
        public float impactAngle = 15f;

        public float resistance = 100f;
        public float spring = 10f;

        public LayerMask stabbingLayers;

        public Rigidbody rb { get; private set; }
        public float unstabTime { get; private set; } = 0.3f;

        public Collider[] colliders { get; private set; }

        public List<Stab> stabs { get; set; } = new List<Stab>();

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
                if (MatchAxis(collision.relativeVelocity))
                {
                    var stab = new Stab(this, collision.gameObject);
                    stab.StartStab();
                    stabs.Add(stab);

                    //Debug.Log($"Added new Stab {stab}, Total stabs: {stabs.Count}");
                }
            }
        }

        //Maybe do this on a longer timestamp for performance (?)
        private void FixedUpdate()
        {
            //Iterate backwards through the stabs so we can remove them if needed without breaking the loop
            for(int i = stabs.Count - 1; i >= 0; i--)
            {
                stabs[i].UpdateStab();
            }
        }

        private bool MatchAxis(Vector3 impact)
        {
            var globalAxis = transform.TransformDirection(GetAxisVector(axis));

            return Vector3.Angle(globalAxis, -impact) < impactAngle;
        }

        static Vector3 GetAxisVector(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return Vector3.right;

                case Axis.Y:
                    return Vector3.up;

                case Axis.Z:
                    return Vector3.forward;

                default:
                    return Vector3.zero;
            }
        }
    }

    /// <summary>
    /// A class to contain all the data and logic for stabs. 
    /// Controlled and managed by the Stabber.
    /// </summary>
    public class Stab
    {
        private Stabber stabber;

        private ConfigurableJoint stabJoint;
        private float stabTime;
        private GameObject stabbedObject;

        private JointDrive _drive;

        public Stab(Stabber stabber, GameObject stabbedObject)
        {
            this.stabber = stabber;
            this.stabbedObject = stabbedObject;
        }

        public void StartStab()
        {
            IgnoreCollisions(stabbedObject.GetComponents<Collider>(), true);

            AttachJoint(stabbedObject);

            stabTime = Time.time;
        }

        public void UpdateStab()
        {
            Collider[] hitColliders = Utilities.CheckBoxCollider(stabber.transform, stabber.stabCollider);

            if (ContainsStabbedObject(hitColliders))
            {
                ApplyFriction();
            }
            else
            {
                TryEndStab();
            }
        }

        public void TryEndStab()
        {
            if ((Time.time - stabTime) > stabber.unstabTime)
            {
                IgnoreCollisions(stabbedObject.GetComponents<Collider>(), false);

                DetachJoint();

                stabber.stabs.Remove(this);
            }
        }

        #region Joints
        void AttachJoint(GameObject objectToStab)
        {
            stabJoint = stabber.gameObject.AddComponent<ConfigurableJoint>();

            stabJoint.angularXMotion = stabJoint.angularYMotion = stabJoint.angularZMotion = ConfigurableJointMotion.Locked;

            LockMotionAndAxis(stabber.axis);

            stabbedObject = objectToStab;

            if (objectToStab.TryGetComponent(out Rigidbody rigidbodyToStab))
            {
                stabJoint.connectedBody = rigidbodyToStab;
            }
        }

        void DetachJoint()
        {
            stabbedObject = null;
            GameObject.Destroy(stabJoint);
        }

        void LockMotionAndAxis(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    stabJoint.yMotion = stabJoint.zMotion = ConfigurableJointMotion.Locked;
                    stabJoint.axis = Vector3.right;
                    break;

                case Axis.Y:
                    stabJoint.xMotion = stabJoint.zMotion = ConfigurableJointMotion.Locked;
                    stabJoint.axis = Vector3.forward;
                    break;

                case Axis.Z:
                    stabJoint.xMotion = stabJoint.yMotion = ConfigurableJointMotion.Locked;
                    stabJoint.axis = Vector3.up;
                    break;
            }
        }
        #endregion

        #region Collisions
        void IgnoreCollisions(Collider[] stabbedColliders, bool ignore)
        {
            foreach (Collider coll in stabber.colliders)
            {
                foreach (Collider stabColl in stabbedColliders)
                {
                    Physics.IgnoreCollision(coll, stabColl, ignore);
                }
            }
        }

        bool ContainsStabbedObject(Collider[] colliders)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject == stabbedObject) return true;
            }

            return false;
        }
        #endregion

        #region Friction
        Vector3 connectedAnchor;
        float stabDistance;

        void ApplyFriction()
        {
            connectedAnchor = stabJoint.connectedBody ? stabJoint.connectedBody.transform.TransformPoint(stabJoint.connectedAnchor) : stabJoint.connectedAnchor;
            stabDistance = Vector3.Distance(stabber.transform.TransformPoint(stabJoint.anchor), connectedAnchor);

            _drive = stabJoint.xDrive;
            _drive.positionDamper = stabber.resistance + stabber.resistance * Mathf.Pow(stabDistance, 2);
            _drive.maximumForce = 1500;
            _drive.positionSpring = stabber.spring;

            stabJoint.xDrive = stabJoint.yDrive = stabJoint.zDrive = _drive;

            stabJoint.targetPosition = stabber.transform.InverseTransformPoint(connectedAnchor);
        }
        #endregion
    }
}