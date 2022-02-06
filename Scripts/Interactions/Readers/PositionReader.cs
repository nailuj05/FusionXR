using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fusion.XR
{
    public class PositionReader : MonoBehaviour
    {
        [Tooltip("The position this object should reach (parent space)")]
        [SerializeField] private Vector3 positionToReach;
        private Vector3 positionToReachGlobal => transform.parent.TransformPoint(positionToReach);

        [SerializeField] private float allowance = 0.05f;

        public UnityEvent OnPositionReached;
        public UnityEvent OnPositionLeft;

        private bool isReached;

        private void Update()
        {
            if (Vector3.Distance(transform.position, positionToReachGlobal) < allowance)
            {
                if (!isReached)
                {
                    isReached = true;
                    OnPositionReached.Invoke();
                }
            }
            else if (isReached)
            {
                isReached = false;
                OnPositionLeft.Invoke();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(positionToReachGlobal, allowance);
        }
#endif
    }
}