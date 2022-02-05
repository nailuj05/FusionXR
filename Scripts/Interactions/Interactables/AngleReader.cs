using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fusion.XR
{
    public class AngleReader : MonoBehaviour
    {
        [Tooltip("The axis around which the object rotates (parent space)")]
        [SerializeField] private Vector3 rotationAxis = new Vector3(0, 0, 1);
        private Vector3 globalAxis => transform.parent.TransformVector(rotationAxis);

        [Tooltip("The direction from which the angle is measured ")]
        [SerializeField] private Vector3 direction = new Vector3(1, 0, 0);
        private Vector3 globalDirection => transform.TransformVector(direction);
        private Vector3 startDirection;

        [SerializeField] private float angleToReach;
        [SerializeField] private float allowance = 10;

        [ReadOnly]
        [SerializeField] private float currentAngle;

        private bool isReached;

        public UnityEvent OnAngleReached;
        public UnityEvent OnAngleLeft;

        private void OnValidate()
        {
            if(angleToReach > 180)
            {
                angleToReach -= 360;
            }
        }

        private void Awake()
        {
            startDirection = transform.parent.InverseTransformVector(globalDirection);
        }

        private void Update()
        {
            currentAngle = Vector3.SignedAngle(startDirection, transform.parent.InverseTransformVector(globalDirection), rotationAxis);

            if(Mathf.Abs(angleToReach - currentAngle) <= allowance)
            {
                if (!isReached)
                {
                    OnAngleReached.Invoke();
                    isReached = true;
                }
            }
            else
            {
                if (isReached)
                {
                    OnAngleLeft.Invoke();
                    isReached = false;
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, globalAxis.normalized * 0.3f);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, globalDirection.normalized * 0.3f);

            //Long boys
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.parent.TransformVector(Quaternion.Euler(rotationAxis * (angleToReach + allowance)) * direction).normalized * 0.3f);
            Gizmos.DrawRay(transform.position, transform.parent.TransformVector(Quaternion.Euler(rotationAxis * (angleToReach - allowance)) * direction).normalized * 0.3f);
        }
#endif
    }
}