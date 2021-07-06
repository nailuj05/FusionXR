using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class BodyTracker : MonoBehaviour
    {
        [SerializeField] private Transform root, head;
        [SerializeField] private Vector3 positionOffset, rotationOffset, headBodyOffset;

        private void LateUpdate()
        {
            root.position = transform.position + headBodyOffset;
            root.forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;

            transform.position = head.TransformPoint(positionOffset);
            transform.rotation = head.rotation * Quaternion.Euler(rotationOffset);
        }
    }
}
