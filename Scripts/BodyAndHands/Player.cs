using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    public class Player : MonoBehaviour
    {
        public static Player main;

        public FusionXRHand LeftHand;
        public FusionXRHand RightHand;

        public Movement movement;
        public CollisionAdjuster collisionAdjuster;

        [HideInInspector]
        public Rigidbody rigidbody;

        private void Awake()
        {
            main = this;
            rigidbody = GetComponent<Rigidbody>();
        }
    }
}
