using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fusion.XR
{
    public class DistanceReader : MonoBehaviour
    {
        [Tooltip("The position from which the distance is measured (parent space)")]
        [SerializeField] private Vector3 initialPosition;
        private Vector3 initialPositionGlobal => transform.parent.TransformPoint(initialPosition);

        [ReadOnly] public float distance;

        public UnityEvent OnDistanceChanged;

        private float lastDistance;

        private void Awake()
        {
            lastDistance = distance;
        }

        private void Update()
        {
            distance = Vector3.Distance(transform.position, initialPositionGlobal);

            if(distance != lastDistance)
            {
                OnDistanceChanged.Invoke();
            }

            lastDistance = distance;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(initialPositionGlobal, Vector3.one * 0.05f);
        }
#endif
    } 
}
