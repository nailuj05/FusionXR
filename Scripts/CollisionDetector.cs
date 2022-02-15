using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fusion.XR
{
    public class CollisionDetector : MonoBehaviour
    {
        public delegate void Coll();
        public event Coll CollisionEnter;
        public event Coll CollisionExit;

        void Start()
        {
            if(!TryGetComponent(out Collider coll))
            {
                Debug.LogError($"No collider found on {gameObject.name}");
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            CollisionEnter();
        }

        private void OnCollisionExit(Collision collision)
        {
            CollisionExit();
        }
    } 
}
