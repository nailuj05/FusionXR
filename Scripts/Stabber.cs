using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public class Stabber : MonoBehaviour
    {
        public Transform stabColliderCenter;
        public Vector3 stabColliderDimensions = Vector3.one / 20;

        public float requiredImpactVelocity;

        private float lastImpactVelocity;

        private GameObject stabbedObject;
        private ConfigurableJoint stabJoint;
        private Collider[] hitColliders;

        //private void OnCollisionEnter(Collision collision)
        //{
        //    //Calculate Impact Velocity relative to knifes stab axis
        //    lastImpactVelocity = Vector3.Project(collision.relativeVelocity, stabColliderCenter.forward).sqrMagnitude;

        //    //Check if impact velocity is large enough
        //    if (lastImpactVelocity >= requiredImpactVelocity)
        //    {
        //        //Check if there is a collision at the stab collider
        //        hitColliders = Physics.OverlapBox(stabColliderCenter.position, stabColliderDimensions, stabColliderCenter.rotation);

        //        if (hitColliders.Length > 0)
        //        {
        //            //Attach joint to the first collider (calculating the closest is to performance heavy)
        //            AttachJoint(hitColliders[0].gameObject);
        //        }
        //    }
        //}

        void AttachJoint(GameObject objectToStab)
        {
            //Add the Joint
            stabJoint = objectToStab.AddComponent<ConfigurableJoint>();

            //If the object has a rigidbody
            if (objectToStab.TryGetComponent(out Rigidbody rigidbodyToStab))
            {
                stabJoint.connectedBody = rigidbodyToStab;
                stabJoint.anchor = objectToStab.transform.TransformPoint(stabColliderCenter.position);
            }
            //If the object has no rigidbody
            else
            {
                stabJoint.anchor = objectToStab.transform.TransformPoint(stabColliderCenter.position);
            }
        }

        public void SetupStabCollider()
        {
            if (stabColliderCenter)
                return;

            GameObject coll = new GameObject("StabCollider");
            coll.transform.parent = transform;
            coll.transform.localPosition = Vector3.zero;

            stabColliderCenter = coll.transform;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 0, 1, 0.4f);
            Gizmos.DrawCube(stabColliderCenter.position, stabColliderDimensions);
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Stabber))] [CanEditMultipleObjects]
    public class StabberEditor : Editor
    {
        Stabber stabber;

        private void Awake()
        {
            stabber = (Stabber)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if(GUILayout.Button("AddStabCollider"))
            {
                stabber.SetupStabCollider();
            }
        }
    }
#endif
}