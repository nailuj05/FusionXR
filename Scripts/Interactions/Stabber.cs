using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public class Stabber : MonoBehaviour
    {
        public float requiredImpactVelocity;

        private ConfigurableJoint stabJoint;

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.relativeVelocity.magnitude > requiredImpactVelocity)
            {
                AttachJoint(collision.collider.gameObject);
            }
        }

        void AttachJoint(GameObject objectToStab)
        {
            stabJoint = gameObject.AddComponent<ConfigurableJoint>();

            stabJoint.angularXMotion = stabJoint.angularYMotion = stabJoint.angularZMotion = ConfigurableJointMotion.Locked;
            stabJoint.xMotion = stabJoint.yMotion = stabJoint.zMotion = ConfigurableJointMotion.Locked;

            if (objectToStab.TryGetComponent(out Rigidbody rigidbodyToStab))
            {
                stabJoint.connectedBody = rigidbodyToStab;
            }
        }
    }
}